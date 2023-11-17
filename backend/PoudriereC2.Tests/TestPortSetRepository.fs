module PoudriereC2.Tests.PortSetRepository

open NUnit.Framework
open FsUnit
open System
open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database

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
    member _.``Can get a list of portsets``() =
        async {
            let ds = testScope.DataSource
            let repo = PortSetRepository(ds)
            let! portSets = repo.GetPortSets(None)
            portSets |> should haveLength 3
            portSets.[2].Name |> should equal "田中太郎"
        }

    [<Test; Order 1>]
    member _.``Can get portsets' members``() =
        async {
            let ds = testScope.DataSource
            let repo = PortSetRepository(ds)
            let! portSets = repo.GetPortSets(None)
            let! portSetMembers = repo.GetPortSetMembers(portSets |> List.map (fun ps -> ps.Id.Value))
            portSetMembers.Keys.Count |> should equal 2 // should haveCount doesn't work?
            portSetMembers.[portSets.[0].Id.Value] |> should haveLength 3
            portSetMembers.[portSets.[2].Id.Value] |> should haveLength 4
        }

    [<Test; Order 2>]
    member _.``Can create a portset``() =
        async {
            let ds = testScope.DataSource
            let repo = PortSetRepository(ds)

            let myPortSet =
                { Id = None
                  Name = "Something or another"
                  PortableName = "somethingoranother"
                  Origins = [ "audio/spotify-player"; "editors/vim"; "www/chromium" ] }

            let! (err, newGuid) = repo.CreatePortSet myPortSet
            err |> should equal NoError
            newGuid |> should not' (be null)
            let! roundTripped = repo.GetPortSets(Some newGuid)
            roundTripped |> should haveLength 1
            roundTripped.[0].Name |> should equal myPortSet.Name
            roundTripped.[0].PortableName |> should equal myPortSet.PortableName
            let! roundTrippedOrigins = repo.GetPortSetMembers([ newGuid ])
            roundTrippedOrigins |> should haveCount 1
            roundTrippedOrigins.[newGuid] |> should equal myPortSet.Origins
        }

    [<Test; Order 3>]
    member _.``Can delete a port set``() =
        async {
            let deletablePortSet = Guid "14a6f67a-ed4e-462c-beb1-4d9a751ac339"
            let ds = testScope.DataSource
            let repo = PortSetRepository(ds)
            let! exists = repo.GetPortSets(Some deletablePortSet)
            exists |> should haveLength 1
            exists.Head.Id.Value |> should equal deletablePortSet
            let! err = repo.DeletePortSet(deletablePortSet)
            err |> should equal NoError
            let! checkAgain = repo.GetPortSets(Some deletablePortSet)
            checkAgain |> should haveLength 0
        }
