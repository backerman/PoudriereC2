namespace Facefault.PoudriereC2
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker.Middleware
open Microsoft.Azure.Functions.Worker
open System.Threading.Tasks

type AADMiddleware () =
    interface IFunctionsWorkerMiddleware with
        member _.Invoke(context: FunctionContext, next: FunctionExecutionDelegate) : Task =
            task {
                // Pre-invocation
                let! requestData = context.GetHttpRequestDataAsync()
                let isAdministrator =
                    requestData.Identities
                    |> Seq.exists (fun identity ->
                        identity.Claims
                        |> Seq.exists (fun claim -> claim.Type = "roles" && claim.Value = "PoudriereC2.Administrator")
                    )
                match isAdministrator with
                | true ->
                    do! next.Invoke(context)
                | false ->
                    let resp = requestData.CreateResponse()
                    resp.StatusCode <- System.Net.HttpStatusCode.Unauthorized
                    resp.writeJsonResponse (Error "Unauthorized") |> ignore
                    context.GetInvocationResult().Value <- resp
                // Post-invocation
                ()
            }