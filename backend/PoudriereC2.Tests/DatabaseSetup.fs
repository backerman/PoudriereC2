namespace PoudriereC2.Tests

open NUnit.Framework
open Npgsql
open System
open System.IO
open System.Threading.Tasks

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
    let mutable isInitialized: bool = false
    let database = new Database(TestContext.CurrentContext)

    let dbCreateDropDataSource =
        NpgsqlDataSourceBuilder(database.ConnectionString).Build()

    let workDataSource =
        let csb = NpgsqlConnectionStringBuilder(database.ConnectionString)
        csb.Database <- database.Name
        NpgsqlDataSourceBuilder(csb.ToString()).Build()

    // The NpgsqlDataSource to use for tests within this scope.
    member _.DataSource =
        if isInitialized then
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
            isInitialized <- true

            let! _ =
                Array.append schemaFiles sampleFiles
                |> Array.map this.loadSql
                |> Async.Sequential

            ()
        }

    member _.dropDatabaseAsync() =
        async {
            let cmd =
                dbCreateDropDataSource.CreateCommand("DROP DATABASE IF EXISTS " + database.Name + " WITH (FORCE)")

            do! workDataSource.DisposeAsync()
            let! _ = cmd.ExecuteNonQueryAsync()
            ()
        }

    interface IDisposable with
        member this.Dispose() =
            this.dropDatabaseAsync () |> Async.RunSynchronously
            (database :> IDisposable).Dispose()

    interface IAsyncDisposable with
        member this.DisposeAsync() =
            task {
                do! this.dropDatabaseAsync ()
                (database :> IDisposable).Dispose()
            }
            |> ValueTask

[<SetUpFixture>]
type DapperSetup() =
    [<OneTimeSetUp>]
    member _.SetUpThings() =
        // Temporary? Copy from the bit in Program.fs.
        Facefault.PoudriereC2.Database.setupDatabaseMappers ()
