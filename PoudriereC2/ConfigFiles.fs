namespace Facefault.PoudriereC2
open Configuration
open Facefault.PoudriereC2.Data
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open System.Text.Json
open Microsoft.AspNetCore.Mvc
open System

type ConfigFileApi (db: DB.dataContext) =

    [<FunctionName("GetConfigFilesMetadata")>]
    member this.getConfigFileMetadata
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/metadata")>]
        req: HttpRequest) (log: ILogger) =
            async {
                let files = Seq.toList <| query {
                   for file in db.Poudrierec2.Configfiles do
                   select
                     { Id = file.Id
                       Deleted = file.Deleted
                       Name = file.Name
                       Portset = None
                       Porttree = None
                       Jail = None
                       FileType = FromString<ConfigFileType> file.Configtype }
                }
                let responseMessage = JsonSerializer.Serialize(files, eventSerializationOptions)
                let response = ContentResult()
                response.Content <- responseMessage
                response.ContentType <- "application/json"
                response.StatusCode <- StatusCodes.Status200OK
                return response :> IActionResult
            } |> Async.StartAsTask

    [<FunctionName("GenerateConfigFile")>]
    member this.generateConfigFile
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/{id:guid}")>]
         req: HttpRequest) (id: Guid) (log: ILogger) =
            async {
                failwith "not implemented"
                let response = ContentResult()
                return response :> IActionResult
            } |> Async.StartAsTask

    [<FunctionName("GetConfigFileOptions")>]
    member this.getConfigFileOptions
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="configurationfiles/{id:guid}/options")>]
         req: HttpRequest) (id: Guid) (log: ILogger) =
            async {
                let opts = Seq.toList <| query {
                    for configOption in db.Poudrierec2.Configoptions do
                    where (configOption.Configfile = id)
                    sortBy configOption.Name
                    select { Name = configOption.Name
                             Value = configOption.Value }
                }
                // Should we error if there's no such file?
                let responseMessage = JsonSerializer.Serialize(opts, eventSerializationOptions)
                let response = ContentResult()
                response.Content <- responseMessage
                response.ContentType <- "application/json"
                response.StatusCode <- StatusCodes.Status200OK
                return response :> IActionResult
            } |> Async.StartAsTask

    [<FunctionName("AddConfigFileOptions")>]
    member this.addConfigFileOptions
        ([<HttpTrigger(AuthorizationLevel.Function, "put", Route="configurationfiles/{id:guid}/options")>]
         req: HttpRequest) (id: Guid) (log: ILogger) =
            async {
                failwith "not implemented"
                let response = ContentResult()
                response.ContentType <- "application/json"
                response.StatusCode <- StatusCodes.Status200OK
                return response :> IActionResult
            } |> Async.StartAsTask
