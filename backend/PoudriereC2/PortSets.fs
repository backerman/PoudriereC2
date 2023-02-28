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
            let! portsetsWithMembers =
                portsets
                |> Seq.map (fun (portset: PortSet) ->
                    async {
                        let! members =
                            match portset.Id with
                            | None ->
                                raise (InvalidOperationException("Port set ID is null"))
                            | Some id -> id
                            |> ps.GetPortSetMembers
                        return { portset with Origins = members }
                    })
                |> Async.Parallel
            let response = req.CreateResponse(HttpStatusCode.OK)
            return response.writeJsonResponse portsetsWithMembers
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

    [<Function("UpdatePortSetMembers")>]
    member _.UpdatePortSetMembers
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route="portsets/{portset}/members")>]
         req: HttpRequestData, execContext: FunctionContext, portset: Guid) =
        let log = execContext.GetLogger()
        async {
            let! portSetUpdate = tryDeserialize<PortSetUpdate list> req log
            let response = req.CreateResponse(HttpStatusCode.OK)

            match portSetUpdate with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload")
                |> ignore
            | Some psu ->
                let! result =
                    ps.UpdatePortSetMembers portset psu
                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK |> ignore
                | ForeignKeyViolation ex ->
                    log.LogError
                        ("Failed upsert of port set {PortSet}: {ViolatedConstraint} violated",
                         portset, ex.ConstraintName)
                    response.StatusCode <- HttpStatusCode.UnprocessableEntity
                    response.writeJsonResponse
                        (Error "Referential integrity violation")
                    |> ignore
                | UniqueViolation ex ->
                    // There shouldn't be any way for this to happen since
                    // ON CONFLICT is set to DO NOTHING.
                    response.StatusCode <- HttpStatusCode.UnprocessableEntity
                    log.LogError
                        ("Failed modification of port set {PortSet}: {ViolatedConstraint} violated",
                         portset, ex.ConstraintName)
                    response.writeJsonResponse
                        (Error "Referential integrity violation")
                    |> ignore
                | Unknown ex -> 
                    log.LogError
                        (ex, "Failed modification of port set {PortSet}", portset)
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    response.writeJsonResponse
                        (Error "Internal server error")
                    |> ignore
            return response
        } |> Async.StartAsTask

    [<Function("CreatePortSet")>]
    member _.CreatePortSet([<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route="portsets")>]
    req: HttpRequestData, execContext: FunctionContext) =
        let log = execContext.GetLogger()
        async {
            let! portSet = tryDeserialize<PortSet> req log
            let response = req.CreateResponse(HttpStatusCode.OK)
            match portSet with
            | None ->
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid or nonexistent payload")
                |> ignore
            | Some pst ->
                let! result =
                    ps.CreatePortSet pst.Name
                match result with
                | (NoError, newGuid) ->
                    let! psmResults = ps.UpdatePortSetMembers newGuid [Add pst.Origins]
                    match psmResults with
                    | NoError ->
                        response.StatusCode <- HttpStatusCode.OK
                        Created newGuid
                        |> response.writeJsonResponse
                        |> ignore
                    | ForeignKeyViolation ex ->
                        log.LogError
                            ("Failed upsert of port set {PortSet}: {ViolatedConstraint} violated",
                             newGuid, ex.ConstraintName)
                        response.StatusCode <- HttpStatusCode.UnprocessableEntity
                        response.writeJsonResponse
                            (Error "Referential integrity violation")
                        |> ignore
                    | UniqueViolation ex ->
                        response.StatusCode <- HttpStatusCode.UnprocessableEntity
                        log.LogError
                            ("Failed creation of port set {PortSet}: {ViolatedConstraint} violated",
                             newGuid, ex.ConstraintName)
                        response.writeJsonResponse
                            (Error "Duplicate port origins listed")
                        |> ignore
                    | Unknown ex ->
                        log.LogError
                            (ex, "Failed creation of port set {PortSet}", newGuid)
                        response.StatusCode <- HttpStatusCode.InternalServerError
                        response.writeJsonResponse
                            (Error "Internal server error")
                        |> ignore
                | (ForeignKeyViolation ex, _)->
                    log.LogError
                        ("Failed upsert of port set {PortSet}: {ViolatedConstraint} violated",
                         pst.Id, ex.ConstraintName)
                    response.StatusCode <- HttpStatusCode.UnprocessableEntity
                    response.writeJsonResponse
                        (Error "Referential integrity violation")
                    |> ignore
                | (UniqueViolation ex, _) ->
                    // There shouldn't be any way for this to happen. At all.
                    response.StatusCode <- HttpStatusCode.UnprocessableEntity
                    log.LogError
                        ("Failed creation of port set {PortSet}: {ViolatedConstraint} violated",
                         pst.Id, ex.ConstraintName)
                    response.writeJsonResponse
                        (Error "Referential integrity violation")
                    |> ignore
                | (Unknown ex, _) -> 
                    log.LogError
                        (ex, "Failed creation of port set {PortSet}", pst.Name)
                    response.StatusCode <- HttpStatusCode.InternalServerError
                    response.writeJsonResponse
                        (Error "Internal server error")
                    |> ignore
            return response
        } |> Async.StartAsTask

    [<Function("DeletePortSet")>]
    member _.DeletePortSet([<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route="portsets/{portset}")>]
    req: HttpRequestData, execContext: FunctionContext, portset: Guid) =
        let log = execContext.GetLogger()
        async {
            let response = req.CreateResponse(HttpStatusCode.OK)
            let! result =
                ps.DeletePortSet portset
            match result with
            | NoError ->
                response.StatusCode <- HttpStatusCode.OK
                response.writeJsonResponse OK |> ignore
            | ForeignKeyViolation ex ->
                log.LogError
                    ("Failed deletion of port set {PortSet}: {ViolatedConstraint} violated",
                     portset, ex.ConstraintName)
                response.StatusCode <- HttpStatusCode.UnprocessableEntity
                response.writeJsonResponse
                    (Error "Referential integrity violation")
                |> ignore
            | UniqueViolation ex ->
                // There shouldn't be any way for this to happen. At all.
                response.StatusCode <- HttpStatusCode.UnprocessableEntity
                log.LogError
                    ("Failed deletion of port set {PortSet}: {ViolatedConstraint} violated",
                     portset, ex.ConstraintName)
                response.writeJsonResponse
                    (Error "Referential integrity violation")
                |> ignore
            | Unknown ex -> 
                log.LogError
                    (ex, "Failed deletion of port set {PortSet}", portset)
                response.StatusCode <- HttpStatusCode.InternalServerError
                response.writeJsonResponse
                    (Error "Internal server error")
                |> ignore
            return response
        } |> Async.StartAsTask