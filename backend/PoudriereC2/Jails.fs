namespace Facefault.PoudriereC2

open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Extensions.Logging

open Facefault.PoudriereC2.Database
open Facefault.PoudriereC2.Serialization
open System.Net
open System

type JailApi(jr: JailRepository, fb: FreeBSDInfo) =
    [<Function("GetJails")>]
    member _.getJails
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "jails")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("GetJails")
            let response = req.CreateResponse()
            let! jails = jr.GetJails()
            response.writeJsonResponse jails |> ignore
            return response
        }
        |> Async.StartAsTask

    [<Function("NewJail")>]
    member _.newJail
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "jails")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("NewJail")
            let response = req.CreateResponse()
            let! maybeNewJail = tryDeserialize<Jail> req log

            match maybeNewJail with
            | None ->
                let responseData: FunctionResult = Error "Invalid or nonexistent payload"
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse responseData |> ignore
            | Some newJail ->
                let! result = jr.CreateJail newJail

                match result with
                | (NoError, aGuid) ->
                    response.StatusCode <- HttpStatusCode.OK
                    Created aGuid |> response.writeJsonResponse |> ignore
                | (someError, _) ->
                    let errResponse =
                        someError.Handle(log, "Failed creation of jail {JailName}: {Error}", newJail.Name, someError)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("UpdateJail")>]
    member _.updateJail
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "jails/{jailId:guid}")>] req: HttpRequestData,
            execContext: FunctionContext,
            jailId: Guid
        ) =
        async {
            let log = execContext.GetLogger("UpdateJail")
            let response = req.CreateResponse()
            let! maybeJail = tryDeserialize<Jail> req log

            match maybeJail with
            | None ->
                let responseData = Error "Invalid or nonexistent payload"
                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse responseData |> ignore
            | Some jail ->
                let! result = jr.UpdateJail jailId jail

                match result with
                | NoError ->
                    response.StatusCode <- HttpStatusCode.OK
                    response.writeJsonResponse OK |> ignore
                | someError ->
                    let errResponse =
                        someError.Handle(log, "Failed update of jail {Jail}: {Error}", jail.Name, someError)

                    response.StatusCode <- errResponse.httpCode
                    response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("DeleteJail")>]
    member _.deleteJail
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "jails/{jail:guid}")>] req: HttpRequestData,
            execContext: FunctionContext,
            jail: Guid
        ) =
        async {
            let log = execContext.GetLogger("DeleteJail")
            let response = req.CreateResponse()
            let! result = jr.DeleteJail jail

            match result with
            | NoError ->
                response.StatusCode <- HttpStatusCode.OK
                response.writeJsonResponse OK |> ignore
            | someError ->
                let errResponse =
                    someError.Handle(log, "Failed deletion of jail {Jail}: {Error}", jail, someError)

                response.StatusCode <- errResponse.httpCode
                response.writeJsonResponse errResponse.result |> ignore

            return response
        }
        |> Async.StartAsTask

    [<Function("GetAvailableArchitectures")>]
    member _.getAvailableArchitectures
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "freebsd/arch")>] req: HttpRequestData,
            execContext: FunctionContext
        ) =
        async {
            let log = execContext.GetLogger("GetAvailableArchitectures")
            let response = req.CreateResponse()
            let! architectures = fb.getFreeBSDArchitectures log
            response.Headers.Add("Cache-Control", "max-age=300")
            response.writeJsonResponse architectures |> ignore
            return response
        }
        |> Async.StartAsTask

    [<Function("GetAvailableReleases")>]
    member _.getAvailableReleases
        (
            // fsharplint:disable-next-line TypedItemSpacing
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "freebsd/arch/{arch}/releases")>] req:
                HttpRequestData,
            execContext: FunctionContext,
            arch: string
        ) =
        async {
            let log = execContext.GetLogger("GetAvailableReleases")
            let response = req.CreateResponse()
            // Validate arch value - should consist of only alphanumeric characters and periods,
            // and be less than 40 characters in length - I don't know if there's a spec but this
            // rule works for all current architectures.
            let isValidCharset =
                arch |> String.forall (fun c -> Char.IsLetterOrDigit c || c = '.')

            let isValidLength = arch.Length < 40

            if isValidCharset && isValidLength then
                let! releases = fb.getFreeBSDReleases log arch
                response.Headers.Add("Cache-Control", "max-age=300")
                response.writeJsonResponse releases |> ignore
            else
                log.LogError(
                    "Invalid architecture value supplied; valid charset? {Charset}, valid length? {Length}",
                    isValidCharset,
                    isValidLength
                )

                response.StatusCode <- HttpStatusCode.BadRequest
                response.writeJsonResponse (Error "Invalid architecture value") |> ignore

            return response
        }
        |> Async.StartAsTask
