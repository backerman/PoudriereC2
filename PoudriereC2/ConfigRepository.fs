namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql

type ConfigRepository (db: DB.dataContext) =

    member __.getConfigFileOptions (configFile: string) =
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

    member __.getConfigFiles () =
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