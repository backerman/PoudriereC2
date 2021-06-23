module Facefault.PoudriereC2.Database

open FSharp.Data.LiteralProviders
open FSharp.Data.Sql
open Npgsql

[<Literal>]
let ConnectionString =
    Env.PostgresConnection.Value

/// The schemas containing database objects used by this application.
[<Literal>]
let Owner = "poudrierec2"

[<Literal>]
let SchemaPath = __SOURCE_DIRECTORY__ + "/schema.json"

type DB = SqlDataProvider<
            DatabaseVendor=Common.DatabaseProviderTypes.POSTGRESQL,
            ConnectionString=ConnectionString,
            UseOptionTypes=true,
            // ContextSchemaPath=SchemaPath,
            Owner=Owner>

type DatabaseError =
    | NoError
    | ForeignKeyViolation
    | UniqueViolation
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
                        | PostgresErrorCodes.UniqueViolation ->
                            UniqueViolation
                        | _ -> Unknown ex.SqlState
                    | _ -> Unknown e.Message
        }


// DB.GetDataContext().``Design Time Commands``.SaveContextSchema.

/// A type to contain results from a database call.