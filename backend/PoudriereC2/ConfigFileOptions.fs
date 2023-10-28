namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net
open System

type ConfigFileOptionsApi(cfg: ConfigRepository) =

    [<Function("UpdateConfigFileOptions")>]
    member _.UpdateConfigFileOptions
        // fsharplint:disable-next-line TypedItemSpacing
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "configurationfiles/{configFile:guid}/options")>] req:
            HttpRequestData)
        (execContext: FunctionContext)
        (configFile: Guid)
        =
        async {
            let log = execContext.GetLogger("UpdateConfigFileOptions")
            let! maybeOpts = tryDeserialize<ConfigOptionUpdate list> req log
            let response = req.CreateResponse()

            match maybeOpts with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload") |> ignore
            | Some [] ->
                response.StatusCode <- HttpStatusCode.BadRequest

                response.writeJsonResponse (Error "At least one option must be provided")
                |> ignore
            | Some opts ->
                let! result = cfg.UpdateConfigFileOptions configFile opts

                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK |> ignore
                | someError ->
                    let errResponse =
                        someError.Handle(log, "Unable to update options for config file {ConfigFile}", configFile)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse (errResponse.result) |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("GetConfigFileOptions")>]
    member _.getConfigFileOptions
        // fsharplint:disable-next-line TypedItemSpacing
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "configurationfiles/{configFile:guid}/options")>] req:
            HttpRequestData)
        (execContext: FunctionContext)
        (configFile: Guid)
        =
        async {
            let log = execContext.GetLogger("GetConfigFileOptions")
            let! opts = cfg.GetConfigFileOptions configFile
            // Should we error if there's no such file?
            let response = req.CreateResponse()
            return response.writeJsonResponse opts
        }
        |> Async.StartAsTask
