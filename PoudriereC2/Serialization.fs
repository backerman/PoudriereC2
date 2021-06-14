module Facefault.PoudriereC2.Serialization

open Microsoft.Extensions.Logging
open Microsoft.Azure.Functions.Worker.Http
open System.Text.Json
open System.Net
open System.Globalization

let tryDeserialize<'T> (req: HttpRequestData) (log: ILogger) =
    async {
        let! tryDeserialize =
            (JsonSerializer.DeserializeAsync<'T>
                (req.Body, eventSerializationOptions)).AsTask()
            |> Async.AwaitTask
            |> Async.Catch
        let res =
            match tryDeserialize with
            | Choice1Of2 successResult -> Some successResult
            | Choice2Of2 e ->
                log.LogError
                    (e, "Failed deserialization")
                None
        return res
    }

type HttpResponseData with
    member this.writeJsonResponse (body: obj) =
        match this.StatusCode with
        | HttpStatusCode.NoContent ->
            // Can't write to a 204 per specification.
            ()
        | _ ->
            this.Headers.Add("Content-Type", "application/json; charset=utf-8")
            JsonSerializer.Serialize(body, eventSerializationOptions)
            |> this.WriteString
        this
