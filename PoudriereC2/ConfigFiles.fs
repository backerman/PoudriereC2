namespace Facefault.PoudriereC2
open Configuration
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open System.Text.Json
open Microsoft.AspNetCore.Mvc


type ConfigFileApi (db: DB.dataContext) =

    [<FunctionName("GetConfigFiles")>]
    member this.getAllConfigFiles
        ([<HttpTrigger(AuthorizationLevel.Function, "get")>]
        req: HttpRequest) (log: ILogger)=
            async {
                let rows = Seq.toList <| query {
                   for file in db.Poudrierec2.Configfiles do
                   select file
                }
                let files =
                    rows
                    |> List.map
                        ( fun file ->
                          { Id = file.Id
                            Deleted = file.Deleted
                            Name = file.Name
                            Portset = None
                            Porttree = None
                            Jail = None
                            FileType = ConfigFileType.FromString file.Configtype }
                        )
                let responseMessage = JsonSerializer.Serialize(files, eventSerializationOptions)
                let response = ContentResult()
                response.Content <- responseMessage
                response.ContentType <- "application/json"
                response.StatusCode <- StatusCodes.Status200OK
                return response :> IActionResult
            } |> Async.StartAsTask