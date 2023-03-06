namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging
open System.Net

type PortsTreesApi (cfg: PortsRepository) =
    [<Function("NewPortsTree")>]
    member _.NewPortsTree
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route="portstrees")>]
         req: HttpRequestData, execContext: FunctionContext) =
        async {
            let log = execContext.GetLogger()
            let response = req.CreateResponse(HttpStatusCode.OK)
            let! maybeConfig = tryDeserialize<PortsTree> req log
            match maybeConfig with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse
                    (Error "Invalid or nonexistent payload")
                |> ignore
            | Some meta ->
                let! result = cfg.AddPortsTrees [meta]
                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK
                    |> ignore
                | _ ->
                    let errResponse =
                        result.Handle(log, "Failed to add ports tree {PortsTree}", meta.Name)
                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result
                    |> ignore
            return response
        } |> Async.StartAsTask

    [<Function("GetPortsTrees")>]
    member _.GetPortsTrees
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="portstrees/{treeName?}")>]
         req: HttpRequestData, execContext: FunctionContext, treeName: string) =
        let log = execContext.GetLogger()
        let treeNameOpt =
            match treeName with
            | null -> None
            | _ -> Some treeName
        async {
            let! files = cfg.GetPortsTrees() // FIXME ignores optional parameter
            let response = req.CreateResponse(HttpStatusCode.OK)
            return response.writeJsonResponse files
        } |> Async.StartAsTask

    [<Function("ModifyPortsTree")>]
    member _.ModifyPortsTree
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route="portstrees/{treeName}")>]
         req: HttpRequestData, execContext: FunctionContext, treeName: string) =
        let log = execContext.GetLogger()
        async {
            let! maybeConfig = tryDeserialize<PortsTree> req log
            let response = req.CreateResponse(HttpStatusCode.OK)
            match maybeConfig with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
            | Some meta ->
                let! result = cfg.UpdatePortsTree treeName meta
                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK
                    |> ignore
                | _ ->
                    let errResponse =
                        result.Handle(log, "Failed to modify ports tree {PortsTree}", treeName)
                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result
                    |> ignore
            return response
        } |> Async.StartAsTask