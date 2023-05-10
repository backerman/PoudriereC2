namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open FSharp.Data.Sql
open System

type PortsRepository(db: DB.dataContext) =

    member _.GetPortsTrees(?portsTree: string) =
        async {
            let filterQuery =
                match portsTree with
                | None -> <@ fun (_: DB.dataContext.``poudrierec2.portstreesEntity``) -> true @>
                | Some f -> <@ fun (pt: DB.dataContext.``poudrierec2.portstreesEntity``) -> pt.Name = portsTree.Value @>

            let! trees =
                query {
                    for pt in db.Poudrierec2.Portstrees do
                        where ((%filterQuery) pt)
                        sortBy pt.Name
                        select pt
                }
                |> Seq.executeQueryAsync

            return
                trees
                |> Seq.map (fun pt ->
                    { Id = Some pt.Id
                      Name = pt.Name
                      Method =
                        // FIXME Just drop the + and everything after it;
                        // poudriere doesn't actually need it.
                        pt.Method.Split([| '+' |]).[0] |> FromString<PortsTreeMethod>
                      Url = pt.Url })
        }

    member _.AddPortsTree(tree: PortsTree) =
        async {
            let row = db.Poudrierec2.Portstrees.Create()
            row.Id <- Guid.NewGuid()
            row.Name <- tree.Name
            row.Method <- UnionToString tree.Method
            row.Url <- tree.Url
            row.OnConflict <- Common.OnConflict.Throw
            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())

            if result <> NoError then
                db.ClearUpdates() |> ignore

            return (result, row.Id)
        }

    member _.UpdatePortsTree (treeId: Guid) (tree: PortsTree) =
        async {
            let! row =
                query {
                    for pt in db.Poudrierec2.Portstrees do
                        where (pt.Id = treeId)
                        select pt
                }
                |> Seq.exactlyOneAsync

            row.Name <- tree.Name
            row.Method <- tree.Method.ToString()
            row.Url <- tree.Url
            row.OnConflict <- Common.OnConflict.Update
            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())

            if result <> NoError then
                db.ClearUpdates() |> ignore

            return result
        }

    member _.DeletePortsTree(treeId: Guid) =
        async {
            let! row =
                query {
                    for pt in db.Poudrierec2.Portstrees do
                        where (pt.Id = treeId)
                        select pt
                }
                |> Seq.exactlyOneAsync

            row.Delete()
            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())

            if result <> NoError then
                db.ClearUpdates() |> ignore

            return result
        }
