namespace Facefault.PoudriereC2

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Azure.Cosmos

module GetConfiguration =
    // Define a nullable container to deserialize into.
    [<AllowNullLiteral>]
    type NameContainer() =
        member val Name = "" with get, set

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let Name = "name"

type HttpTriggerMe(dbClient: CosmosClient) =
    member this.getJobConfig (jobId: string) =
        async {
            let db = dbClient.GetDatabase "poudrierec2"
            let configs = db.GetContainer "configurations"
            
            let! jobConfig = configs.ReadItemAsync<JobConfig>(jobId, PartitionKey "jobConfig")
            return jobConfig
        }

    [<FunctionName("GetConfiguration")>]
    member this.run ([<HttpTrigger(AuthorizationLevel.Function, "get", Route = null)>]req: HttpRequest) (log: ILogger) =
        async {
            log.LogInformation("F# HTTP trigger function processed a request.")
            let db = dbClient.GetDatabase "poudrierec2"
            let configs = db.GetContainer "configurations"
            
            let nameOpt = 
                if req.Query.ContainsKey(GetConfiguration.Name) then
                    Some(req.Query.[GetConfiguration.Name].[0])
                else
                    None

            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() |> Async.AwaitTask

            let data = JsonConvert.DeserializeObject<GetConfiguration.NameContainer>(reqBody)

            let name =
                match nameOpt with
                | Some n -> n
                | None ->
                   match data with
                   | null -> ""
                   | nc -> nc.Name
            
            let responseMessage =             
                if (String.IsNullOrWhiteSpace(name)) then
                    "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                else
                    "Hello, " +  name + ". This HTTP triggered function executed successfully."

            return OkObjectResult(responseMessage) :> IActionResult
        } |> Async.StartAsTask
