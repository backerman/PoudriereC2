namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open System
open Dapper
open Npgsql

type JobRepository(ds: NpgsqlDataSource) =

    /// Get a list of all job configurations.
    member _.GetJobConfigs() =
        async {
            use! conn = ds.OpenConnectionAsync()

            let! queryResult =
                """
                SELECT jc.id id, jc.name name, jc.deleted deleted,
                       ps.id portset, ps.name portsetname,
                       pt.id portstree, pt.name portstreename,
                       j.id jail, j.name jailname
                FROM poudrierec2.jobconfigs jc
                JOIN poudrierec2.portsets ps ON jc.portset = ps.id
                JOIN poudrierec2.portstrees pt ON jc.portstree = pt.id
                JOIN poudrierec2.jails j ON jc.jail = j.id
                ORDER BY name, id
                """
                |> conn.QueryAsync<JobConfig>
                |> Async.AwaitTask
                |> Async.Catch

            return
                match queryResult with
                | Choice1Of2 jobConfigs -> (NoError, jobConfigs)
                | Choice2Of2 e -> (DatabaseError.FromException e, [])
        }

    /// Function that gets a specific job's metadata.
    member _.GetJobConfig(id: Guid) : Async<JobConfig option> =
        async {
            failwith "To be implemented"
            return None
        }

    /// Update a job configuration.
    member _.UpdateJobConfig(jc: JobConfig) : Async<DatabaseError> =
        async {
            use! conn = ds.OpenConnectionAsync()

            let query =
                """
                UPDATE poudrierec2.jobconfigs
                SET name = @name, portset = @portset, portstree = @portstree, jail = @jail
                WHERE id = @id
                """

            let! result = conn.ExecuteAsync(query, jc) |> Async.AwaitTask |> Async.Catch

            return
                match result with
                // assume number of rows is 1. If not, there were no rows updated because
                // the GUID is invalid. This should result in an error, but that will wait
                // for a future release.
                | Choice1Of2 _ -> NoError
                | Choice2Of2 e -> DatabaseError.FromException e
        }

    /// Delete a job configuration.
    member _.DeleteJobConfiguration(id: Guid) : Async<DatabaseError> =
        async {
            use! conn = ds.OpenConnectionAsync()

            let query =
                """
                UPDATE poudrierec2.jobconfigs
                SET deleted = true
                WHERE id = @id
                """

            let! result = conn.ExecuteAsync(query, {| id = id |}) |> Async.AwaitTask |> Async.Catch

            return
                match result with
                // assume number of rows is 1. If not, there were no rows updated because
                // the GUID is invalid. This should result in an error, but that will wait
                // for a future release.
                | Choice1Of2 _ -> NoError
                | Choice2Of2 e -> DatabaseError.FromException e
        }

    member _.CreateJobConfiguration(jc: JobConfig) =
        async {
            use! conn = ds.OpenConnectionAsync()
            let newGuid = Guid.NewGuid()
            let query =
                """ INSERT INTO poudrierec2.jobconfigs
                    (id, name, portset, portstree, jail)
                    VALUES (@id, @name, @portset, @portstree, @jail)
                """
            let! result =
                conn.ExecuteAsync(query, {jc with Id = Some newGuid})
                |> DatabaseError.FromQuery
            return (result, newGuid)
        }

    /// Function that gets a VM's next available job.
    member _.GetNextJob(vmId: Guid) : Async<DatabaseError * ConfigFileMetadata list> =
        async {
            failwith "To be implemented"
            return (NoError, [])
        }

    member _.CompleteJob(vmId: Guid) : Async<bool * DatabaseError> =
        async {
            failwith "To be implemented"
            return (false, NoError)
        }
