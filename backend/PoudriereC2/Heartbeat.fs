namespace Facefault.PoudriereC2

open System
open System.IO
open System.Text.Json
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

module Heartbeat =

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let Name = "name"

    [<FunctionName("Heartbeat")>]
    let run ([<HttpTrigger(AuthorizationLevel.Function, "post", Route = null)>]req: HttpRequest) (log: ILogger) =
        async {
            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() |> Async.AwaitTask
            let data = 
                try
                    JsonSerializer.Deserialize<Event>
                        (reqBody, eventSerializationOptions) |> Some
                with
                | :? System.Text.Json.JsonException -> None
                | e -> raise e
            let response =
                match data with
                | Some d ->
                    let msg = "Received heartbeat from " + d.VmName + "."
                    OkObjectResult(msg) :> IActionResult
                | None ->
                    log.LogError("Request from {0} not deserializable.", req.HttpContext.Connection.RemoteIpAddress)
                    BadRequestObjectResult("Bad request.") :> IActionResult
            return response
        } |> Async.StartAsTask