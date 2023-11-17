module Facefault.PoudriereC2.Entry

open Facefault.PoudriereC2.Database
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open System
open System.Data
open Npgsql
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
        .AddSingleton<NpgsqlDataSource>(fun x ->
            let loggerFactory = x.GetRequiredService<ILoggerFactory>()
            let ds =
                NpgsqlDataSourceBuilder(connStr)
                    .UseLoggerFactory(loggerFactory)
                    .EnableParameterLogging(configuration.["AZURE_FUNCTIONS_ENVIRONMENT"] = "Development")
                    .Build()
            setupDatabaseMappers()
            ds)
        .AddSingleton<ConfigRepository>()
        .AddSingleton<PortRepository>()
        .AddSingleton<JobRepository>()
        .AddSingleton<PortSetRepository>()
        .AddSingleton<JailRepository>()
        .AddSingleton<FreeBSDInfo>()
        .AddSingleton<ScheduleRepository>()
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
    host.Run()
    0
