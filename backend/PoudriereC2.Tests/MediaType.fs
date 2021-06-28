module PoudriereC2.Tests.MediaType

open NUnit.Framework
open FsUnit
open AutoFixture
open AutoFixture.AutoFoq
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker.Http
open AutoFixture.Kernel
open Facefault.PoudriereC2.Tests.RequestData

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