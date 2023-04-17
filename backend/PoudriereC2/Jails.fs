namespace Facefault.PoudriereC2

open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open System.Net
open System

type JailApi (jr: JailRepository) =
    [<Function("GetJails")>]
    member _.getJails
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="jails")>]
         req: HttpRequestData, execContext: FunctionContext) =
        async {
            let log = execContext.GetLogger()
            let response = req.CreateResponse()
            let! jails = jr.GetJails()
            response.writeJsonResponse jails |> ignore
            return response
        } |> Async.StartAsTask
    
    [<Function("NewJail")>]
    member _.newJail
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route="jails")>]
         req: HttpRequestData, execContext: FunctionContext) =
        async {
            let log = execContext.GetLogger()
            let response = req.CreateResponse()
            let! maybeNewJail = tryDeserialize<Jail> req log
            match maybeNewJail with
            | None ->
                let responseData : FunctionResult = Error "Invalid or nonexistent payload"
                response.writeJsonResponse responseData |> ignore
            | Some newJail ->
                let! result = jr.CreateJail newJail
                match result with
                | (NoError, aGuid) ->
                    response.StatusCode <- HttpStatusCode.OK
                    Created aGuid
                    |> response.writeJsonResponse
                    |> ignore
                | (someError, _) ->
                    let errResponse =
                        someError.Handle
                            (log, "Failed creation of jail {JailName}: {Error}", newJail.Name, someError)
                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result
                    |> ignore
            return response
        } |> Async.StartAsTask

    [<Function("UpdateJail")>]
    member _.updateJail
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route="jails/{jailId:guid}")>]
         req: HttpRequestData, execContext: FunctionContext, jailId: Guid) =
        async {
            let log = execContext.GetLogger()
            let response = req.CreateResponse()
            let! maybeJail = tryDeserialize<Jail> req log
            match maybeJail with
            | None ->
                let responseData = Error "Invalid or nonexistent payload"
                response.writeJsonResponse responseData |> ignore
            | Some jail ->
                let! result = jr.UpdateJail jailId jail
                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK
                    |> ignore
                | someError ->
                    let errResponse =
                        someError.Handle
                            (log, "Failed update of jail {Jail}: {Error}", jail.Name, someError)
                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result
                    |> ignore
            return response
        } |> Async.StartAsTask

    [<Function("DeleteJail")>]
    member _.deleteJail
        ([<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route="jails/{jail:guid}")>]
         req: HttpRequestData, execContext: FunctionContext, jail: Guid) =
        async {
            let log = execContext.GetLogger()
            let response = req.CreateResponse()
            let! result = jr.DeleteJail jail
            match result with
            | NoError ->
                response.StatusCode <- HttpStatusCode.OK
                response.writeJsonResponse OK
                |> ignore
            | someError ->
                let errResponse =
                    someError.Handle
                        (log, "Failed deletion of jail {Jail}: {Error}", jail, someError)
                response.StatusCode <- errResponse.httpCode
                response.writeJsonResponse errResponse.result
                |> ignore
            return response
        } |> Async.StartAsTask