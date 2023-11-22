namespace Facefault.PoudriereC2

open Dapper
open Facefault.PoudriereC2
open Npgsql

type ScheduleRepository(ds: NpgsqlDataSource) =
    member _.GetSchedulableJobs() =
        async {
            let query =
                """
                SELECT      id, name, last_completed, current_requested, runat
                FROM        poudrierec2.jobs_lastrun_scheduled
                WHERE       current_requested IS NULL
                ORDER BY    id
                """
            use! conn = ds.OpenConnectionAsync()
            let! result = query |> conn.QueryAsync<JobSchedule>
            return result |> List.ofSeq
        }
