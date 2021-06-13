namespace Facefault.PoudriereC2

open Azure.Cosmos
open Azure.Cosmos.Fluent
open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System
open FSharp.Data.LiteralProviders
open FSharp.Data.Sql

module Configuration =

    /// The connection string to use by default.
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

    type Startup() =
        inherit FunctionsStartup()

        let configuration =
            (new ConfigurationBuilder())
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build()
        
        override this.Configure(builder: IFunctionsHostBuilder): unit = 
            let buildClient =
                (fun s ->
                    let cosmosConnectionString = configuration.["CosmosDBConnection"]
                    if String.IsNullOrEmpty connectionString then
                        failwith "Please provide a valid thingy"
                    let clientBuilder = 
                        (new CosmosClientBuilder(cosmosConnectionString))
                            .WithCustomSerializer(MyCustomCosmosSerializer())
                    clientBuilder.Build())
            builder.Services.AddSingleton<CosmosClient> buildClient |> ignore
            builder.Services.AddSingleton<DB.dataContext> (DB.GetDataContext()) |> ignore
            if configuration.["AZURE_FUNCTIONS_ENVIRONMENT"] = "Develpment" then
                Common.QueryEvents.SqlQueryEvent
                |> Event.add
                    (fun sql -> printfn "Executing SQL: %O" sql)

    [<assembly:FunctionsStartup(typeof<Startup>)>]
    do
        printfn "Configuring singletons."