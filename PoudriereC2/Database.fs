module Facefault.PoudriereC2.Database

open FSharp.Data.LiteralProviders
open FSharp.Data.Sql

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
