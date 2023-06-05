namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System
open System.Net
open FSharp.Data.Sql

type ConfigFileApi(cfg: ConfigRepository) =

    [<Function("NewConfigFile")>]
    member _.newConfigFile
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "configurationfiles")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("NewConfigFile")
            let response = req.CreateResponse(HttpStatusCode.OK)
            let! maybeConfig = tryDeserialize<ConfigFileMetadata> req log

            match maybeConfig with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload") |> ignore
            | Some meta ->
                let! result = cfg.NewConfigFile meta

                match result with
                | (NoError, aGuid) ->
                    response.StatusCode <- HttpStatusCode.OK
                    Created aGuid |> response.writeJsonResponse |> ignore
                | (someError, _) ->
                    let errResponse =
                        someError.Handle(log, "Failed creation of config {ConfigFile}: {Error}", meta.Name, someError)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("UpdateConfigFile")>]
    member _.updateConfigFile
        (
            // fsharplint:disable-next-line TypedItemSpacing
            [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "configurationfiles/{configFile:guid}")>] req:
                HttpRequestData,
            execContext: FunctionContext,
            configFile: Guid
        ) =
        async {
            let log = execContext.GetLogger("UpdateConfigFile")
            let response = req.CreateResponse(HttpStatusCode.OK)
            let! maybeConfig = tryDeserialize<ConfigFileMetadata> req log

            match maybeConfig with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload") |> ignore
            | Some meta ->
                let! result = cfg.UpdateConfigFile { meta with Id = Some configFile }

                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK |> ignore
                | someError ->
                    let errResponse =
                        someError.Handle(log, "Failed update of config {ConfigFile}: {Error}", meta.Name, someError)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("GetConfigFilesMetadata")>]
    member _.getConfigFileMetadata
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "configurationfiles")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        let log = execContext.GetLogger("GetConfigFilesMetadata")

        async {
            let! files = cfg.GetConfigFiles()
            let response = req.CreateResponse(HttpStatusCode.OK)
            return response.writeJsonResponse files
        }
        |> Async.StartAsTask

    [<Function("DeleteConfigFile")>]
    member _.deleteConfigFile
        (
            // fsharplint:disable-next-line TypedItemSpacing
            [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "configurationfiles/{configFile:guid}")>] req:
                HttpRequestData,
            execContext: FunctionContext,
            configFile: Guid
        ) =
        async {
            let log = execContext.GetLogger("DeleteConfigFile")
            let response = req.CreateResponse(HttpStatusCode.OK)
            let! result = cfg.DeleteConfigFile configFile

            match result with
            | NoError ->
                response.StatusCode <- HttpStatusCode.OK
                response.writeJsonResponse OK |> ignore
            | someError ->
                let errResponse =
                    someError.Handle(log, "Failed deletion of config {ConfigFile}: {Error}", configFile, someError)

                response.StatusCode <- errResponse.httpCode
                response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("GenerateConfigFile")>]
    [<Authorize(AuthorizationPolicy.WorkerNode)>]
    member _.generateConfigFile
        // fsharplint:disable-next-line TypedItemSpacing
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "configurationfiles/{configFile:guid}")>] req:
            HttpRequestData)
        (execContext: FunctionContext)
        (configFile: Guid)
        =
        async {
            let log = execContext.GetLogger("GenerateConfigFile")
            let response = req.CreateResponse()

            match pickReturnMediaType req with
            | Some AnyType
            | Some PlainText
            | None ->
                // let! configMetadataSeq = cfg.GetConfigFiles configFile
                let! configOptions = configFile.ToString() |> cfg.GetConfigFileOptions
                response.StatusCode <- HttpStatusCode.OK

                configOptions
                |> Seq.map (fun opt -> $"{opt.Name}={opt.Value.ShellQuote()}")
                |> String.concat Environment.NewLine
                |> response.writeTextResponse
                |> ignore
            | _ ->
                response.StatusCode <- HttpStatusCode.UnsupportedMediaType
                Error "Unsupported media type" |> response.writeJsonResponse |> ignore

            return response
        }
        |> Async.StartAsTask
