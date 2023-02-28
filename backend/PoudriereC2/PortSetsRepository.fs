namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open System.Linq

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
                        where ((%filterQuery) portsetEntity)
                        sortBy portsetEntity.Id

                        select
                            { Id = Some portsetEntity.Id
                              Name = portsetEntity.Name
                              Origins = [] }
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
                |> List.executeQueryAsync

            return ports
        }

    /// Process one or more add/delete actions on a port set as part of a
    /// single transaction. The order of operations within the transaction
    /// is not guaranteed.
    member _.UpdatePortSetMembers (portSet: Guid) (actions: PortSetUpdate list) =
        let processAction (action: PortSetUpdate) =
            match action with
            | Add ports ->
                ports
                |> List.map (fun port -> db.Poudrierec2.PortsetMembers.Create())
                |> List.zip ports
                |> List.map (fun (port, row) ->
                    row.Portname <- port
                    row.Portset <- portSet
                    row.OnConflict <- Common.OnConflict.DoNothing
                    row)
                |> ignore
            | Delete ports ->
                query {
                    for psm in db.Poudrierec2.PortsetMembers do
                        where (psm.Portset = portSet && ports.Contains(psm.Portname))
                }
                |> Seq.iter (fun row -> row.Delete())

        async {
            actions |> List.iter processAction
            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())
            return result
        }

    member _.CreatePortSet (name: string) =
        async {
            // The database code needs to be switched to something that alllows
            // explicit transaction control. As a hack, the portset is created
            // first, then the members are added with UpdatePortSetMembers;
            // otherwise, order of operations is not guaranteed by SQLProvider.
            let psGuid = Guid.NewGuid()
            let portSet = db.Poudrierec2.Portsets.Create()
            portSet.Id <- psGuid
            portSet.Name <- name
            portSet.OnConflict <- Common.OnConflict.Throw
            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())
            if result <> NoError then
                db.ClearUpdates()
                |> ignore
            return result, portSet.Id
        }

    member _.DeletePortSet (portSet: Guid) =
        async {
            // portset_members is ON DELETE CASCADE, so we don't need to
            // explicitly delete the members.
            let! portSet =
                query {
                    for ps in db.Poudrierec2.Portsets do
                    where (ps.Id = portSet)
                } |> Seq.executeQueryAsync
            portSet
            |> Seq.iter(fun row -> row.Delete())

            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())
            return result
        }