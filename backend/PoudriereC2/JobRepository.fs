namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open System.Linq

type JobRepository (db: DB.dataContext) =

    /// Function that gets a specific job's metadata.
    member _.GetJob(id: Guid) : Async<JobConfig option> =
        async {
            let! job =
                query {
                    for j in db.Poudrierec2.Jobconfigs do
                    where (j.Id = id)
                    select (j.MapTo<JobConfig>())
                } |> List.executeQueryAsync
            return
                match job.Length with
                | 0 -> None
                | _ -> job.First() |> Some
        }

    /// Function that gets a VM's next available job.
    member _.GetNextJob(vmId: Guid) : Async<JobConfig option> = 
        async {
            return Some {
                Id = vmId
                Deleted = false
                Title = "Build a thing with ports"
                PortsTree = Guid "local"
                PortSet = Guid "server"
                Jail = "13_0-amd64"
                ConfigFiles = []
            }
        }