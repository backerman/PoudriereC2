namespace Facefault.PoudriereC2
open System

[<AutoOpen>]
module ConfigTypes =
    open Dapper
    open Data
    open Dapper.FSharp.OptionTypes

    /// Run a specific job with a specific crontab schedule.
    type JobSchedule =
        { JobId: Guid
          RunAt: string }

    type PackageOptions =
        { Id: int
          Category: string option
          Package: string option
          Set: string list
          Unset: string list }

    /// A makefile-style configuration option. It can end with a +, in which case
    /// it will add to the value rather than replacing it.
    type ConfigOption =
        { Name: string
          Value: string }

    /// A type of configuration file
    type ConfigFileType =
        | PoudriereConf
        | MakeConf
        | SrcConf

        override x.ToString () = Data.UnionToString(x)

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

    /// The configuration of a build job.
    type JobConfig =
        { Id: Guid
          Deleted: bool
          Title: string
          PortsTree: Guid
          PortSet: Guid
          Jail: string
          ConfigFiles: Guid list }

    type PortsTreeMethod =
        | Null
        | Git
        | Svn

        override this.ToString () =
          Data.UnionToString(this)

    /// A repository of port definitions that can be built.
    /// Id may only be None when creating a new ports tree, where it will be
    /// ignored.
    type PortsTree =
        { Id: Guid option
          Name: string
          Method: PortsTreeMethod
          Url: string option }
    
    /// A set of ports to be built.
    /// Id may only be None when creating a new port set, where it will be
    /// ignored.
    type PortSet =
        { Id: Guid option
          Name: string
          Origins: string list}

    /// Command to add or delete ports from a port set
    type PortSetUpdate =
        | Add of Ports: string list
        | Delete of Ports: string list

    type JailMethod =
      | Allbsd
      | Ftp
      | FtpArchive
      | Http
      | Freebsdci
      | Url

    /// A jail to build ports in.
    [<CLIMutable>]
    type Jail =
      {
        Id: Guid option
        Name: string
        Version: string
        Architecture: string option
        Method: JailMethod option
        Url: string option
      }

    type AutoTypeHandler<'T>() =
      inherit SqlMapper.TypeHandler<'T>()

      override _.SetValue(param, value) =
        param.Value <- UnionToString value

      override _.Parse(value) =
        value
        |> string
        |> FromString<'T>

    type AutoTypeHandlerOption<'T>() =
      inherit SqlMapper.TypeHandler<Option<'T>>()

      override _.SetValue(param, value) =
        match value with
        | None -> param.Value <- DBNull.Value
        | Some v -> param.Value <- UnionToString v

      override _.Parse(value) =
        match value with
        | null -> None
        | _ -> Some (value |> string |> FromString<'T>)

    type JailMethodTypeHandler = AutoTypeHandler<JailMethod>
    type JailMethodOptionTypeHandler = AutoTypeHandlerOption<JailMethod>