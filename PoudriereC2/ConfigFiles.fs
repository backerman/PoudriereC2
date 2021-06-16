namespace Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
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

type ConfigFileApi (db: DB.dataContext, cfg: ConfigRepository) =

    [<Function("GetConfigFilesMetadata")>]
    member this.getConfigFileMetadata
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/metadata")>]
        req: HttpRequestData) (execContext: FunctionContext) =
            let log = execContext.GetLogger()
            async {
                let files = Seq.toList <| query {
                   for file in db.Poudrierec2.Configfiles do
                   select
                     { Id = file.Id
                       Deleted = file.Deleted
                       Name = file.Name
                       Portset = file.Portset
                       Porttree = file.Porttree
                       Jail = file.Jail
                       FileType = FromString<ConfigFileType> file.Configtype }
                }
                let response = req.CreateResponse(HttpStatusCode.OK)
                return response.writeJsonResponse files
            } |> Async.StartAsTask

    [<Function("GenerateConfigFile")>]
    member this.generateConfigFile
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
