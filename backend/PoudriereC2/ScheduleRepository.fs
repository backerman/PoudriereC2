namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open System.Linq

type ScheduleRepository (db: DB.dataContext) =

    /// Function that creates a new schedule.
    member _.ScheduleJob(sched: JobSchedule) : Async<DatabaseError> =
        async {
            db.Poudrierec2.Schedules.Create(sched.JobId, sched.RunAt) |> ignore
            let! result = db.SubmitUpdatesAsync() |> DatabaseError.FromQuery
            return result
        }

    /// Function that gets all active schedules.
    member _.GetSchedules() : Async<JobSchedule list> = 
        async {
            let! scheds =
                query {
                    for s in db.Poudrierec2.Schedules do
                    sortBy s.Jobconfig
                    select {
                        JobId = s.Jobconfig
                        RunAt = s.Runat
                    }
                } |> Seq.executeQueryAsync
            return scheds |> List.ofSeq
        }