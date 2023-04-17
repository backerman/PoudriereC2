namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open System.Linq

type ConfigRepository (db: DB.dataContext) =

    member _.GetConfigFileOptions (configFile: string) =
        async {
            let! opts =
                query {
                    for configOption in db.Poudrierec2.Configoptions do
                    where (configOption.Configfile = Guid configFile)
                    sortBy configOption.Name
                    select { Name = configOption.Name
                             Value = configOption.Value }
                } |> Seq.executeQueryAsync
            return opts
        }

    member _.GetConfigFiles (?configFile: string) =
        async {
            let filterQuery =
                match configFile with
                | None -> <@ fun (_: DB.dataContext.``poudrierec2.configfilesEntity``) -> true @>
                | Some f -> <@ fun (file: DB.dataContext.``poudrierec2.configfilesEntity``) -> file.Id = Guid f @>
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
                } |> Seq.executeQueryAsync
            return configFiles
        }

    member _.NewConfigFile (metadata: ConfigFileMetadata) =
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
            let! result = DatabaseError.FromQuery (db.SubmitUpdatesAsync())
            if result <> NoError then
                db.ClearUpdates() |> ignore
            return (result, row.Id)
        }

    member _.UpdateConfigFile (metadata: ConfigFileMetadata) : Async<DatabaseError> =
        async {
            let rowGuid =
                match metadata.Id with
                | Some id -> id
                | None ->
                    raise (ArgumentException("Config file metadata must have an ID"))
            let! row =
                query {
                    for file in db.Poudrierec2.Configfiles do
                    where (file.Id = rowGuid)
                    select file
                } |> Seq.exactlyOneAsync
            row.Name <- metadata.Name
            row.Deleted <- metadata.Deleted
            row.Jail <- metadata.Jail
            row.Portset <- metadata.PortSet
            row.Portstree <- metadata.PortsTree
            row.Configtype <- UnionToString metadata.FileType

            row.OnConflict <- Common.OnConflict.Throw
            let! result = DatabaseError.FromQuery (db.SubmitUpdatesAsync())
            if result <> NoError then
                db.ClearUpdates() |> ignore
            return result
        }

    member _.DeleteConfigFile (configFile: Guid) : Async<DatabaseError> =
        async {
            let! row =
                query {
                    for file in db.Poudrierec2.Configfiles do
                    where (file.Id = configFile)
                    select file
                } |> Seq.exactlyOneAsync
            row.Deleted <- true
            row.OnConflict <- Common.OnConflict.Throw
            let! result = DatabaseError.FromQuery (db.SubmitUpdatesAsync())
            if result <> NoError then
                db.ClearUpdates() |> ignore
            return result
        }

    member _.AddConfigFileOptions (configFile: string) (options: ConfigOption list)
            : Async<DatabaseError> =
        async {
            options
            |> List.iter
                (fun o -> 
                    let row = db.Poudrierec2.Configoptions.Create()
                    row.Configfile <- Guid configFile
                    row.Name <- o.Name
                    row.Value <- o.Value
                    row.OnConflict <- Common.OnConflict.Update)
            let! result = DatabaseError.FromQuery (db.SubmitUpdatesAsync())
            if result <> NoError then
                db.ClearUpdates() |> ignore
            return result
        }

    member _.DeleteConfigFileOptions (configFile: string) (options: string list)
            : Async<DatabaseError> =
        async {
            let q =
                query {
                    for o in db.Poudrierec2.Configoptions do
                    where (o.Configfile = Guid configFile && options.Contains o.Name)
                } |> Seq.``delete all items from single table``
            let! result = DatabaseError.FromQuery q
            if result <> NoError then
                db.ClearUpdates() |> ignore
            return result
        }