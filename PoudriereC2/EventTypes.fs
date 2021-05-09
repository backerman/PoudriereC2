namespace Facefault.PoudriereC2

open System
open System.Text.Json
open System.Text.Json.Serialization

[<AutoOpen>]
module Types =

    type HeartbeatInfo =
        { LoadAverage: float list;
          NumCPUs: int;
          VmSize: string }

    type JailInfo =
        { MountPath: string }

    type BuilderInfo =
        { Id: int;
          MountPath: string }

    [<Literal>]
    let private EventType = "type"

    type Hook =
        | Jail of JailInfo
        | Builder of BuilderInfo
        | PkgBuild of
            pkgName: string *
            dependentOrigin: string *
            reason: string *
            phase: string
        | PkgClean of
            deletedPackages: bool *
            builtRepository: bool
        | PkgRepo of
            packagesDirectory: string *
            pkgRepoSigningKey: string *
            pkgRepoFromHost: string *
            pkgRepoMetaFile: string
        | Bulk of
            numBuilt: int *
            numFailed: int *
            numIgnored: int *
            numSkipped: int
        | PortsUpdate
        | Heartbeat of HeartbeatInfo

    type Event =
        { Timestamp: DateTime;
          VmGuid: Guid;
          VmName: string;
          Event: Hook }

    let serializationOptions discriminator =
        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        JsonFSharpConverter(
            unionTagName = discriminator,
            unionTagNamingPolicy = JsonNamingPolicy.CamelCase,
            unionEncoding = (
                JsonUnionEncoding.InternalTag
                ||| JsonUnionEncoding.NamedFields
                ||| JsonUnionEncoding.UnwrapRecordCases
            )
        ) |> options.Converters.Add
        options
    
    let eventSerializationOptions = serializationOptions EventType