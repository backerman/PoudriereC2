module PoudriereC2.Tests.ScheduleRepository

open NUnit.Framework
open FsUnit
open System
open Facefault.PoudriereC2

[<TestFixture; NonParallelizable>]
type PortSetRepositoryTests() =
    let testScope: TestScope = new TestScope()

    [<OneTimeSetUp>]
    member _.Setup() =
        async { do! testScope.createDatabaseAsync () }

    [<OneTimeTearDown>]
    member _.OneTimeTearDown() =
        try
            (testScope :> IDisposable).Dispose()
        with :? AggregateException as exn ->
            exn.Flatten().InnerExceptions
            |> Seq.iter (fun e ->
                if (e :? Npgsql.PostgresException) then
                    TestContext.Error.WriteLine(
                        "Unable to drop test database (this message can usually be ignored): {0}",
                        e.Message
                    )
                else
                    raise e)

    [<Test; Order 1>]
    member _.``Can get a list of schedulable jobs``() =
        async {
            let ds = testScope.DataSource
            let repo = ScheduleRepository(ds)
            let! schedulableJobs = repo.GetSchedulableJobs()
            schedulableJobs |> should haveLength 1
            schedulableJobs.[0].Name |> Option.get |> should equal "Configuration with only completed jobs"
        }