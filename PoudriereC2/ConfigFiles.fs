namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System
open System.Net
open FSharp.Data.Sql
open System.Runtime.InteropServices

type ConfigFileApi (db: DB.dataContext, cfg: ConfigRepository) =

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
                    let! configMetadata =
                        query {
                            for f in db.Poudrierec2.Configfiles do
                            where (f.Id = Guid configFile)
                            select f
                        } |> Seq.executeQueryAsync
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
