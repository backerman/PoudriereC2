namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql

type PortSetsRepository(db: DB.dataContext) =
    member _.GetPortSets(portSet: Guid option) =
        async {
            let filterQuery =
                match portSet with
                | None -> <@ fun (_: DB.dataContext.``poudrierec2.portsetsEntity``) -> true @>
                | Some f -> <@ fun (portset: DB.dataContext.``poudrierec2.portsetsEntity``) -> portset.Id = f @>

            let! portSets =
                query {
                    for portsetEntity in db.Poudrierec2.Portsets do
                    join psm in (!!) db.Poudrierec2.PortsetMembers on (portsetEntity.Id = psm.Portset)
                    where ((%filterQuery) portsetEntity)
                    sortBy portsetEntity.Id
                    select
                        { Id = portsetEntity.Id
                          Name = portsetEntity.Name }
                }
                |> Seq.executeQueryAsync

            return portSets
        }

    member _.GetPortSetMembers(portSet: Guid) =
        async {
            let! ports =
                query {
                    for port in db.Poudrierec2.PortsetMembers do
                    where (port.Portset = portSet)
                    sortBy port.Portname
                    select port.Portname
                }
                |> Seq.executeQueryAsync

            return ports
        }