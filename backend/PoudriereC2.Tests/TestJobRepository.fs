module PoudriereC2.Tests.JobRepository

open NUnit.Framework
open System
open Facefault.PoudriereC2
open FsUnit
open Facefault.PoudriereC2.Database

[<TestFixture>]
type JobRepositoryTests() =
    let testScope: TestScope = new TestScope()

    [<OneTimeSetUp>]
    member _.Setup() =
        async { do! testScope.createDatabaseAsync () }

    [<OneTimeTearDown>]
    member _.TestSomething() = (testScope :> IDisposable).Dispose()

    [<Test; Order 1>]
    member _.``Can get job configurations``() =
        async {
            let ds = testScope.DataSource
            let repo = JobRepository(ds)
            let! (err, jobConfigs) = repo.GetJobConfigs()
            err |> should equal NoError
            jobConfigs |> should haveCount 2

            jobConfigs
            |> Seq.head
            |> should
                equal
                { Id = Guid("209fc7b5-18c5-40e1-a205-4ae82790621e") |> Some
                  Deleted = false
                  Name = "Configuration with a currently-running job"
                  PoudriereConf = Guid("97241b1e-9c04-4b58-9cdc-4c90eef35225")
                  PoudriereConfName = Some "Random poudriere.conf"
                  PortsTree = Guid("4e6d2feb-2a99-4bed-8545-d5462c66ba0c")
                  PortsTreeName = Some "main"
                  PortSet = Guid("11a4e47a-e778-4499-8ad3-4ad117fe0a2f")
                  PortSetName = Some "server"
                  Jail = Guid("0a62a787-c7c1-48bc-8ba0-90d7fbe9c098")
                  JailName = Some "13_0-amd64"
                  RunAt = Some "42 3 * * *" }
        }

    [<Test; Order 1>]
    member _.``Can get job details``() =
        async {
            let ds = testScope.DataSource
            let repo = JobRepository(ds)
            let! maybeJobInfo = "209fc7b5-18c5-40e1-a205-4ae82790621e" |> Guid |> repo.GetJobDetails
            maybeJobInfo |> should not' (equal None)
            let jobInfo = maybeJobInfo |> Option.get

            jobInfo
            |> should
                equal
                { JobId = Guid("209fc7b5-18c5-40e1-a205-4ae82790621e")
                  JobName = "Configuration with a currently-running job"
                  PortsTree =
                    { Id = Guid("4e6d2feb-2a99-4bed-8545-d5462c66ba0c") |> Some
                      Name = "main"
                      PortableName = "main"
                      Method = PortsTreeMethod.Git
                      Url = Some "https://git.freebsd.org/ports.git" }
                  PortSet =
                    { Id = Guid("11a4e47a-e778-4499-8ad3-4ad117fe0a2f") |> Some
                      Name = "server"
                      PortableName = "server"
                      Origins = [ "net/rclone"; "security/sssd"; "sysutils/tmux" ] }
                  Jail =
                    { Id = Guid("0a62a787-c7c1-48bc-8ba0-90d7fbe9c098") |> Some
                      Name = "13_0-amd64"
                      PortableName = "130-amd64"
                      Version = Some "13.0-RELEASE"
                      Architecture = Some "amd64.amd64"
                      Method = None
                      Url = None
                      Path = None }
                  ConfigFiles =
                    [ Guid "7557d8a8-bba5-4c99-ba6f-2ffbebb0be63"
                      Guid "97241b1e-9c04-4b58-9cdc-4c90eef35225" ] }

            ()
        }

    [<Test; Order 2>]
    member _.``Can update a job configuration``() =
        async {
            let ds = testScope.DataSource
            let repo = JobRepository(ds)
            // TODO: Get a job config and update it with new values
            let updatedConfig =
                { Id = Guid("209fc7b5-18c5-40e1-a205-4ae82790621e") |> Some
                  Deleted = true
                  Name = "Tanaka Tarō"
                  PoudriereConf = Guid("97241b1e-9c04-4b58-9cdc-4c90eef35225")
                  PoudriereConfName = Some "Random poudriere.conf"
                  PortsTree = Guid("4e6d2feb-2a99-4bed-8545-d5462c66ba0c")
                  PortsTreeName = Some "main"
                  PortSet = Guid("47234d7c-82d1-4ffd-ae18-25aae674f245")
                  PortSetName = Some "田中太郎"
                  Jail = Guid("0a62a787-c7c1-48bc-8ba0-90d7fbe9c098")
                  JailName = Some "13_0-amd64"
                  RunAt = Some "*/2 7 1 * *" }
            let! err = repo.UpdateJobConfig updatedConfig
            err |> should equal NoError
            // TODO: Then get it again and check that the values are updated
            let! (err, jobConfigs) = repo.GetJobConfigs()
            err |> should equal NoError
            jobConfigs |> should haveCount 2
            let roundTrippedConfig: JobConfig = Seq.find (fun jc -> jc.Id = updatedConfig.Id) jobConfigs
            roundTrippedConfig |> should equal updatedConfig
            ()
        }
