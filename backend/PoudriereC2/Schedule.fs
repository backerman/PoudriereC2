namespace Facefault.PoudriereC2

open Cronos
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System
open System.Net

type ScheduleApi(jobs: JobRepository, sched: ScheduleRepository) =

    let isJobSchedulable (job: JobSchedule) =
        match job.LastCompleted with
        | None -> true
        | Some lastFinishedTime ->
            let cronExpression = CronExpression.Parse(job.RunAt)

            let nextRunTime =
                cronExpression.GetNextOccurrence(lastFinishedTime, TimeZoneInfo.Utc)

            match nextRunTime.HasValue with
            | false -> false
            | true -> nextRunTime.Value > DateTimeOffset.UtcNow

    /// Schedule a job.
    [<Function("ScheduleJob")>]
    [<Authorize(AuthorizationPolicy.Administrator)>]
    member _.scheduleJob
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "jobs")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("ScheduleJob")
            let response = req.CreateResponse()
            let! maybeSchedule = tryDeserialize<JobSchedule> req log

            match maybeSchedule with
            | Some jobSchedule ->
                let! result = sched.ScheduleJob jobSchedule
                let handled = result.Handle(log, "Unable to schedule {RunAt}", jobSchedule.RunAt)
                response.StatusCode <- handled.httpCode
                response.writeJsonResponse handled.result |> ignore
            | None ->
                let responseData: FunctionResult = Error "Invalid or nonexistent payload"
                response.writeJsonResponse responseData |> ignore

            return response
        }
        |> Async.StartAsTask

    /// Get a job for the calling virtual machine.
    [<Function("GetNextJob")>]
    [<Authorize(AuthorizationPolicy.WorkerNode)>]
    member _.getNextJob
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "jobs/mine")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("GetNextJob")
            let response = req.CreateResponse()
            let! schedulableJobs = sched.GetSchedulableJobs()
            let needToBeRun = schedulableJobs |> List.filter isJobSchedulable
            // TODO: Something based on the specific machine.
            let job = needToBeRun |> List.tryHead

            match job with
            | None ->
                log.LogInformation("No jobs available")
                response.StatusCode <- HttpStatusCode.NoContent
            | Some job ->
                let! result = jobs.GetJobDetails job.JobId

                match result with
                | None ->
                    log.LogError("Unable to get job configuration ID {JobId}", job.JobId)
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    Error "Unable to get job config" |> response.writeJsonResponse |> ignore
                | Some jobConfig ->
                    log.LogInformation("Returning job {JobId}: {JobInfo}", job.JobId, jobConfig)
                    response.writeJsonResponse jobConfig |> ignore

            return response
        }
        |> Async.StartAsTask
