module Facefault.PoudriereC2.Database

open FSharp.Data.LiteralProviders
open FSharp.Data.Sql
open Npgsql
open System

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
