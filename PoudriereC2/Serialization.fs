module Facefault.PoudriereC2.Serialization

open Microsoft.Extensions.Logging
open Microsoft.Azure.Functions.Worker.Http
open System.Text.Json
open System.Net
open System.Globalization
open Microsoft.Net.Http.Headers
open Microsoft.Extensions.Primitives
open System.Text

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

    member this.writeTextResponse (body: string) =
        match this.StatusCode with
        | HttpStatusCode.NoContent ->
            // Can't write to a 204 per specification.
            ()
        | _ ->
            this.Headers.Add("Content-Type", "text/plain; charset=utf-8")
            this.WriteString body
        this

let private posixSpecials =
    ['|'; '&'; ';'; '<'; '>'; '('; ')'; '$'; '`'; '\\'; '"'; '\''; ' '; '\r'; '\n';
    '*'; '?'; '['; '#'; '~'; '='; '%' ]

let private posixEscapeInQuotes =
    ['"'; '$'; '`'; '\\']

type System.String with
    /// Escape all shell special characters per POSIX.1-2017 by enclosing
    /// the string in quotes iff necessary.
    member this.ShellQuote() =
        if String.exists (fun c -> Seq.contains c posixSpecials) this then
            // quote it
            let sb = new StringBuilder()
            sb.Append '"' |> ignore
            this
            |> String.iter
                (fun c ->
                    if Seq.contains c posixEscapeInQuotes then
                        sb.Append '\\' |> ignore
                    sb.Append c |> ignore)
            sb.Append('"').ToString()
        else
            // No need for quoting
            this

let private applicationJson = 
    StringSegment "application/json"
    |> MediaTypeHeaderValue.Parse 

let private textPlain =
    StringSegment "text/plain"
    |> MediaTypeHeaderValue.Parse 

type ReturnMediaType =
    | Json
    | PlainText
    | AnyType

/// Given the client's HTTP request, parse the Accept header and return
/// the ReturnMediaType matching the one and only type listed.
let pickReturnMediaType (req: HttpRequestData) =
    let typeMatches m =
        Option.bind
            (fun (mt: MediaTypeHeaderValue) -> 
                if mt.MatchesAllTypes then Some AnyType
                elif mt.IsSubsetOf applicationJson then Some Json
                elif mt.IsSubsetOf textPlain then Some PlainText
                else None) m

    let parsedMediaType strType =
        Option.bind
            (fun s ->
                s
                |> StringSegment
                |> MediaTypeHeaderValue.Parse
                |> Some) strType

    req.Headers.GetValues(HeaderNames.Accept)
    |> Seq.tryExactlyOne
    |> parsedMediaType
    |> typeMatches