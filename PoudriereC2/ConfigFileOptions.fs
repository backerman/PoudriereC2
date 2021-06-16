namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Extensions.Logging
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Text.Json
open Npgsql
open System
open System.Net
open System.Linq
open FSharp.Data.Sql

type ConfigFileOptionsApi (db: DB.dataContext, cfg: ConfigRepository) =

    [<Function("GetConfigFileOptions")>]
    member this.getConfigFileOptions
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
    member this.addConfigFileOptions
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
                        (Error "Can't show tea and no tea to the door") |> ignore
                | Some [] ->
                    response.StatusCode <- HttpStatusCode.BadRequest
                    response.writeJsonResponse
                        (Error "Nope!") |> ignore
                | Some opts ->
                    opts
                    |> List.iter
                        (fun o -> 
                            let row = db.Poudrierec2.Configoptions.Create()
                            row.Configfile <- Guid configFile
                            row.Name <- o.Name
                            row.Value <- o.Value
                            row.OnConflict <- Common.OnConflict.Update)
                    let! result = Async.Catch(db.SubmitUpdatesAsync())
                    match result with
                    | Choice1Of2 _ ->
                        response.StatusCode <- HttpStatusCode.OK
                        response.writeJsonResponse OK |> ignore
                    | Choice2Of2 e ->
                        log.LogError
                            (e, "Failed upsert of config {ConfigFile}", configFile)
                        response.StatusCode <- HttpStatusCode.InternalServerError
                        match e.InnerException with
                        | :? PostgresException as ex ->
                            match ex.SqlState with
                            | PostgresErrorCodes.ForeignKeyViolation ->
                                response.writeJsonResponse
                                    (Error "configuration file does not exist")
                                    |> ignore
                            | _ -> ()
                        | _ -> ()
                return response
            } |> Async.StartAsTask

    [<Function("DeleteConfigFileOptions")>]
    member this.deleteConfigFileOptions
        ([<HttpTrigger(AuthorizationLevel.Function, "delete", Route="configurationfiles/{configFile:guid}/options")>]
         req: HttpRequestData) (execContext: FunctionContext) (configFile: string) =
            async {
                let log = execContext.GetLogger()
                let! maybeOpts = Serialization.tryDeserialize<string list> req log
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
                    query {
                        for o in db.Poudrierec2.Configoptions do
                        where (o.Configfile = Guid configFile && opts.Contains o.Name)
                    } |> Seq.``delete all items from single table``
                    |> Async.RunSynchronously
                    |> ignore
                return response
            } |> Async.StartAsTask