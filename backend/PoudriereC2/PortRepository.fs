namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open Npgsql
open System
open Dapper

type PortRepository(ds: NpgsqlDataSource) =

    member _.GetPortsTrees(?portsTree: Guid) =
        async {
            let p = DynamicParameters()

            let filterQuery =
                match portsTree with
                | None -> "1 = 1"
                | Some id ->
                    p.Add("id", id)
                    "id = @id"

            let query =
                $"""
                SELECT * FROM poudrierec2.portstrees
                WHERE {filterQuery}
                ORDER BY id
                """

            use! conn = ds.OpenConnectionAsync()
            let! trees = conn.QueryAsync<PortsTree>(query, p)
            return trees
        }

    member _.AddPortsTree(tree: PortsTree) =
        async {
            use! conn = ds.OpenConnectionAsync()
            let guid = Guid.NewGuid()

            let sql =
                """
                INSERT INTO poudrierec2.portstrees
                (id, name, portable_name, method, url)
                VALUES
                (@id, @name, @portablename, @method, @url)
                """

            let! result = conn.ExecuteAsync(sql, { tree with Id = Some guid }) |> DatabaseError.FromQuery

            return (result, guid)
        }

    member _.UpdatePortsTree(tree: PortsTree) =
        async {
            use! conn = ds.OpenConnectionAsync()

            if tree.Id.IsNone then
                raise (ArgumentException("Ports tree metadata must have an ID"))

            let sql =
                """
                UPDATE poudrierec2.portstrees
                SET    name = @name, portable_name = @portablename,
                       method = @method, url = @url
                WHERE id = @id
                """

            let! result = conn.ExecuteAsync(sql, tree) |> DatabaseError.FromQuery

            return result
        }

    member _.DeletePortsTree(treeId: Guid) =
        async {
            use! conn = ds.OpenConnectionAsync()

            let sql =
                """
                DELETE FROM poudrierec2.configfiles
                WHERE       id = @id
                """

            let! result = conn.ExecuteAsync(sql, dict [ "id" => treeId ]) |> DatabaseError.FromQuery

            return result
        }
