namespace Facefault.PoudriereC2
open System

[<AutoOpen>]
module ConfigTypes =

    type JobSchedule =
        { RunAt: string }

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
      { Id: Guid
        Deleted: bool
        Name: string
        Portset: string option
        Porttree: string option
        Jail: string option
        FileType: ConfigFileType }

    type MakeConfOptions =
      { ConfigOptions: ConfigOption list
        PackageOptions: PackageOptions list }

    type ConfigFile =
      | PoudriereConf of ConfigOption list
      | MakeConf of MakeConfOptions

    type JobConfig =
      { Id: Guid
        Title: string
        PortTree: string
        PortSet: string
        Jail: string
        ConfigFiles: ConfigFileMetadata list }
    
    type PortsTreeMethod =
      | Null
      | Git of Uri: string
      | Svn of Uri: string

      /// Check if this value's URI is valid for the given method.
      member this.isValid =
        match this with
        | Null -> true
        | Git uri ->
            let parsedUri = Uri uri
            Seq.contains parsedUri.Scheme
                // These schemes are based on what is allowed in `ports.sh`.
                ["http"; "https"; "file"; "ssh"; "git"]
        | Svn uri ->
            let parsedUri = Uri uri
            Seq.contains parsedUri.Scheme
                ["http"; "https"; "file"; "svn+ssh"; "svn"]

    type PortsTree =
      { Name: string
        Method: PortsTreeMethod }