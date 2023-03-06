namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System
open System.Net
open FSharp.Data.Sql
open System.Runtime.InteropServices

type ConfigFileApi (cfg: ConfigRepository) =

    [<Function("NewOrUpdateConfigFile")>]
    member _.newOrUpdateConfigFile
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route="configurationfiles/metadata")>]
         req: HttpRequestData, execContext: FunctionContext) =
            async {
                let log = execContext.GetLogger()
                let response = req.CreateResponse(HttpStatusCode.OK)
                let! maybeConfig = tryDeserialize<ConfigFileMetadata> req log
                match maybeConfig with
                | None ->
                    response.StatusCode <- HttpStatusCode.BadRequest
                    response.writeJsonResponse
                        (Error "Invalid or nonexistent payload")
                    |> ignore
                | Some meta ->
                    let! result = cfg.NewOrUpdateConfigFile meta
                    match result with
                    | NoError ->
                        response.StatusCode <- HttpStatusCode.OK
                        response.writeJsonResponse OK |> ignore
                    | someError ->
                        let errResponse = 
                            someError.Handle
                                (log, "Failed upsert of config {ConfigFile}: {Error}", meta.Name, someError)
                        response.StatusCode <- errResponse.httpCode
                        response.writeJsonResponse errResponse.result
                        |> ignore
                return response
            } |> Async.StartAsTask

    [<Function("GetConfigFilesMetadata")>]
    member _.getConfigFileMetadata
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="configurationfiles/metadata/{configFile:guid?}")>]
         req: HttpRequestData, execContext: FunctionContext, [<Optional>]configFile: string) =
            let log = execContext.GetLogger()
            let configFileOpt =
                match configFile with
                | null -> None
                | _ -> Some configFile
            async {
                let! files = cfg.GetConfigFiles(?configFile = configFileOpt)
                let response = req.CreateResponse(HttpStatusCode.OK)
                return response.writeJsonResponse files
            } |> Async.StartAsTask

    [<Function("GenerateConfigFile")>]
    member _.generateConfigFile
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="configurationfiles/{configFile:guid}")>]
        req: HttpRequestData) (execContext: FunctionContext) (configFile: string) =
            async {
                let log = execContext.GetLogger()
                let response = req.CreateResponse()
                match pickReturnMediaType req with
                | Some AnyType
                | Some PlainText ->
                    let! configMetadataSeq = cfg.GetConfigFiles configFile
                    let! configOptions =
                        cfg.GetConfigFileOptions configFile
                    response.StatusCode <- HttpStatusCode.OK
                    configOptions
                    |> Seq.map
                        (fun opt -> $"{opt.Name}={opt.Value.ShellQuote()}")
                    |> String.concat Environment.NewLine
                    |> response.writeTextResponse
                    |> ignore
                | _ ->
                    response.StatusCode <- HttpStatusCode.UnsupportedMediaType
                    Error "Unsupported media type"
                    |> response.writeJsonResponse
                    |> ignore
                return response
            } |> Async.StartAsTask