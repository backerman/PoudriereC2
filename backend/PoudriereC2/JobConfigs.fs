namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net
open System

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

    [<Function("UpdateJobConfig")>]
    [<Authorize(AuthorizationPolicy.Administrator)>]
    member _.updateJobConfigs
        (
            // fsharplint:disable-next-line TypedItemSpacing
            [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "jobconfigs/{jobConfigId:guid}")>] req:
                HttpRequestData,
            execContext: FunctionContext,
            jobConfigId: Guid
        ) =
        async {
            let log = execContext.GetLogger("UpdateJobConfigs")
            let response = req.CreateResponse()
            let! maybeJobConfig = tryDeserialize<JobConfig> req log

            match maybeJobConfig with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload") |> ignore
            | Some jobconfig ->
                let! result = jr.UpdateJobConfig({ jobconfig with Id = Some jobConfigId })

                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK |> ignore
                | err ->
                    let errResponse =
                        err.Handle(log, "Failed to update job config {JobConfig}", jobconfig.Id)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("DeleteJobConfig")>]
    [<Authorize(AuthorizationPolicy.Administrator)>]
    member _.deleteJobConfig
        (
            // fsharplint:disable-next-line TypedItemSpacing
            [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "jobconfigs/{jobConfigId:guid}")>] req:
                HttpRequestData,
            execContext: FunctionContext,
            jobConfigId: Guid
        ) =
        async {
            let log = execContext.GetLogger("DeleteJobConfig")
            let response = req.CreateResponse()
            let! result = jr.DeleteJobConfiguration(jobConfigId)

            match result with
            | NoError ->
                response.StatusCode <- HttpStatusCode.OK
                response.writeJsonResponse OK |> ignore
            | err ->
                let errResponse =
                    err.Handle(log, "Failed to update job config {JobConfig}", jobConfigId)

                response.StatusCode <- errResponse.httpCode
                response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("CreateJobConfig")>]
    [<Authorize(AuthorizationPolicy.Administrator)>]
    member _.createJobConfig
        (
            // fsharplint:disable-next-line TypedItemSpacing
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "jobconfigs")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("DeleteJobConfig")
            let response = req.CreateResponse()
            let! maybeJobConfig = tryDeserialize<JobConfig> req log

            match maybeJobConfig with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload") |> ignore
            | Some jobConfig ->
                let! result = jr.CreateJobConfiguration(jobConfig)

                match result with
                | NoError, newGuid ->
                    response.StatusCode <- HttpStatusCode.OK
                    Created newGuid |> response.writeJsonResponse |> ignore
                | err, _ ->
                    let errResponse =
                        err.Handle(log, "Failed to create job config {JobConfig}", jobConfig.Name)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask
