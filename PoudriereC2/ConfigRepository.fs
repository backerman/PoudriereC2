namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open System.Linq

type ConfigRepository (db: DB.dataContext) =

    member _.getConfigFileOptions (configFile: string) =
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

    member _.getConfigFiles () =
        async {
            let! configFiles =
                query {
                    for file in db.Poudrierec2.Configfiles do
                    select
                     { Id = file.Id
                       Deleted = file.Deleted
                       Name = file.Name
                       Portset = file.Portset
                       Porttree = file.Porttree
                       Jail = file.Jail
                       FileType = FromString<ConfigFileType> file.Configtype }
                } |> Seq.executeQueryAsync
            return configFiles
        }

    member _.addConfigFileOptions (configFile: string) (options: ConfigOption list)
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
            return! DatabaseError.FromQuery (db.SubmitUpdatesAsync())
        }

    member _.deleteConfigFileOptions (configFile: string) (options: string list)
            : Async<DatabaseError> =
        async {
            let q =
                query {
                    for o in db.Poudrierec2.Configoptions do
                    where (o.Configfile = Guid configFile && options.Contains o.Name)
                } |> Seq.``delete all items from single table``
            return! (DatabaseError.FromQuery q)
        }