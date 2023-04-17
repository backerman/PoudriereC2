[<AutoOpen>]
module Facefault.PoudriereC2.Types

open System
open System.Text.Json
open System.Text.Json.Serialization

/// A virtual machine (as passed in request)
type VirtualMachineInfo =
    {
        /// The virtual machine's VM name
        Name: string
        /// The virtual machine's GUID
        VmId: Guid
    }

/// A client heartbeat.
type HeartbeatInfo =
    {
        /// The 1-, 5-, and 15-minute load averages of the VM.
        LoadAverage: float list;
        /// The number of CPUs in the VM.
        NumCPUs: int;
        /// <summary>The size of the VM (e.g. <c>Standard_D96as_v4</c>).</summary>
        VmSize: string
    }

/// Payload for a jail event.
type JailInfo =
    { 
        /// The path at which the jail root is mounted in the host filesystem.
        MountPath: string
    }

/// Payload for a builder event.
type BuilderInfo =
    {
        /// The ID number of the builder jail.
        Id: int;
        /// The path at which the builder jail root is mounted in the host filesystem.
        MountPath: string
    }

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
        Event: EventDetail
    }

/// A standard container for operation results.
type FunctionResult =
    | OK
    | Error of Error: string
    | Created of Guid: Guid

type LowerCaseNamingPolicy() =
    inherit JsonNamingPolicy()
    override _.ConvertName (n: string) : string =
        n.ToLowerInvariant()

let serializationOptions discriminator =
    let serializationOverrides (options: JsonFSharpOptions) = dict [
                (typeof<ConfigFileType>, options
                    .WithUnionEncoding(JsonUnionEncoding.UnwrapFieldlessTags)
                    .WithUnionTagNamingPolicy(LowerCaseNamingPolicy()))
                (typeof<PortSetUpdate>, options
                    .WithUnionTagName("action")
                    .WithUnionTagNamingPolicy(LowerCaseNamingPolicy()))
                (typeof<FunctionResult>, options
                    .WithUnionTagName("result")
                    .WithUnionTagNamingPolicy(LowerCaseNamingPolicy()))
                (typeof<PortsTreeMethod>, options
                    .WithUnionUnwrapFieldlessTags())
                (typeof<JailMethod>, options
                    .WithUnionUnwrapFieldlessTags())
            ]
    let options =
        JsonFSharpOptions.Default()
            .WithSkippableOptionFields(true)
            .WithUnionTagName(discriminator)
            .WithUnionTagNamingPolicy(JsonNamingPolicy.CamelCase)
            .WithUnionEncoding(
                JsonUnionEncoding.InternalTag
                ||| JsonUnionEncoding.NamedFields
                ||| JsonUnionEncoding.UnwrapRecordCases
                ||| JsonUnionEncoding.UnwrapOption
            )
            .WithAllowOverride(true)
            .WithOverrides(serializationOverrides)
            .ToJsonSerializerOptions()
    options.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    options

/// The JSON serialization options to use for types in this module.
let eventSerializationOptions = serializationOptions EventType

/// One execution (or requested execution) of a job.
type JobHistory =
    { JobConfig: Guid
      VirtualMachine: Guid option
      Requested: DateTime
      Started: DateTime
      Completed: DateTime }