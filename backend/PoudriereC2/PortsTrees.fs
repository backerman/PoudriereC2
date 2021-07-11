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
        ([<HttpTrigger(AuthorizationLevel.Function, "put", Route="portstrees")>]
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
                let! result = cfg.addPortsTrees [meta]
                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK |> ignore
                | ForeignKeyViolation ex ->
                    log.LogError
                        ("Failed upsert of ports tree {PortsTree}: {ViolatedConstraint} violated",
                         meta.Name, ex.ConstraintName)
                    response.StatusCode <- HttpStatusCode.UnprocessableEntity
                    response.writeJsonResponse
                        (Error "Referential integrity violation")
                    |> ignore
                | UniqueViolation ex ->
                    response.StatusCode <- HttpStatusCode.UnprocessableEntity
                    let errorText =
                        match ex.ConstraintName with
                        | "portstrees_pk" -> "name"
                        | _ -> ""
                    log.LogError
                        (ex, "Failed upsert of ports tree {ConfigFile}: {ViolatedConstraint} violated",
                         meta.Name, ex.ConstraintName)
                    response.writeJsonResponse
                        (Error $"Ports tree {meta.Name}: {errorText} already exists")
                    |> ignore
                | Unknown ex ->
                    log.LogError
                        (ex, "Failed insert of ports tree {ConfigFile}", meta.Name)
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    response.writeJsonResponse
                        (Error "Internal server error")
                    |> ignore
            return response
        } |> Async.StartAsTask

    [<Function("GetPortsTrees")>]
    member _.GetPortsTrees
        ([<HttpTrigger(AuthorizationLevel.Function, "get", Route="portstrees/{treeName?}")>]
         req: HttpRequestData, execContext: FunctionContext, treeName: string) =
        let log = execContext.GetLogger()
        let treeNameOpt =
            match treeName with
            | null -> None
            | _ -> Some treeName
        async {
            let! files = cfg.getPortsTrees() // FIXME ignores optional parameter
            let response = req.CreateResponse(HttpStatusCode.OK)
            return response.writeJsonResponse files
        } |> Async.StartAsTask

    [<Function("ModifyPortsTree")>]
    member _.ModifyPortsTree
        ([<HttpTrigger(AuthorizationLevel.Function, "patch", Route="portstrees/{treeName}")>]
         req: HttpRequestData, execContext: FunctionContext, treeName: string) =
        let log = execContext.GetLogger()
        async {
            let! maybeConfig = tryDeserialize<PortsTree> req log
            match maybeConfig with
            | None ->
                req.CreateResponse
                    (HttpStatusCode.BadRequest)
                |> ignore
            | Some meta ->
                let! result = cfg.updatePortsTree treeName meta
                match result with
                | NoError ->
                    req.CreateResponse
                        (HttpStatusCode.OK)
                    |> ignore
                | ForeignKeyViolation ex ->
                    log.LogError
                        ("Failed upsert of ports tree {PortsTree}: {ViolatedConstraint} violated",
                         meta.Name, ex.ConstraintName)
                    req.CreateResponse
                        (HttpStatusCode.UnprocessableEntity)
                    |> ignore
                | UniqueViolation ex ->
                    req.CreateResponse
                        (HttpStatusCode.UnprocessableEntity)
                    |> ignore
                | Unknown ex ->
                    log.LogError
                        (ex, "Failed insert of ports tree {ConfigFile}", meta.Name)
                    req.CreateResponse
                        (HttpStatusCode.InternalServerError)
                    |> ignore
            return req.CreateResponse (HttpStatusCode.OK) |> ignore
        } |> Async.StartAsTask