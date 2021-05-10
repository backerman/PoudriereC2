namespace Facefault.PoudriereC2
open System

[<AutoOpen>]
module ConfigTypes =

    type JobSchedule =
        { RunAt: string }

    type JobConfig =
        { Id: Guid 
          Title: string
          Type: string
          PoudriereConf: Guid
          MakeConfs: Guid list
          PackageList: Guid
          Schedule: JobSchedule list }