namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open System.Linq

type JobRepository(db: DB.dataContext) =

    /// Function that gets a specific job's metadata.
    member _.GetJob(id: Guid) : Async<JobConfig option> =
        async {
            let! job =
                query {
                    for j in db.Poudrierec2.Jobconfigs do
                        where (j.Id = id)
                        select (j.MapTo<JobConfig>())
                }
                |> List.executeQueryAsync

            return
                match job.Length with
                | 0 -> None
                | _ -> job.First() |> Some
        }

    /// Function that gets a VM's next available job.
    [<Authorize(AuthorizationPolicy.Machine)>]
    member _.GetNextJob(vmId: Guid) : Async<DatabaseError * ConfigFileMetadata list> =
        async {
            let mutable dbError = NoError

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
                }
                |> List.executeQueryAsync

            let configFiles =
                configFilesEntities
                |> List.map (fun (cf) ->
                    { Id = Some cf.Id
                      Name = cf.Name
                      Jail = cf.Jail
                      JailName = None // FIXME?
                      PortsTree = cf.Portstree
                      PortsTreeName = None
                      PortSet = cf.Portset
                      PortSetName = None
                      Deleted = cf.Deleted
                      FileType = FromString<ConfigFileType>(cf.Configtype) })

            match myJob with
            | Some job ->
                job.Started <- Some DateTime.Now
                let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())

                if result <> NoError then
                    db.ClearUpdates() |> ignore

                dbError <- result
            | None -> ()

            return (dbError, configFiles)
        }

    member _.CompleteJob(vmId: Guid) : Async<bool * DatabaseError> =
        async {
            let! thisJob =
                query {
                    for j in db.Poudrierec2.Jobruns do
                        where (j.Virtualmachine = Some vmId && j.Started.IsSome && j.Completed.IsNone)
                        select j
                }
                |> Seq.tryExactlyOneAsync
            // result must be declared mutable because otherwise we'd need to
            // let! within its binding, and you can't await an async within
            // a let statement.
            let mutable present, result = (true, NoError)

            match thisJob with
            | None ->
                // Better error handling?
                present <- false
                result <- Unknown(Exception "Job not found.")
            | Some j ->
                present <- true
                j.Completed <- Some DateTime.Now
                let! aResult = DatabaseError.FromQuery(db.SubmitUpdatesAsync())
                result <- aResult

                if result <> NoError then
                    db.ClearUpdates() |> ignore

            return (present, result)
        }
