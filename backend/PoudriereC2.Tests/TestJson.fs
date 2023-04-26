module PoudriereC2.Tests.Json

open NUnit.Framework
open FsUnit
open Facefault.PoudriereC2
open System
open System.Text.Json
open System.IO

let canonicalizeJson (json: string) =
    use stream = new MemoryStream()
    use writer = new Utf8JsonWriter(stream)
    JsonDocument.Parse(json).WriteTo(writer)
    writer.Flush()
    stream.Close()
    stream.ToArray() |> Text.Encoding.UTF8.GetString

[<TestFixture>]
type JsonTests() =

    [<Test>]
    member _.TestHookSerialization() =
        // Relevant portion of DU:
        // | Bulk of
        //     numBuilt: int *
        //     numFailed: int *
        //     numIgnored: int *
        //     numSkipped: int
        let bulkBuild = Bulk(42, 86, 99, 13)

        let expected =
            """{"type":"bulk","numBuilt":42,"numFailed":86,"numIgnored":99,"numSkipped":13}"""

        JsonSerializer.Serialize(bulkBuild, eventSerializationOptions)
        |> should equal expected

    [<Test>]
    member _.TestHeartbeatSerialization() =
        let aHeartbeat =
            { LoadAverage = [ 0.1; 0.2; 0.3 ]
              NumCPUs = 16
              VmSize = "Standard_D96as_v4" }

        let anEvent =
            { Timestamp = DateTime.Now
              VmGuid = Guid("12345678-9abc-def0-1234-56789abcdef0")
              VmName = "cthulhu"
              Event = Heartbeat(aHeartbeat) }

        let actual = JsonSerializer.Serialize(anEvent, eventSerializationOptions)

        JsonSerializer.Deserialize<Event>(actual, eventSerializationOptions)
        |> should equal anEvent

    [<Test>]
    member _.TestPortSetUpdateSerialization() =
        let someUpdates: PortSetUpdate list =
            [ Add [ "www/apache24"; "security/tailscale" ]; Delete [ "www/apache24" ] ]

        let expectedSerializations =
            [ """{"action":"add","ports":["www/apache24","security/tailscale"]}"""
              """{"action":"delete","ports":["www/apache24"]}""" ]

        let actualSerializations =
            someUpdates
            |> List.map (fun x -> JsonSerializer.Serialize(x, eventSerializationOptions))

        actualSerializations |> should equal expectedSerializations
        let actualJsonList = "[" + (actualSerializations |> String.concat ", ") + "]"

        JsonSerializer.Deserialize<PortSetUpdate list>(actualJsonList, eventSerializationOptions)
        |> should equal someUpdates

    [<Test>]
    member _.TestFunctionResultSerialization() =
        let successResult = OK
        let successExpected = """{"result":"ok"}"""

        JsonSerializer.Serialize(successResult, eventSerializationOptions)
        |> should equal successExpected

        let failureResult = Error "Trapped in space warped by someone"

        let failureExpected =
            """{"result":"error","error":"Trapped in space warped by someone"}"""

        JsonSerializer.Serialize(failureResult, eventSerializationOptions)
        |> should equal failureExpected

        let guidResult = Created(Guid("12345678-9abc-def0-1234-56789abcdef0"))

        let guidExpected =
            """{"result":"created","guid":"12345678-9abc-def0-1234-56789abcdef0"}"""

        JsonSerializer.Serialize(guidResult, eventSerializationOptions)
        |> should equal guidExpected

    [<Test>]
    member _.TestPortSetSerialization() =
        let somePortSet =
            { Id = Some(Guid("12345678-9abc-def0-1234-56789abcdef0"))
              Name = "test"
              Origins = [ "www/apache24"; "security/tailscale" ] }

        let expected =
            """
            {"id":"12345678-9abc-def0-1234-56789abcdef0","name":"test","origins":["www/apache24","security/tailscale"]}
            """

        JsonSerializer.Serialize(somePortSet, eventSerializationOptions)
        |> should equal (expected.Trim())

        JsonSerializer.Deserialize<PortSet>(expected, eventSerializationOptions)
        |> should equal somePortSet

    [<Test>]
    member _.TestPortSetDeserialization() =
        let noGuidPortSetJson =
            """
            {"name":"test","origins":["www/apache24","security/tailscale"]}
            """
                .Trim()

        let expectedNoGuid =
            { Id = None
              Name = "test"
              Origins = [ "www/apache24"; "security/tailscale" ] }

        JsonSerializer.Deserialize<PortSet>(noGuidPortSetJson, eventSerializationOptions)
        |> should equal expectedNoGuid

        let withGuidPortSetJson =
            """
            {"id":"12345678-9abc-def0-1234-56789abcdef0","name":"test","origins":["www/apache24","security/tailscale"]}
            """

        let expectedWithGuid =
            { Id = Some(Guid "12345678-9abc-def0-1234-56789abcdef0")
              Name = "test"
              Origins = [ "www/apache24"; "security/tailscale" ] }

        JsonSerializer.Deserialize<PortSet>(withGuidPortSetJson, eventSerializationOptions)
        |> should equal expectedWithGuid

    [<Test>]
    member _.TestNullPortsTreeSerialization() =
        let sampleTreeJson =
            """
            {"name": "foo bar baz", "method": "null"}
            """
            |> canonicalizeJson

        let expectedTree =
            { Id = None
              Name = "foo bar baz"
              Method = PortsTreeMethod.Null
              Url = None }

        JsonSerializer.Deserialize<PortsTree>(sampleTreeJson, eventSerializationOptions)
        |> should equal expectedTree

        JsonSerializer.Serialize(expectedTree, eventSerializationOptions)
        |> canonicalizeJson
        |> should equal sampleTreeJson

    [<Test>]
    member _.TestGitPortsTreeSerialization() =
        let sampleTreeJson =
            """
            {"id": "572abd41-a8eb-41f5-9a54-9329513dbba4",
             "name": "foo bar baz!",
             "method": "git",
             "url": "https://github.com/foo/bar.git"}
            """
            |> canonicalizeJson

        let expectedTree =
            { Id = Some(Guid("572abd41-a8eb-41f5-9a54-9329513dbba4"))
              Name = "foo bar baz!"
              Method = PortsTreeMethod.Git
              Url = Some "https://github.com/foo/bar.git" }

        JsonSerializer.Deserialize<PortsTree>(sampleTreeJson, eventSerializationOptions)
        |> should equal expectedTree

        JsonSerializer.Serialize(expectedTree, eventSerializationOptions)
        |> canonicalizeJson
        |> should equal sampleTreeJson

    [<Test>]
    member _.TestJailSerialization() =
        let jsonSamples =
            [ """
            {"id": "c8d14c28-dff3-4a3a-9cd0-42a47224d8aa",
             "name": "次の死にたい奴、前に出ろ！",
             "version": "13.2-RELEASE",
             "method": "http",
             "url": "https://git.freebsd.org/src.git"
            }
            """
              """
            {"id": "2e8b9f94-5d3a-4fdd-b346-e238d640ea48",
             "name": "United States Disciplinary Barracks",
             "version": "6.2-RELEASE",
             "architecture": "sparc64",
             "method": "ftp-archive"
            }
            """ ]
            |> List.map canonicalizeJson

        let expectedJails =
            [ { Id = Some(Guid("c8d14c28-dff3-4a3a-9cd0-42a47224d8aa"))
                Name = "次の死にたい奴、前に出ろ！"
                Version = Some "13.2-RELEASE"
                Method = Some Http
                Architecture = None
                Path = None
                Url = Some "https://git.freebsd.org/src.git" }
              { Id = Some(Guid("2e8b9f94-5d3a-4fdd-b346-e238d640ea48"))
                Name = "United States Disciplinary Barracks"
                Version = Some "6.2-RELEASE"
                Method = Some FtpArchive
                Architecture = Some "sparc64"
                Path = None
                Url = None } ]

        let actualJails =
            jsonSamples
            |> List.map (fun x -> JsonSerializer.Deserialize<Jail>(x, eventSerializationOptions))

        actualJails |> should equal expectedJails

        let roundTrippedJails =
            expectedJails
            |> List.map (fun x -> JsonSerializer.Serialize(x, eventSerializationOptions))

        roundTrippedJails |> List.map canonicalizeJson |> should equal jsonSamples

    [<SetUp>]
    member _.setup() =
        FSharpCustomMessageFormatter() |> ignore