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
                    sortBy file.Id
                    select
                        { Id = file.Id
                          Deleted = file.Deleted
                          Name = file.Name
                          PortSet = file.Portset
                          PortsTree = file.Portstree
                          Jail = file.Jail
                          FileType = FromString<ConfigFileType> file.Configtype }
                } |> Seq.executeQueryAsync
            return configFiles
        }
    
    member _.NewConfigFile (metadata: ConfigFileMetadata) : Async<DatabaseError> =
        async {
            let row = db.Poudrierec2.Configfiles.Create()
            row.Name <- metadata.Name
            row.Deleted <- false
            row.Id <- metadata.Id
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