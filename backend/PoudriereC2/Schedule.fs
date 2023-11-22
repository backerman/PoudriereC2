namespace Facefault.PoudriereC2

open Cronos
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System
open System.Net

type ScheduleApi(jobs: JobRepository, sch: ScheduleRepository) =

    let shouldJobRun (job: JobSchedule) =
        match job.LastCompleted with
        | None -> true
        | Some lastCompletedTime ->
            let cronExpression = CronExpression.Parse(job.RunAt)

            let nextRunTime =
                cronExpression.GetNextOccurrence(lastCompletedTime, TimeZoneInfo.Utc)

            // If the next run time is in the past, then the job should be scheduled.
            match nextRunTime.HasValue with
            | false -> false
            | true -> nextRunTime.Value < DateTime.UtcNow

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
            let! schedulableJobs = sch.GetSchedulableJobs()
            let needToBeRun = schedulableJobs |> List.filter shouldJobRun
            // TODO: Something based on the specific machine.
            let job = needToBeRun |> List.tryHead

            match job with
            | None ->
                log.LogInformation("No jobs available")
                response.StatusCode <- HttpStatusCode.NoContent
            | Some job ->
                let! result = jobs.GetJobDetails job.Id

                match result with
                | None ->
                    log.LogError("Unable to get job configuration ID {JobId}", job.Id)
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    Error "Unable to get job config" |> response.writeJsonResponse |> ignore
                | Some jobConfig ->
                    log.LogInformation("Returning job {JobId}: {JobInfo}", job.Id, jobConfig)
                    response.writeJsonResponse jobConfig |> ignore

            return response
        }
        |> Async.StartAsTask
