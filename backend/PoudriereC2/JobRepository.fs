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
    member _.GetNextJob(vmId: Guid) : Async<ConfigFileMetadata list> = 
        async {
            let nextJobQuery =
                query {
                    for jr in db.Poudrierec2.Jobruns do
                    where ((jr.Virtualmachine = Some vmId) && (jr.Started.IsNone))
                    sortBy jr.Requested
                    select jr
                    take 1
                }
            let! myJob = Seq.tryHeadAsync nextJobQuery

            let! configFilesEntities =
                query {
                    // Can't figure out how to execute this query conditionally
                    // so it's more complicated than should be necessary.
                    for jr in nextJobQuery do
                    for jccf in db.Poudrierec2.JobconfigsConfigfiles do
                    where (jccf.IdJobconfigs = jr.Jobconfig)
                    join cf in db.Poudrierec2.Configfiles on (jccf.IdConfigfiles = cf.Id)
                    select cf
                } |> List.executeQueryAsync

            let configFiles =
                configFilesEntities
                |> List.map
                    (fun (cf) ->
                        { Id = cf.Id
                          Name = cf.Name
                          Jail = cf.Jail
                          PortsTree = cf.Portstree
                          PortSet = cf.Portset
                          Deleted = cf.Deleted
                          FileType = FromString<ConfigFileType>(cf.Configtype) })
            return configFiles
        }