module PoudriereC2.Tests.JailRepository

open NUnit.Framework
open System
open Facefault.PoudriereC2
open FsUnit

[<TestFixture>]
type JailRepositoryTests() =
    let testScope: TestScope = new TestScope()

    [<OneTimeSetUp>]
    member _.Setup() =
        async { do! testScope.createDatabaseAsync () }

    [<OneTimeTearDown>]
    member _.TestSomething() = (testScope :> IDisposable).Dispose()

    [<Test>]
    member _.TestGetJails() =
        async {
            let ds = testScope.DataSource
            let repo = JailRepository(ds)
            let! jails = repo.GetJails()
            jails |> should haveCount 1

            jails
            |> Seq.head
            |> should
                equal {
                    Id = Guid("0a62a787-c7c1-48bc-8ba0-90d7fbe9c098") |> Some
                    Name = "13_0-amd64"
                    PortableName = "130-amd64"
                    Version = Some "13.0-RELEASE"
                    Architecture = Some "amd64.amd64"
                    Method = None
                    Url = None
                    Path = None
                }
        }
