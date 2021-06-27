module Facefault.PoudriereC2.Tests.RequestData

open Microsoft.Azure.Functions.Worker.Http
open System
open System.Collections.Generic
open System.IO

type ConcreteResponseData(functionContext) =
    inherit HttpResponseData(functionContext)
    override _.Body
        with get (): Stream = 
            failwith "Not Implemented"
        and set (v: Stream): unit = 
            failwith "Not Implemented"
    override _.Cookies: HttpCookies = 
        failwith "Not Implemented"
    override _.Headers
        with get (): HttpHeadersCollection = 
            failwith "Not Implemented"
        and set (v: HttpHeadersCollection): unit = 
            failwith "Not Implemented"
    override this.StatusCode
        with get (): Net.HttpStatusCode = 
            failwith "Not Implemented"
        and set (v: Net.HttpStatusCode): unit = 
            failwith "Not Implemented"

type ConcreteRequestData(functionContext) =
    inherit HttpRequestData(functionContext)
    let functionContext = functionContext
    override val Body =
        new MemoryStream() :> Stream with get
    override val Cookies =
        [] :> IReadOnlyCollection<IHttpCookie> with get
    override this.CreateResponse(): HttpResponseData =
        let resp = ConcreteResponseData(functionContext)
        resp :> HttpResponseData
    override val Headers = HttpHeadersCollection() with get
    override val Identities = 
        [] |> Seq.ofList with get
    override val Method = "GET" with get
    override val Url = 
        Uri("http://localhost") with get
