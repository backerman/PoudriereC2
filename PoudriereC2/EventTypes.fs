namespace Facefault.PoudriereC2

open System
open System.Text.Json
open System.Text.Json.Serialization

[<AutoOpen>]
module Types =

    /// A client heartbeat.
    type HeartbeatInfo =
        { /// The 1-, 5-, and 15-minute load averages of the VM.
          LoadAverage: float list;
          /// The number of CPUs in the VM.
          NumCPUs: int;
          /// The size of the VM (e.g. "Standard_D96as_v4").
          VmSize: string }

    /// Payload for a jail event.
    type JailInfo =
        { MountPath: string }

    /// Payload for a builder event.
    type BuilderInfo =
        { Id: int;
          MountPath: string }

    [<Literal>]
    let private EventType = "type"

    /// The type of an event and event-specific payload.
    type EventDetail =
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

    /// A VM event.
    type Event =
        {
          /// The client timestamp.
          Timestamp: DateTime;
          /// The GUID of the client's Azure VM instance (from IMDS).
          VmGuid: Guid;
          /// The name of the client's Azure VM instance (from IMDS).
          VmName: string;
          /// The event type and payload.
          Event: EventDetail }

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
    
    /// The JSON serialization options to use for types in this module.
    let eventSerializationOptions = serializationOptions EventType