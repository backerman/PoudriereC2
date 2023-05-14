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

            let query = """SELECT * FROM poudrierec2.jobconfigs ORDER BY name, id"""
            let! queryResult = conn.QueryAsync<JobConfig> query |> Async.AwaitTask |> Async.Catch

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
