module Facefault.PoudriereC2.Database

open FSharp.Data.LiteralProviders
open FSharp.Data.Sql
open Npgsql
open System
open Microsoft.Extensions.Logging
open System.Net

[<Literal>]
let ConnectionString =
    Env.PostgresConnection.Value

/// The schemas containing database objects used by this application.
[<Literal>]
let Owner = "poudrierec2"

type DB = SqlDataProvider<
            DatabaseVendor=Common.DatabaseProviderTypes.POSTGRESQL,
            ConnectionString=ConnectionString,
            UseOptionTypes=true,
            Owner=Owner>

type DatabaseError =
    | NoError
    | ForeignKeyViolation of PostgresException
    | UniqueViolation of PostgresException
    | Unknown of Exception

    static member FromQuery q =
        async {
            let! opResult = Async.Catch q
            return
                match opResult with
                | Choice1Of2 _ -> NoError
                | Choice2Of2 e ->
                    match e.InnerException with
                    | :? PostgresException as ex ->
                        match ex.SqlState with
                        | PostgresErrorCodes.ForeignKeyViolation ->
                            ForeignKeyViolation ex
                        | PostgresErrorCodes.UniqueViolation ->
                            UniqueViolation ex
                        | _ -> Unknown e
                    | _ -> Unknown e
        }

    /// Handle a DatabaseError, returning a FunctionResult.
    member dbError.Handle
        (log: ILogger<obj>,
         failureMessage: string,
         [<ParamArray>]failureMessageArgs: obj[])
            : {| httpCode: HttpStatusCode; result: FunctionResult |} =
        match dbError with
        | NoError -> {| httpCode = HttpStatusCode.OK; result = OK |}
        | ForeignKeyViolation exn ->
            let userError =
                match exn.ColumnName with
                | null -> "A referenced row does not exist."
                | x -> $"The value of ${x} refers to a nonexistent row."
            log.LogError(exn, $"${failureMessage}", failureMessageArgs)
            {| httpCode = HttpStatusCode.BadRequest; result = Error userError |}
        | UniqueViolation exn ->
            let userError =
                match exn.ColumnName with
                | null -> "This row is a duplicate."
                | x -> $"There is already a row for this ${x}."
            log.LogError(exn, failureMessage, failureMessageArgs)
            {| httpCode = HttpStatusCode.BadRequest; result = Error userError |}
        | Unknown exn ->
            // Assume this error contains sensitive data.
            log.LogError(exn, failureMessage, failureMessageArgs)
            {| httpCode = HttpStatusCode.InternalServerError
               result = Error "An internal error occurred." |}