module PoudriereC2.Tests.JobRepository

open NUnit.Framework
open System
open Facefault.PoudriereC2
open FsUnit
open Facefault.PoudriereC2.Database

[<TestFixture>]
type JobRepositoryTests() =
    // member val testScope : TestScope = null with get, set
    let testScope : TestScope = new TestScope()

    [<OneTimeSetUp>]
    member _.Setup() =
        async {
            do! testScope.createDatabaseAsync()
        }

    [<OneTimeTearDown>]
    member _.TestSomething() =
        (testScope :> IDisposable).Dispose()

    [<Test>]
    member _.TestGetJobConfig() =
        async {
            let ds = testScope.DataSource
            let repo = JobRepository(ds)
            let! (err, jobConfigs) = repo.GetJobConfigs()
            err |> should equal NoError
            Array.ofSeq jobConfigs |> should haveLength 1
            jobConfigs |> Seq.head |> should equal {
                Id = Guid("209fc7b5-18c5-40e1-a205-4ae82790621e") |> Some
                Deleted = false
                Name = "Yes it's a job configuration!"
                PortsTree = Guid("4e6d2feb-2a99-4bed-8545-d5462c66ba0c")
                PortsTreeName = Some "main"
                PortSet = Guid("11a4e47a-e778-4499-8ad3-4ad117fe0a2f")
                PortSetName = Some "server"
                Jail = Guid("0a62a787-c7c1-48bc-8ba0-90d7fbe9c098")
                JailName = Some "13_0-amd64"
            }
        }