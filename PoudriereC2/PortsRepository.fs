namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open FSharp.Data.Sql

type PortsRepository(db: DB.dataContext) =

    member _.getPortsTrees(?portsTree: string) =
        async {
            let filterQuery =
                match portsTree with
                | None ->
                    <@ fun (_: DB.dataContext.``poudrierec2.porttreesEntity``) -> true @>
                | Some f ->
                    <@ fun (pt: DB.dataContext.``poudrierec2.porttreesEntity``) ->
                        pt.Name = portsTree.Value @>
            let! trees =
                query {
                    for pt in db.Poudrierec2.Porttrees do
                    where ((%filterQuery) pt)
                    select pt
                } |> Seq.executeQueryAsync
            return
                trees
                |> Seq.map
                    (fun pt ->
                        { Name = pt.Name
                          Method = PortsTreeMethod.FromString
                            (pt.Method, ?url=pt.Url) })
        }

    member _.addPortsTrees (trees: PortsTree list) =
        async {
            trees
            |> List.iter
                (fun t -> 
                    let row = db.Poudrierec2.Porttrees.Create()
                    row.Name <- t.Name
                    match t.Method with
                    | Null ->
                        row.Method <- t.ToString()
                    | Git uri
                    | Svn uri  ->
                        row.Method <- UnionToString t.Method
                        row.Url <- Some uri
                    row.OnConflict <- Common.OnConflict.Throw)
            let! result = DatabaseError.FromQuery (db.SubmitUpdatesAsync())
            if result <> NoError then
                db.ClearUpdates() |> ignore
            return result
        }