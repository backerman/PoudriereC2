module Facefault.PoudriereC2.AssemblyMetadata
open Microsoft.Azure.Functions.Extensions.DependencyInjection

[<assembly:FunctionsStartup(typeof<Configuration.Startup>)>]
do
    printfn "Configuring singletons."