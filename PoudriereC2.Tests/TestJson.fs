module PoudriereC2.Tests.Json

open NUnit.Framework
open FsUnit
open Facefault.PoudriereC2
open System
open System.Text.Json

[<TestFixture>]
type JsonTests() =

    [<Test>]
    member __.TestHookSerialization () =
        // Relevant portion of DU:
        // | Bulk of
        //     numBuilt: int *
        //     numFailed: int *
        //     numIgnored: int *
        //     numSkipped: int
        let bulkBuild = Bulk(42,86,99,13)
        let expected = """{"type":"bulk","numBuilt":42,"numFailed":86,"numIgnored":99,"numSkipped":13}"""
        let actual = JsonSerializer.Serialize(bulkBuild, eventSerializationOptions)
        Assert.That(actual, Is.EqualTo(expected))

    [<Test>]
    member __.TestHeartbeatSerialization () =
        let aHeartbeat = 
            { LoadAverage = [0.1; 0.2; 0.3];
              NumCPUs = 16;
              VmSize = "Standard_D96as_v4" }
        let anEvent =
            { Timestamp = DateTime.Now;
              VmGuid = Guid("12345678-9abc-def0-1234-56789abcdef0");
              VmName = "cthulhu";
              Event = Heartbeat(aHeartbeat) }
        let actual = JsonSerializer.Serialize(anEvent, eventSerializationOptions)
        let roundTrip = JsonSerializer.Deserialize<Event>(actual, eventSerializationOptions)
        roundTrip |> should equal anEvent

    [<SetUp>]
    member __.setup () =
        FSharpCustomMessageFormatter() |> ignore

