namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System
open System.Net
open FSharp.Data.Sql
open System.Runtime.InteropServices

type ScheduleApi (jobs: JobRepository, sched: ScheduleRepository) =

    /// Schedule a job.
    [<Function("ScheduleJob")>]
    member _.scheduleJob
        ([<HttpTrigger(AuthorizationLevel.Function, "put", Route="jobs/schedule")>]
         req: HttpRequestData, execContext: FunctionContext) =
        async {
            let log = execContext.GetLogger()
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