module Facefault.PoudriereC2.Database

open FSharp.Data.LiteralProviders
open FSharp.Data.Sql
open System
open Npgsql

[<Literal>]
let connectionString =
    Env.PostgresConnection.Value

/// The schemas containing database objects used by this application.
[<Literal>]
let owner = "poudrierec2"

type DB = SqlDataProvider<
            DatabaseVendor=Common.DatabaseProviderTypes.POSTGRESQL,
            ConnectionString=connectionString,
            UseOptionTypes=true,
            Owner=owner>

/// A type to contain results from a database call.
type DatabaseError =
    | NoError
    | ForeignKeyViolation
    | Unknown of string

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
                            ForeignKeyViolation
                        | _ -> Unknown ex.SqlState
                    | _ -> Unknown e.Message
        }
