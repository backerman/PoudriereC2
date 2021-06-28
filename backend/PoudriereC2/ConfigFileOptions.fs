namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Extensions.Logging
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net

type ConfigFileOptionsApi (cfg: ConfigRepository) =

    [<Function("GetConfigFileOptions")>]
    member _.getConfigFileOptions
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/{configFile:guid}/options")>]
         req: HttpRequestData) (execContext: FunctionContext) (configFile: string) =
            async {
                let log = execContext.GetLogger()
                let! opts = cfg.getConfigFileOptions configFile
                // Should we error if there's no such file?
                let response = req.CreateResponse()
                return response.writeJsonResponse opts
            } |> Async.StartAsTask

    [<Function("AddConfigFileOptions")>]
    member _.addConfigFileOptions
        ([<HttpTrigger(AuthorizationLevel.Function, "put", Route="configurationfiles/{configFile:guid}/options")>]
         req: HttpRequestData) (execContext: FunctionContext) (configFile: string) =
            async {
                let log = execContext.GetLogger()
                let response = req.CreateResponse(HttpStatusCode.OK)
                let! maybeOpts = tryDeserialize<ConfigOption list> req log 
                match maybeOpts with
                | None ->
                    response.StatusCode <- HttpStatusCode.BadRequest
                    response.writeJsonResponse
                        (Error "Invalid or nonexistent payload") |> ignore
                | Some [] ->
                    response.StatusCode <- HttpStatusCode.BadRequest
                    response.writeJsonResponse
                        (Error "At least one option must be provided") |> ignore
                | Some opts ->
                    let! result = cfg.addConfigFileOptions configFile opts
                    match result with
                    | NoError ->
                        response.StatusCode <- HttpStatusCode.OK
                        response.writeJsonResponse OK |> ignore
                    | ForeignKeyViolation _ ->
                        log.LogError
                            ("Failed upsert of config {ConfigFile}: config does not exist", configFile)
                        response.StatusCode <- HttpStatusCode.UnprocessableEntity
                        response.writeJsonResponse
                            (Error "Nonexistent configuration file") |> ignore
                    | UniqueViolation _ -> 
                        // can't happen but.
                        log.LogError
                            ("Failed upsert of config {ConfigFile}: unexpected uniqueness violation", configFile)
                        response.StatusCode <- HttpStatusCode.InternalServerError
                        response.writeJsonResponse
                            (Error "Bad request") |> ignore
                    | Unknown ex ->
                        log.LogError
                            (ex, "Failed upsert of config {ConfigFile}: {errorMsg}", configFile)
                        response.StatusCode <- HttpStatusCode.InternalServerError
                        response.writeJsonResponse
                            (Error "Bad request") |> ignore
                return response
            } |> Async.StartAsTask

    [<Function("DeleteConfigFileOptions")>]
    member _.deleteConfigFileOptions
        ([<HttpTrigger(AuthorizationLevel.Function, "delete", Route="configurationfiles/{configFile:guid}/options")>]
         req: HttpRequestData) (execContext: FunctionContext) (configFile: string) =
            async {
                let log = execContext.GetLogger()
                let! maybeOpts = tryDeserialize<string list> req log
                let response = req.CreateResponse()
                match maybeOpts with
                | None ->
                    response.StatusCode <- HttpStatusCode.BadRequest
                    response.writeJsonResponse
                        (Error "Unable to show tea and no tea to the door")
                        |> ignore
                | Some [] ->
                    response.StatusCode <- HttpStatusCode.BadRequest
                    response.writeJsonResponse
                        (Error "What is the difference between a chicken?")
                        |> ignore
                | Some opts -> 
                    response.StatusCode <- HttpStatusCode.NoContent
                    let! _ = cfg.deleteConfigFileOptions configFile opts
                    ()
                return response
            } |> Async.StartAsTask