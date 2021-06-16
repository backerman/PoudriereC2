namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql

type ConfigRepository (db: DB.dataContext) =

    member this.getConfigFileOptions (configFile: string) =
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
