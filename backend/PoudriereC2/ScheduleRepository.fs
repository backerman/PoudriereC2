namespace Facefault.PoudriereC2

open Dapper
open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open Npgsql

type ScheduleRepository(ds: NpgsqlDataSource) =

    /// Create a new job schedule.
    member _.ScheduleJob(sched: JobSchedule) : Async<DatabaseError> =
        async {
            let query =
                """
                INSERT INTO poudrierec2.schedules (jobconfig, runat)
                VALUES (@JobId, @RunAt)
                """

            use! conn = ds.OpenConnectionAsync()
            let! result = conn.ExecuteAsync(query, sched) |> DatabaseError.FromQuery

            return result
        }

    /// Get all active schedules.
    member _.GetSchedules() : Async<JobSchedule list> =
        async {
            let query =
                """
                SELECT name, id, runat
                FROM   poudrierec2.schedules s
                JOIN   poudrierec2.jobconfigs c ON s.jobconfig = c.id
                WHERE  c.deleted = false
                """

            use! conn = ds.OpenConnectionAsync()
            let! result = query |> conn.QueryAsync<JobSchedule>

            return result |> List.ofSeq
        }

    member _.GetSchedulableJobs() =
        async {
            let query =
                """
                SELECT      jobid, name, last_completed, current_requested, runat
                FROM        poudrierec2.jobs_lastrun_scheduled
                ORDER BY    jobid
                """
            use! conn = ds.OpenConnectionAsync()
            let! result = query |> conn.QueryAsync<JobSchedule>
            return result |> List.ofSeq
        }
