namespace Facefault.PoudriereC2

open Microsoft.Azure.Cosmos.Fluent
open Microsoft.Azure.Functions.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System
open Microsoft.Azure.Cosmos

module Configuration =
    type Startup() =
        inherit FunctionsStartup()

        let configuration =
            (new ConfigurationBuilder())
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build()
        
        override this.Configure(builder: IFunctionsHostBuilder): unit = 
            // Need to define function elsewhere because lambda doesn't support
            // annotating return value.
            let buildClient =
                (fun s ->
                    let connectionString = configuration.["CosmosDBConnection"]
                    if String.IsNullOrEmpty connectionString then
                        failwith "Please provide a valid thingy"
                    let clientBuilder = new CosmosClientBuilder(connectionString)
                    clientBuilder.Build())
            builder.Services.AddSingleton<CosmosClient> buildClient |> ignore

    [<assembly:FunctionsStartup(typeof<Startup>)>]
    do
        printfn "Configuring singletons."