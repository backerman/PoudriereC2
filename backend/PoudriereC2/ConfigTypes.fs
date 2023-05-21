namespace Facefault.PoudriereC2

open System
open System.Text.Json.Serialization
open Dapper
open Data

[<AutoOpen>]
module ConfigTypes =
    open Npgsql

    /// Run a specific job with a specific crontab schedule.
    [<CLIMutable>]
    type JobSchedule =
        {
            Name: string option
            JobId: Guid
            /// The last time this job was started and successfully completed;
            /// will only be populated if provided by GetSchedulableJobs.
            LastFinished: DateTimeOffset option
            RunAt: string
        }

    type PackageOptions =
        { Id: int
          Category: string option
          Package: string option
          Set: string list
          Unset: string list }

    /// A makefile-style configuration option. It can end with a +, in which case
    /// it will add to the value rather than replacing it.
    type ConfigOption = { Name: string; Value: string }

    /// A type of configuration file
    type ConfigFileType =
        | PoudriereConf
        | MakeConf
        | SrcConf

        override x.ToString() = Data.UnionToString(x)

    type ConfigFileMetadata =
        { Id: Guid option
          Deleted: bool
          Name: string
          PortSet: Guid option
          PortSetName: string option
          PortsTree: Guid option
          PortsTreeName: string option
          Jail: Guid option
          JailName: string option
          FileType: ConfigFileType }

    type MakeConfOptions =
        { ConfigOptions: ConfigOption list
          PackageOptions: PackageOptions list }

    type ConfigFile =
        | PoudriereConf of ConfigOption list
        | MakeConf of MakeConfOptions

    /// Updates to a configuration file.
    type ConfigOptionUpdate =
        | Add of Options: ConfigOption list
        | Delete of Options: string list

    /// The configuration of a build job.
    [<CLIMutable>]
    type JobConfig =
        { Id: Guid option
          Deleted: bool
          Name: string
          PortsTree: Guid
          PortsTreeName: string option
          PortSet: Guid
          PortSetName: string option
          Jail: Guid
          JailName: string option }

    type PortsTreeMethod =
        | Null
        | Git
        | Svn

        override this.ToString() = Data.UnionToString(this)

    /// A repository of port definitions that can be built.
    /// Id may only be None when creating a new ports tree, where it will be
    /// ignored.
    [<CLIMutable>]
    type PortsTree =
        { Id: Guid option
          Name: string
          Method: PortsTreeMethod
          Url: string option }

    /// A set of ports to be built.
    /// Id may only be None when creating a new port set, where it will be
    /// ignored.
    [<CLIMutable>]
    type PortSet =
        { Id: Guid option
          Name: string
          Origins: string array }

    /// Command to add or delete ports from a port set
    type PortSetUpdate =
        | Add of Ports: string list
        | Delete of Ports: string list

    type JailMethod =
        | Allbsd
        | [<JsonName("freebsd-ci")>] FreebsdCI
        | Ftp
        | [<JsonName("ftp-archive")>] FtpArchive
        | Git
        | Http
        | Null
        | Src
        | Svn
        | Tar
        | Url

    /// A jail to build ports in.
    [<CLIMutable>]
    type Jail =
        { Id: Guid option
          Name: string
          Version: string option
          Architecture: string option
          Method: JailMethod option
          Url: string option
          Path: string option }

    /// A configuration to send to the worker node.
    [<CLIMutable>]
    type JobInfo =
        { JobId: Guid
          JobName: string
          PortsTree: PortsTree
          PortSet: PortSet
          Jail: Jail
          ConfigFiles: Guid array }

    type UnionAutoTypeHandler<'T>() =
        inherit SqlMapper.TypeHandler<'T>()

        override _.SetValue(param, value) = param.Value <- UnionToString value

        override _.Parse(value) = value |> string |> FromString<'T>

    type UnionOptionAutoTypeHandler<'T>() =
        inherit SqlMapper.TypeHandler<'T option>()

        override _.SetValue(param, value) =
            match value with
            | None -> param.Value <- DBNull.Value
            | Some v -> param.Value <- UnionToString v

        override _.Parse(value) =
            match value with
            | null -> None
            | _ -> Some(value |> string |> FromString<'T>)

    type JailMethodTypeHandler = UnionAutoTypeHandler<JailMethod>
    type JailMethodOptionTypeHandler = UnionOptionAutoTypeHandler<JailMethod>
    type PortsTreeMethodTypeHandler = UnionAutoTypeHandler<PortsTreeMethod>
