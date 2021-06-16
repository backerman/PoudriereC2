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

type ConfigFileApi (db: DB.dataContext) =

    [<Function("GetConfigFilesMetadata")>]
    member this.getConfigFileMetadata
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/metadata")>]
        req: HttpRequestData) (execContext: FunctionContext) =
            let log = execContext.GetLogger("GetConfigFilesMetadata")
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
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/{id:guid}")>]
         req: HttpRequestData) (execContext: FunctionContext) =
            async {
                let log = execContext.GetLogger("GenerateConfigFile")
                failwith "not implemented"
            } |> Async.StartAsTask
