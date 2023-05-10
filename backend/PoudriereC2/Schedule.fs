namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http

type ScheduleApi (jobs: JobRepository, sched: ScheduleRepository) =

    /// Schedule a job.
    [<Function("ScheduleJob")>]
    member _.scheduleJob
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route="jobs/schedule")>]
         req: HttpRequestData, execContext: FunctionContext) =
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
                let responseData : FunctionResult = Error "Invalid or nonexistent payload"
                response.writeJsonResponse responseData |> ignore
            return response
        } |> Async.StartAsTask