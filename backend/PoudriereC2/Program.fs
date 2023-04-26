module Facefault.PoudriereC2.Entry

open Facefault.PoudriereC2.Database
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open FSharp.Data.Sql
open System
open System.Data
open Npgsql
open Dapper

let configuration =
    ConfigurationBuilder()
        .SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables()
        .Build()

[<EntryPoint>]
let main argv =
    let host =
        let connStr = System.Environment.GetEnvironmentVariable "PostgresConnection"
        if isNull (System.Environment.GetEnvironmentVariable "PGPASSWORD") then
            let accessToken = getAccessToken()
            System.Environment.SetEnvironmentVariable("PGPASSWORD", accessToken)
        HostBuilder()
            .ConfigureFunctionsWorkerDefaults(
                fun (_: HostBuilderContext) (builder: IFunctionsWorkerApplicationBuilder) ->
                    builder.UseWhen<AADMiddleware>(
                        fun _ -> configuration.["AZURE_FUNCTIONS_ENVIRONMENT"] <> "Development"
                    ) |> ignore
            )
            .ConfigureServices(
                fun s ->
                    s.AddSingleton<DB.dataContext> (DB.GetDataContext(connStr)) |> ignore
                    s.AddSingleton<NpgsqlConnection>(getDatabaseConnection()) |> ignore
                    s.AddSingleton<ConfigRepository> () |> ignore
                    s.AddSingleton<PortsRepository> () |> ignore
                    s.AddSingleton<JobRepository> () |> ignore
                    s.AddSingleton<PortSetsRepository> () |> ignore
                    s.AddSingleton<JailRepository> () |> ignore
                    s.AddSingleton<FreeBSDInfo> () |> ignore
            )
            .Build()
    if configuration.["AZURE_FUNCTIONS_ENVIRONMENT"] = "Development" then
        Common.QueryEvents.SqlQueryEvent
        |> Event.add
            (fun sql -> printfn $"Executing SQL: {sql}")

    // Set up Dapper type mappers.
    SqlMapper.AddTypeHandler (JailMethodTypeHandler())
    SqlMapper.AddTypeHandler (JailMethodOptionTypeHandler())
    host.Run()
    0