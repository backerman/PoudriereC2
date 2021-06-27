module Facefault.PoudriereC2.Tests.RequestData

open Microsoft.Azure.Functions.Worker.Http
open System
open System.Collections.Generic
open System.IO

type ConcreteResponseData(functionContext) =
    inherit HttpResponseData(functionContext)
    override val Body = new MemoryStream() :> Stream with get, set
    override _.Cookies: HttpCookies = 
        failwith "Not Implemented"
    override val StatusCode = Net.HttpStatusCode.OK with get, set
    override val Headers = HttpHeadersCollection() with get, set

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
