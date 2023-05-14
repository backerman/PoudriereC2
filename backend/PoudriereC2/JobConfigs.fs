namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System.Net

type JobConfigApi(jr: JobRepository) =

    [<Function("GetJobConfigs")>]
    [<Authorize(AuthorizationPolicy.Viewer)>]
    member _.getJobConfigs
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "jobconfigs")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("GetJobConfigs")
            let response = req.CreateResponse()
            let! maybeJobConfigs = jr.GetJobConfigs()

            match maybeJobConfigs with
            | (NoError, jobConfigs) ->
                response.StatusCode <- HttpStatusCode.OK
                response.writeJsonResponse jobConfigs |> ignore
            | (err, _) ->
                let errResponse = err.Handle(log, "Failed to get job configs")
                response.StatusCode <- errResponse.httpCode
                response.writeJsonResponse errResponse.result |> ignore
            return response
        }
        |> Async.StartAsTask
