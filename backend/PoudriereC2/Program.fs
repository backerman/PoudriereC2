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
open Microsoft.Extensions.Logging

let configuration =
    ConfigurationBuilder()
        .SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile("appsettings.json", true)
        .AddEnvironmentVariables()
        .Build()

let functionsWorkerDefaultsDelegate (_: HostBuilderContext) (builder: IFunctionsWorkerApplicationBuilder) =
    builder.UseWhen<AADMiddleware>(fun _ -> configuration.["AZURE_FUNCTIONS_ENVIRONMENT"] <> "Development")
    |> ignore

let servicesDelegate (s: IServiceCollection) =
    let connStr = System.Environment.GetEnvironmentVariable "PostgresConnection"

    s
        .AddSingleton<DB.dataContext>(DB.GetDataContext(connStr))
        .AddSingleton<NpgsqlDataSource>(fun x ->
            let loggerFactory = x.GetRequiredService<ILoggerFactory>()
            FSharp.PostgreSQL.OptionTypes.register () |> ignore

            NpgsqlDataSourceBuilder(ConnectionString)
                .UseLoggerFactory(loggerFactory)
                .EnableParameterLogging(configuration.["AZURE_FUNCTIONS_ENVIRONMENT"] = "Development")
                .Build())
        .AddSingleton<ConfigRepository>()
        .AddSingleton<PortsRepository>()
        .AddSingleton<JobRepository>()
        .AddSingleton<PortSetsRepository>()
        .AddSingleton<JailRepository>()
        .AddSingleton<FreeBSDInfo>()
    |> ignore

[<EntryPoint>]
let main argv =
    let host =
        if isNull (System.Environment.GetEnvironmentVariable "PGPASSWORD") then
            let accessToken = getAccessToken ()
            System.Environment.SetEnvironmentVariable("PGPASSWORD", accessToken)

        HostBuilder()
            .ConfigureFunctionsWorkerDefaults(functionsWorkerDefaultsDelegate)
            .ConfigureServices(servicesDelegate)
            .Build()

    if configuration.["AZURE_FUNCTIONS_ENVIRONMENT"] = "Development" then
        Common.QueryEvents.SqlQueryEvent
        |> Event.add (fun sql -> printfn $"Executing SQL: {sql}")

    // Set up Dapper type mappers.
    SqlMapper.AddTypeHandler(JailMethodTypeHandler())
    SqlMapper.AddTypeHandler(JailMethodOptionTypeHandler())
    host.Run()
    0
