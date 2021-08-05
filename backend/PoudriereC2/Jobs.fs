namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System
open System.Net
open FSharp.Data.Sql
open System.Runtime.InteropServices


type JobAPI (cfg: JobRepository) =

    /// Endpoint called by a worker VM requesting a job.
    [<Function("RequestJob")>]
    member _.requestJob
        ([<HttpTrigger(AuthorizationLevel.Function, "post", Route="jobRequest")>]
         req: HttpRequestData, execContext: FunctionContext) =
            async {
                let log = execContext.GetLogger()
                let response = req.CreateResponse()
                let! maybeVm = tryDeserialize<VirtualMachineInfo> req log
                match maybeVm with
                | None ->
                    response.StatusCode <- HttpStatusCode.BadRequest
                    response.writeJsonResponse
                        (Error "Invalid or nonexistent payload")
                    |> ignore
                | Some vm ->
                    let! myJob = cfg.GetNextJob(vm.VmId);
                    response.writeJsonResponse(myJob) |> ignore

                return response
            } |> Async.StartAsTask

    /// Endpoint called by a worker VM to mark job completed.
    [<Function("CompleteJob")>]
    member _.completeJob
        ([<HttpTrigger(AuthorizationLevel.Function, "post", Route="completeJob")>]
         req: HttpRequestData, execContext: FunctionContext) =
            async {
                let response = req.CreateResponse()
                
                return response.writeJsonResponse(null)
            } |> Async.StartAsTask