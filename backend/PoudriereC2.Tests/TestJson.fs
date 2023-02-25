module PoudriereC2.Tests.Json

open NUnit.Framework
open FsUnit
open Facefault.PoudriereC2
open System
open System.Text.Json

[<TestFixture>]
type JsonTests() =

    [<Test>]
    member _.TestHookSerialization () =
        // Relevant portion of DU:
        // | Bulk of
        //     numBuilt: int *
        //     numFailed: int *
        //     numIgnored: int *
        //     numSkipped: int
        let bulkBuild = Bulk(42, 86, 99, 13)
        let expected = """{"type":"bulk","numBuilt":42,"numFailed":86,"numIgnored":99,"numSkipped":13}"""
        JsonSerializer.Serialize(bulkBuild, eventSerializationOptions)
        |> should equal expected

    [<Test>]
    member _.TestHeartbeatSerialization () =
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
        JsonSerializer.Deserialize<Event>(actual, eventSerializationOptions)
        |> should equal anEvent

    [<Test>]
    member _.TestPortSetUpdateSerialization () =
        let someUpdates: PortSetUpdate list =
            [
                Add [ "www/apache24"; "security/tailscale" ] ;
                Delete [ "www/apache24" ]
            ]
        let expectedSerializations =
            [ 
                """{"action":"add","ports":["www/apache24","security/tailscale"]}"""
                """{"action":"delete","ports":["www/apache24"]}"""
            ]
        let actualSerializations =
            someUpdates
            |> List.map (fun x -> JsonSerializer.Serialize(x, eventSerializationOptions))
        actualSerializations
        |> should equal expectedSerializations
        let actualJsonList = "[" + (actualSerializations |> String.concat ", ") + "]" 
        JsonSerializer.Deserialize<PortSetUpdate list>(actualJsonList, eventSerializationOptions)
        |> should equal someUpdates
    
    [<Test>]
    member _.TestFunctionResultSerialization () =
        let successResult = 
            OK
        let successExpected = """{"result":"ok"}"""
        JsonSerializer.Serialize(successResult, eventSerializationOptions)
        |> should equal successExpected
        let failureResult = 
            Error "Trapped in space warped by someone"
        let failureExpected = """{"result":"error","error":"Trapped in space warped by someone"}"""
        JsonSerializer.Serialize(failureResult, eventSerializationOptions)
        |> should equal failureExpected

    [<Test>]

    [<SetUp>]
    member _.setup () =
        FSharpCustomMessageFormatter() |> ignore