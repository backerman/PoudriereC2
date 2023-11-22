module PoudriereC2.Tests.JailRepository

open NUnit.Framework
open System
open Facefault.PoudriereC2
open FsUnit
open Facefault.PoudriereC2.Database

[<TestFixture>]
type JailRepositoryTests() =
    let testScope: TestScope = new TestScope()

    [<OneTimeSetUp>]
    member _.Setup() =
        async { do! testScope.createDatabaseAsync () }

    [<OneTimeTearDown>]
    member _.OneTimeTearDown() = (testScope :> IDisposable).Dispose()

    [<Test>]
    member _.``Can get a list of jails``() =
        async {
            let ds = testScope.DataSource
            let repo = JailRepository(ds)
            let! jails = repo.GetJails()
            jails |> should haveCount 1

            jails
            |> Seq.head
            |> should
                equal
                { Id = Guid "0a62a787-c7c1-48bc-8ba0-90d7fbe9c098" |> Some
                  Name = "13_0-amd64"
                  PortableName = "130-amd64"
                  Version = Some "13.0-RELEASE"
                  Architecture = Some "amd64.amd64"
                  Method = None
                  Url = None
                  Path = None }
        }

    [<Test>]
    member _.``Can update a jail``() =
        async {
            let ds = testScope.DataSource
            let repo = JailRepository(ds)
            let jailToUpdate = Guid "0a62a787-c7c1-48bc-8ba0-90d7fbe9c098"

            let updatedJail =
                { Id = jailToUpdate |> Some
                  Name = "';-- DROP TABLE jails;--"
                  PortableName = "littlebobbyjails"
                  Version = Some "14.0-RC1"
                  Architecture = Some "6502"
                  Method = Some Tar
                  Url = None
                  Path = Some "/dev/null" }

            let! err = repo.UpdateJail jailToUpdate updatedJail
            err |> should equal NoError
            let! jails = repo.GetJails()
            let roundtrippedJail = jails |> Seq.find (fun j -> j.Id = Some jailToUpdate)
            roundtrippedJail |> should equal updatedJail
        }
