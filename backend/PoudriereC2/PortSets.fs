namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System.Net
open System

type PortSetsApi (ps: PortSetsRepository) =
    [<Function("GetPortSets")>]
    member _.GetPortSets
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="portsets/{portset?}")>]
         req: HttpRequestData, execContext: FunctionContext, portset: Guid option) =
        async {
            let! portsets = ps.GetPortSets(portset)
            let response = req.CreateResponse(HttpStatusCode.OK)
            return response.writeJsonResponse portsets
        } |> Async.StartAsTask

    [<Function("GetPortSetMembers")>]
    member _.GetPortSetMembers
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="portsets/{portset}/members")>]
         req: HttpRequestData, execContext: FunctionContext, portset: Guid) =
        async {
            let! portsets = ps.GetPortSetMembers(portset)
            let response = req.CreateResponse(HttpStatusCode.OK)
            return response.writeJsonResponse portsets
        } |> Async.StartAsTask
