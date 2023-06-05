namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System.Net

type JobAPI(repo: JobRepository) =

    /// Endpoint called by a worker VM requesting a job.
    [<Function("RequestJob")>]
    [<Authorize(AuthorizationPolicy.WorkerNode)>]
    member _.requestJob
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "jobs/request")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("RequestJob")
            let response = req.CreateResponse()
            let! maybeVm = tryDeserialize<VirtualMachineInfo> req log

            match maybeVm with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload") |> ignore
            | Some vm ->
                let! err, myJob = repo.GetNextJob(vm.VmId)

                match err with
                | NoError -> response.writeJsonResponse (myJob)
                | Unknown exn ->
                    log.LogError(exn, "Unexpected database error")
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    Error "Internal server error." |> response.writeJsonResponse
                | ForeignKeyViolation exn
                | UniqueViolation exn ->
                    log.LogError(exn, "Really unexpected database error")
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    Error "Internal server error." |> response.writeJsonResponse
                |> ignore

            return response
        }
        |> Async.StartAsTask

    /// Endpoint called by a worker VM to mark job completed.
    [<Function("CompleteJob")>]
    [<Authorize(AuthorizationPolicy.WorkerNode)>]
    member _.completeJob
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "jobs/complete")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("CompleteJob")
            let response = req.CreateResponse()
            let! maybeVm = tryDeserialize<VirtualMachineInfo> req log

            match maybeVm with
            | None -> ()
            | Some vm ->
                let! jobPresent, result = repo.CompleteJob(vm.VmId)

                match jobPresent with
                | true -> response.StatusCode <- HttpStatusCode.OK
                | false ->
                    log.LogError("VM {vmId} does not have a current job", vm.VmId)
                    response.StatusCode <- HttpStatusCode.NotFound

                match result with
                | NoError ->
                    if jobPresent then
                        response.writeJsonResponse OK |> ignore
                | Unknown exn ->
                    log.LogError(exn, "Unexpected database error")
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    Error "Internal server error." |> response.writeJsonResponse |> ignore
                | ForeignKeyViolation exn
                | UniqueViolation exn ->
                    log.LogError(exn, "Really unexpected database error")
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    Error "Internal server error." |> response.writeJsonResponse |> ignore

                if response.StatusCode = HttpStatusCode.NotFound then
                    Error "VM does not have a current job." |> response.writeJsonResponse |> ignore

            return response
        }
        |> Async.StartAsTask
