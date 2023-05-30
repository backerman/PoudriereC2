namespace PoudriereC2.Tests

open Dapper
open NUnit.Framework
open Npgsql
open Facefault.PoudriereC2
open System
open System.IO

// Inspired by https://blog.sanderaernouts.com/database-testing-with-nunit
// but required a ton of changes for F#.

/// Set up a database for NUnit fixtures.
type Database(ctx: TestContext) =
    let randomPostfix = ctx.Random.GetString(6, "abcdefghijklmnopqrstuvw0123456789")

    let shortClassName =
        ctx.Test.ClassName
            .Substring(ctx.Test.ClassName.LastIndexOf(".", StringComparison.Ordinal) + 1)
            .Replace("+", "_")

    // Connection string, without the database part; the database won't exist yet.
    let connStr =
        let cs = Environment.GetEnvironmentVariable("PostgresConnection")
        let builder = NpgsqlConnectionStringBuilder(cs)
        builder.Remove("Database") |> ignore
        builder.ToString()

    /// The name of the database to create for this test.
    member val Name = $"{shortClassName}_{randomPostfix}".ToLowerInvariant() with get
    member val ConnectionString = connStr with get

    interface IDisposable with
        member _.Dispose() = ()

type TestScope() =
    let mutable isInitalized: bool = false
    let database = new Database(TestContext.CurrentContext)

    let dbCreateDropDataSource =
        NpgsqlDataSourceBuilder(database.ConnectionString).Build()

    let workDataSource =
        let csb = NpgsqlConnectionStringBuilder(database.ConnectionString)
        csb.Database <- database.Name
        NpgsqlDataSourceBuilder(csb.ToString()).Build()

    // The NpgsqlDataSource to use for tests within this scope.
    member _.DataSource =
        if isInitalized then
            workDataSource
        else
            dbCreateDropDataSource

    member private this.loadSql(filename: string) =
        async {
            let cmdText: string = filename |> File.ReadAllText
            let cmd = this.DataSource.CreateCommand cmdText
            let! numRows = cmd.ExecuteNonQueryAsync()
            return numRows
        }

    member this.createDatabaseAsync() =
        async {
            // drop if exists
            // PostgreSQL doesn't support parameterizing drop/create database.
            let dropCmd =
                dbCreateDropDataSource.CreateCommand("DROP DATABASE IF EXISTS " + database.Name)

            let! _ = dropCmd.ExecuteNonQueryAsync()
            // create db
            let createCmd =
                dbCreateDropDataSource.CreateCommand $"CREATE DATABASE {database.Name} ENCODING = 'UTF8'"

            let! _ = createCmd.ExecuteNonQueryAsync()
            // populate db
            let schemaFileDir: string =
                __SOURCE_DIRECTORY__ + "/../../database" |> System.IO.Path.GetFullPath

            let sampleFileDir =
                __SOURCE_DIRECTORY__ + "/../../database/sample" |> System.IO.Path.GetFullPath

            let schemaFiles = System.IO.Directory.GetFiles(schemaFileDir, "*.sql")
            let sampleFiles = System.IO.Directory.GetFiles(sampleFileDir, "*.sql")
            // Must be set here so that the correct DataSource is used.
            isInitalized <- true

            let! _ =
                Array.append schemaFiles sampleFiles
                |> Array.map this.loadSql
                |> Async.Sequential

            ()
        }

    interface IDisposable with
        member _.Dispose() = (database :> IDisposable).Dispose()

[<SetUpFixture>]
type DapperSetup() =
    [<OneTimeSetUp>]
    member _.SetUpThings() =
        FSharp.PostgreSQL.OptionTypes.register () |> ignore
        // Temporary? Copy from the bit in Program.fs.
        SqlMapper.AddTypeHandler(JailMethodTypeHandler())
        SqlMapper.AddTypeHandler(JailMethodOptionTypeHandler())
        SqlMapper.AddTypeHandler(PortsTreeMethodTypeHandler())
