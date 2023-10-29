module PoudriereC2.Tests.ConfigRepository

open NUnit.Framework
open System
open Facefault.PoudriereC2
open FsUnit

[<TestFixture; NonParallelizable>]
type ConfigRepositoryTests() =
    let testScope: TestScope = new TestScope()

    [<OneTimeSetUp>]
    member _.Setup() =
        async { do! testScope.createDatabaseAsync () }

    [<OneTimeTearDown>]
    member _.TestSomething() = (testScope :> IDisposable).Dispose()

    [<Test; Order 1>]
    member _.``Can get a list of configuration files``() =
        async {
            let ds = testScope.DataSource
            let repo = ConfigRepository(ds)
            let! configFiles = repo.GetConfigFiles()
            let! aConfigFile = repo.GetConfigFiles("7557d8a8-bba5-4c99-ba6f-2ffbebb0be63" |> Guid)

            let sampleConfig =
                { Id = "7557d8a8-bba5-4c99-ba6f-2ffbebb0be63" |> Guid |> Some
                  Deleted = false
                  Name = "Some make.conf"
                  PortSet = None
                  PortSetName = None
                  PortsTree = None
                  PortsTreeName = None
                  Jail = "0a62a787-c7c1-48bc-8ba0-90d7fbe9c098" |> Guid |> Some
                  JailName = Some "13_0-amd64"
                  FileType = ConfigFileType.MakeConf }

            configFiles |> should haveCount 2
            configFiles |> Seq.head |> should equal sampleConfig
            aConfigFile |> should haveCount 1
            aConfigFile |> Seq.head |> should equal sampleConfig
        }

    [<Test; Order 2>]
    member _.``Can create a configuration file``() =
        async {
            let sampleConfig =
                { Id = None
                  Deleted = false
                  Name = "Some other make.conf"
                  PortSet = "11a4e47a-e778-4499-8ad3-4ad117fe0a2f" |> Guid |> Some
                  PortSetName = Some "server"
                  PortsTree = "4e6d2feb-2a99-4bed-8545-d5462c66ba0c" |> Guid |> Some
                  PortsTreeName = Some "main"
                  Jail = None
                  JailName = None
                  FileType = SrcConf }

            let ds = testScope.DataSource
            let repo = ConfigRepository(ds)
            let! (error, guid) = repo.NewConfigFile(sampleConfig)
            error |> should be (sameAs Database.DatabaseError.NoError)
            let! configFiles = repo.GetConfigFiles()
            configFiles |> should haveCount 3

            configFiles
            |> Seq.find (fun c -> c.Id = Some guid)
            |> should equal { sampleConfig with Id = Some guid }
        }

    [<Test; Order 3>]
    member _.``Can modify configuration file metadata``() =
        async {
            let ds = testScope.DataSource
            let repo = ConfigRepository(ds)

            let! configFileSeq = repo.GetConfigFiles("97241b1e-9c04-4b58-9cdc-4c90eef35225" |> Guid)
            configFileSeq |> should haveCount 1
            let configFile = Seq.head configFileSeq
            let modifiedConfigFile = { configFile with Name = "不思議な転校生" }
            let! error = repo.UpdateConfigFile(modifiedConfigFile)
            error |> should be (sameAs Database.DatabaseError.NoError)
            let! modConfigFileSeq = repo.GetConfigFiles("97241b1e-9c04-4b58-9cdc-4c90eef35225" |> Guid)
            modConfigFileSeq |> should haveCount 1
            let modConfigFileActual = Seq.head modConfigFileSeq
            modConfigFileActual |> should equal modifiedConfigFile
        }

    [<Test; Order 3>]
    member _.``Can get and modify configuration file options``() =
        async {
            let ds = testScope.DataSource
            let repo = ConfigRepository(ds)

            let! opts = repo.GetConfigFileOptions("7557d8a8-bba5-4c99-ba6f-2ffbebb0be63" |> Guid)
            opts |> should haveCount 2

            let patches: ConfigOptionUpdate list =
                [ ConfigOptionUpdate.Add
                      [ { Name = "WITH_PKGNG"; Value = "yes" }
                        { Name = "BOOT_COMCONSOLE_SPEED"
                          Value = "38400" } ]
                  ConfigOptionUpdate.Delete [ "NO_MODULES" ] ]

            let! error = repo.UpdateConfigFileOptions ("7557d8a8-bba5-4c99-ba6f-2ffbebb0be63" |> Guid) patches
            error |> should be (sameAs Database.DatabaseError.NoError)
            let! newOpts = repo.GetConfigFileOptions("7557d8a8-bba5-4c99-ba6f-2ffbebb0be63" |> Guid)
            newOpts |> should haveCount 3
        }

    [<Test; Order 4>]
    member _.``Can delete a configuration file``() =
        async {
            let fileToDelete = "97241b1e-9c04-4b58-9cdc-4c90eef35225" |> Guid
            let ds = testScope.DataSource
            let repo = ConfigRepository(ds)
            let! error = repo.DeleteConfigFile(fileToDelete)
            error |> should be (sameAs Database.DatabaseError.NoError)
            let! configFiles = repo.GetConfigFiles(fileToDelete)
            let configFile = Seq.head configFiles
            configFile.Deleted |> should be True
        }
