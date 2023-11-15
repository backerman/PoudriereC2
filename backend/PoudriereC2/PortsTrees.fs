namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System.Net
open System

type PortsTreesApi(cfg: PortRepository) =
    [<Function("NewPortsTree")>]
    member _.NewPortsTree
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "portstrees")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("NewPortsTree")
            let response = req.CreateResponse(HttpStatusCode.OK)
            let! maybeConfig = tryDeserialize<PortsTree> req log

            match maybeConfig with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload") |> ignore
            | Some meta ->
                let! result = cfg.AddPortsTree meta

                match result with
                | NoError, newGuid ->
                    response.StatusCode <- HttpStatusCode.OK
                    Created newGuid |> response.writeJsonResponse |> ignore
                | someError, _ ->
                    let errResponse =
                        someError.Handle(log, "Failed to add ports tree {PortsTree}", meta.Name)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("GetPortsTree")>]
    member _.GetPortsTree
        (
            // fsharplint:disable-next-line TypedItemSpacing
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "portstrees/{treeId:guid}")>] req:
                HttpRequestData,
            execContext: FunctionContext,
            treeId: Guid
        ) =
        let log = execContext.GetLogger("GetPortsTree")

        async {
            let! files = cfg.GetPortsTrees(treeId)
            let response = req.CreateResponse(HttpStatusCode.OK)
            return response.writeJsonResponse files
        }
        |> Async.StartAsTask

    [<Function("GetPortsTrees")>]
    member _.GetPortsTrees
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "portstrees")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        let log = execContext.GetLogger("GetPortsTrees")

        async {
            let! files = cfg.GetPortsTrees()
            let response = req.CreateResponse(HttpStatusCode.OK)
            return response.writeJsonResponse files
        }
        |> Async.StartAsTask

    [<Function("ModifyPortsTree")>]
    member _.ModifyPortsTree
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "portstrees/{treeId}")>] req: HttpRequestData,
            execContext: FunctionContext,
            treeId: Guid
        ) =
        let log = execContext.GetLogger("ModifyPortsTree")

        async {
            let! maybeConfig = tryDeserialize<PortsTree> req log
            let response = req.CreateResponse(HttpStatusCode.OK)

            match maybeConfig with
            | None -> response.StatusCode <- HttpStatusCode.BadRequest
            | Some meta ->
                let! result = cfg.UpdatePortsTree {meta with Id = Some treeId}

                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK |> ignore
                | _ ->
                    let errResponse =
                        result.Handle(log, "Failed to modify ports tree {PortsTree}", treeId)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("DeletePortsTree")>]
    member _.DeletePortsTree
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "portstrees/{treeId}")>] req: HttpRequestData,
            execContext: FunctionContext,
            treeId: Guid
        ) =
        let log = execContext.GetLogger("DeletePortsTree")

        async {
            let! result = cfg.DeletePortsTree treeId
            let response = req.CreateResponse(HttpStatusCode.OK)

            match result with
            | NoError ->
                response.StatusCode <- HttpStatusCode.OK
                response.writeJsonResponse OK |> ignore
            | _ ->
                let errResponse =
                    result.Handle(log, "Failed to delete ports tree {PortsTree}", treeId)

                response.StatusCode <- errResponse.httpCode
                response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask
