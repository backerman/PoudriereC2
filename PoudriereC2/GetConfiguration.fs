namespace Facefault.PoudriereC2

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Azure.Cosmos
open System.Text.Json
open Microsoft.AspNetCore

module GetConfiguration =
    // Define a nullable container to deserialize into.
    [<AllowNullLiteral>]
    type NameContainer() =
        member val Name = "" with get, set

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let Name = "name"

type HttpTriggerMe(dbClient: CosmosClient) =
    member this.getJobConfig (configs: CosmosContainer) (jobId: Guid) =
        async {
            let! jobConfig =
                configs.ReadItemAsync<JobConfig>(jobId.ToString(), PartitionKey "jobConfig")
            return jobConfig
        }

    [<FunctionName("GetConfiguration")>]
    member this.run
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetConfiguration/jobId/{jobId:guid}")>]
            req: HttpRequest) (log: ILogger) (jobId: Guid) =
        async {
            let db = dbClient.GetDatabase "poudrierec2"
            let configs = db.GetContainer "configurations"
            let! jobConfig = this.getJobConfig configs jobId
            let responseMessage = JsonSerializer.Serialize(jobConfig.Value, eventSerializationOptions)
            let response = ContentResult()
            response.Content <- responseMessage
            response.ContentType <- "application/json"
            response.StatusCode <- Http.StatusCodes.Status200OK
            return response :> IActionResult
        } |> Async.StartAsTask
