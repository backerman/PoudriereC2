namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data
open Facefault.PoudriereC2.Database
open FSharp.Data.Sql

type PortsRepository(db: DB.dataContext) =

    member _.getPortsTrees() =
        async {
            let! trees =
                query {
                    for pt in db.Poudrierec2.Porttrees do
                    select pt
                } |> Seq.executeQueryAsync
        return trees
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
                        row.Method <- UnionToString t
                        row.Url <- Some uri
                    row.OnConflict <- Common.OnConflict.Update)
            let! result = DatabaseError.FromQuery (db.SubmitUpdatesAsync())
            if result <> NoError then
                db.ClearUpdates() |> ignore
            return result
        }