module PoudriereC2.Tests.PortSetRepository

open NUnit.Framework
open FsUnit
open System
open Facefault.PoudriereC2
open System

[<TestFixture; NonParallelizable>]
type PortSetRepositoryTests() =
    let testScope: TestScope = new TestScope()

    [<OneTimeSetUp>]
    member _.Setup() =
        async { do! testScope.createDatabaseAsync () }

    [<OneTimeTearDown>]
    member _.TestSomething() =
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

    [<Test>]
    member _.``Can get a list of portsets``() =
        async {
            let ds = testScope.DataSource
            let repo = PortSetRepository(null, ds)
            let! portSets = repo.GetPortSets(None)
            portSets |> should haveLength 2
            portSets.[1].Name |> should equal "田中太郎"
        }

    [<Test>]
    member _.``Can get portsets' members``() =
        async {
            let ds = testScope.DataSource
            let repo = PortSetRepository(null, ds)
            let! portSets = repo.GetPortSets(None)
            let! portSetMembers = repo.GetPortSetMembers(portSets |> List.map (fun ps -> ps.Id.Value))
            portSetMembers.Keys.Count |> should equal 2 // should haveCount doesn't work?
            portSetMembers.[portSets.[0].Id.Value] |> should haveLength 3
            portSetMembers.[portSets.[1].Id.Value] |> should haveLength 4
        }
