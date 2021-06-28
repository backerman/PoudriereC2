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
          PortSet: string option
          PortsTree: string option
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
          PortsTree: string
          PortSet: string
          Jail: string
          ConfigFiles: ConfigFileMetadata list }
    
    type PortsTreeMethod =
        | Null
        | Git of Url: string
        | Svn of Url: string

        /// Check if this value's URL is valid for the given method.
        member this.isValid =
            match this with
            | Null -> true
            | Git url ->
                let parsedUri = Uri url
                Seq.contains parsedUri.Scheme
                    // These schemes are based on what is allowed in `ports.sh`.
                    ["http"; "https"; "file"; "ssh"; "git"]
            | Svn url ->
                let parsedUri = Uri url
                Seq.contains parsedUri.Scheme
                    ["http"; "https"; "file"; "svn+ssh"; "svn"]
        
        static member FromString (method: string, ?url: string) =
            // FIXME should validate agreement of method and URI.
            if method.StartsWith "git" then
                if url.IsNone then Null
                else Git url.Value
            elif method.StartsWith "svn" then
                if url.IsNone then Null
                else Svn url.Value
            else
                Null

    type PortsTree =
        { Name: string
          Method: PortsTreeMethod }