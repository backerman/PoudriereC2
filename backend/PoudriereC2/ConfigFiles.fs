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

    [<Function("NewConfigFile")>]
    member _.newConfigFile
        ([<HttpTrigger(AuthorizationLevel.Function, "put", Route="configurationfiles/metadata")>]
        req: HttpRequestData, execContext: FunctionContext) =
            async {
                let log = execContext.GetLogger()
                let response = req.CreateResponse(HttpStatusCode.OK)
                let! maybeConfig = tryDeserialize<ConfigFileMetadata> req log
                match maybeConfig with
                | None ->
                    response.StatusCode <- HttpStatusCode.BadRequest
                    response.writeJsonResponse
                        (Error "Invalid or nonexistent payload") |> ignore
                | Some meta ->
                    let! result = cfg.newConfigFile meta
                    match result with
                    | NoError ->
                        response.StatusCode <- HttpStatusCode.OK
                        response.writeJsonResponse OK |> ignore
                    | ForeignKeyViolation ex ->
                        log.LogError
                            ("Failed upsert of config {ConfigFile}: {ViolatedConstraint} violated",
                            meta.Id, ex.ConstraintName)
                        response.StatusCode <- HttpStatusCode.UnprocessableEntity
                        response.writeJsonResponse
                            (Error
                            $"Referential integrity violation: value of {ex.ColumnName} does not exist")
                            |> ignore
                    | UniqueViolation ex ->
                        response.StatusCode <- HttpStatusCode.UnprocessableEntity
                        let errorText =
                            match ex.ConstraintName with
                            | "configfiles_index_undeleted_titles" -> "title"
                            | "configfiles_pk" -> "GUID"
                            | _ -> ""
                        log.LogError
                            ("Failed upsert of config {ConfigFile}: {ViolatedConstraint} violated",
                            meta.Id, ex.ConstraintName)
                        response.writeJsonResponse
                            (Error $"Configuration {errorText} already exists") |> ignore
                    | Unknown ex ->
                        log.LogError
                            (ex, "Failed insert of config {ConfigFile}", meta.Id)
                        response.StatusCode <- HttpStatusCode.InternalServerError
                        response.writeJsonResponse
                            (Error "Internal server error") |> ignore
                return response
            } |> Async.StartAsTask

    [<Function("GetConfigFilesMetadata")>]
    member _.getConfigFileMetadata
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/metadata/{configFile:guid?}")>]
        req: HttpRequestData, execContext: FunctionContext, [<Optional>]configFile: string) =
            let log = execContext.GetLogger()
            let configFileOpt =
                match configFile with
                | null -> None
                | _ -> Some configFile
            async {
                let! files = cfg.getConfigFiles(?configFile = configFileOpt)
                let response = req.CreateResponse(HttpStatusCode.OK)
                return response.writeJsonResponse files
            } |> Async.StartAsTask

    [<Function("GenerateConfigFile")>]
    member _.generateConfigFile
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/{configFile:guid}")>]
         req: HttpRequestData) (execContext: FunctionContext) (configFile: string) =
            async {
                let log = execContext.GetLogger()
                let response = req.CreateResponse()
                match pickReturnMediaType req with
                | Some AnyType | Some PlainText ->
                    let! configMetadataSeq = cfg.getConfigFiles configFile
                    let! configOptions =
                        cfg.getConfigFileOptions configFile
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
