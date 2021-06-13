module Facefault.PoudriereC2.Serialization

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System.Text.Json

let tryDeserialize<'T> (req: HttpRequest) (log: ILogger) =
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