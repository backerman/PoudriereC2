module PoudriereC2.Tests.MediaType

open NUnit.Framework
open FsUnit
open AutoFixture
open AutoFixture.AutoFoq
open System
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker.Http
open System.IO
open AutoFixture.Kernel

type ConcreteRequestData(functionContext) =
    inherit HttpRequestData(functionContext)
    override val Body =
        new MemoryStream() :> Stream with get
    override val Cookies =
        [] :> Collections.Generic.IReadOnlyCollection<IHttpCookie> with get
    override this.CreateResponse(): HttpResponseData = 
        failwith "Not Implemented"
    override val Headers = HttpHeadersCollection() with get
    override val Identities = 
        [] |> Seq.ofList with get
    override val Method = "GET" with get
    override val Url = 
        Uri("http://localhost") with get

[<TestFixture>]
type MediaTypeHandlerTests() =
    [<Test>]
    member _.TestGoodValues() =
        let fixture = Fixture()
        AutoFoqCustomization()
        |> fixture.Customize
        |> ignore
        fixture.Customizations.Add(
            TypeRelay(typeof<HttpRequestData>, typeof<ConcreteRequestData>))
        [{|MediaType = "application/json"; Result = Some Json|};
         {|MediaType = "text/plain"; Result = Some PlainText|};
         {|MediaType = "*/*"; Result = Some AnyType|};
         {|MediaType = "application/vnd.visio"; Result = None|}]
        |> Seq.iter
            (fun testCase ->
                let jsonRequest = fixture.Create<HttpRequestData>()
                jsonRequest.Headers.Add("Accept", testCase.MediaType)
                pickReturnMediaType jsonRequest
                |> should equal testCase.Result)
