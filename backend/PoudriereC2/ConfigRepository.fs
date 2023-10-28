namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open System.Linq

type ConfigRepository(db: DB.dataContext) =

    member _.GetConfigFileOptions(configFile: Guid) =
        async {
            let! opts =
                query {
                    for configOption in db.Poudrierec2.Configoptions do
                        where (configOption.Configfile = configFile)
                        sortBy configOption.Name

                        select
                            { Name = configOption.Name
                              Value = configOption.Value }
                }
                |> Seq.executeQueryAsync

            return opts
        }

    member _.GetConfigFiles(?configFile: Guid) =
        async {
            let filterQuery =
                match configFile with
                | None -> <@ fun (_: DB.dataContext.``poudrierec2.configfilesEntity``) -> true @>
                | Some f -> <@ fun (file: DB.dataContext.``poudrierec2.configfilesEntity``) -> file.Id = f @>

            let! configFiles =
                query {
                    for file in db.Poudrierec2.Configfiles do
                        where ((%filterQuery) file)
                        join portSet in (!!) db.Poudrierec2.Portsets on (file.Portset = Some portSet.Id)
                        join portsTree in (!!) db.Poudrierec2.Portstrees on (file.Portstree = Some portsTree.Id)
                        join jail in (!!) db.Poudrierec2.Jails on (file.Jail = Some jail.Id)
                        sortBy file.Id

                        select
                            { Id = Some file.Id
                              Deleted = file.Deleted
                              Name = file.Name
                              PortSet = file.Portset
                              PortSetName =
                                match portSet with
                                | null -> None
                                | _ -> Some portSet.Name
                              PortsTree = file.Portstree
                              PortsTreeName =
                                match portsTree with
                                | null -> None
                                | _ -> Some portsTree.Name
                              Jail = file.Jail
                              JailName =
                                match jail with
                                | null -> None
                                | _ -> Some jail.Name
                              FileType = FromString<ConfigFileType> file.Configtype }
                }
                |> Seq.executeQueryAsync

            return configFiles
        }

    member _.NewConfigFile(metadata: ConfigFileMetadata) =
        async {
            let guid = Guid.NewGuid()
            let row = db.Poudrierec2.Configfiles.Create()
            row.Name <- metadata.Name
            row.Deleted <- false
            row.Id <- guid
            row.Jail <- metadata.Jail
            row.Portset <- metadata.PortSet
            row.Portstree <- metadata.PortsTree
            row.Configtype <- UnionToString metadata.FileType

            row.OnConflict <- Common.OnConflict.Update
            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())

            if result <> NoError then
                db.ClearUpdates() |> ignore

            return (result, row.Id)
        }

    member _.UpdateConfigFile(metadata: ConfigFileMetadata) : Async<DatabaseError> =
        async {
            let rowGuid =
                match metadata.Id with
                | Some id -> id
                | None -> raise (ArgumentException("Config file metadata must have an ID"))

            let! row =
                query {
                    for file in db.Poudrierec2.Configfiles do
                        where (file.Id = rowGuid)
                        select file
                }
                |> Seq.exactlyOneAsync

            row.Name <- metadata.Name
            row.Deleted <- metadata.Deleted
            row.Jail <- metadata.Jail
            row.Portset <- metadata.PortSet
            row.Portstree <- metadata.PortsTree
            row.Configtype <- UnionToString metadata.FileType

            row.OnConflict <- Common.OnConflict.Throw
            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())

            if result <> NoError then
                db.ClearUpdates() |> ignore

            return result
        }

    member _.DeleteConfigFile(configFile: Guid) : Async<DatabaseError> =
        async {
            let! row =
                query {
                    for file in db.Poudrierec2.Configfiles do
                        where (file.Id = configFile)
                        select file
                }
                |> Seq.exactlyOneAsync

            row.Deleted <- true
            row.OnConflict <- Common.OnConflict.Throw
            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())

            if result <> NoError then
                db.ClearUpdates() |> ignore

            return result
        }

    member _.UpdateConfigFileOptions (configFile: Guid) (updates: ConfigOptionUpdate list) =
        let processAction (action: ConfigOptionUpdate) : unit =
            match action with
            | ConfigOptionUpdate.Add opts ->
                opts
                |> List.map (fun opt -> db.Poudrierec2.Configoptions.Create())
                |> List.zip opts
                |> List.map (fun (opt, row) ->
                    row.Name <- opt.Name
                    row.Value <- opt.Value
                    row.Configfile <- configFile
                    row.OnConflict <- Common.OnConflict.Throw)
                |> ignore
            | ConfigOptionUpdate.Delete opts ->
                opts
                |> List.map (fun opt ->
                    query {
                        for o in db.Poudrierec2.Configoptions do
                            where (o.Configfile = configFile && o.Name = opt)
                    }
                    |> Seq.iter (fun row -> row.Delete()))
                |> ignore

        async {
            List.iter processAction updates
            let! result = db.SubmitUpdatesAsync() |> DatabaseError.FromQuery

            if result <> NoError then
                db.ClearUpdates() |> ignore

            return result
        }
