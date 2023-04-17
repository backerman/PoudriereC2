namespace Facefault.PoudriereC2

open Dapper
open Npgsql
open System
open Facefault.PoudriereC2.Database

type JailRepository (conn: NpgsqlConnection) =
    member _.GetJails () =
        async {
            let query =
                """SELECT * FROM poudrierec2.jails
                   ORDER BY name"""
            let! jails =
                query
                |> conn.QueryAsync<Jail>
            return jails
        }

    member _.CreateJail (j: Jail)=
        async {
            let query =
                """INSERT INTO poudrierec2.jails (id, name, version, architecture, method, url)
                   VALUES (@id, @name, @version, @architecture, @method, @url)"""
            let newGuid = Guid.NewGuid()
            let! result =
                conn.ExecuteAsync(query, {j with Id = Some newGuid})
                |> DatabaseError.FromQuery
            return (result, newGuid)
        }

    member _.DeleteJail (jailId: Guid) =
        async {
            let query =
                "DELETE FROM poudrierec2.jails WHERE id = @id"
            let queryParams = [("id", jailId)] |> dict
            let! result =
                conn.ExecuteAsync(query, queryParams)
                |> DatabaseError.FromQuery
            return result
        }

    member _.UpdateJail (jailId: Guid) (j: Jail) =
        async {
            let query =
                """UPDATE poudrierec2.jails
                   SET name = @name, version = @version, architecture = @architecture, method = @method, url = @url
                   WHERE id = @id"""
            let! result =
                conn.ExecuteAsync(query, {j with Id = Some jailId})
                |> DatabaseError.FromQuery
            return result
        }