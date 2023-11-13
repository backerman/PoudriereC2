namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open Npgsql
open Dapper
open FSharp.Control
open System.Collections.Generic
open System.Data

type PortSetRepository(db: DB.dataContext, ds: NpgsqlDataSource) =
    /// Return a list of all port sets if portSet is None, or a list
    /// containing only the port set with the given ID if portSet is Some.
    /// The list is sorted by ID, and the returned values do not have their
    /// Origins members populated.
    member _.GetPortSets(portSet: Guid option) =
        async {
            let p = DynamicParameters()

            let filterQuery =
                match portSet with
                | None -> "1 = 1"
                | Some id ->
                    p.Add("id", id)
                    "id = @id"

            let query =
                $"""
                SELECT * FROM poudrierec2.portsets
                WHERE {filterQuery}
                ORDER BY id
                """

            use! conn = ds.OpenConnectionAsync()
            let! portSets = conn.QueryAsync<PortSet>(query, p)
            return List.ofSeq portSets
        }

    member _.GetPortSetMembers(portSets: Guid list) =
        async {
            let! conn = ds.OpenConnectionAsync()
            let (placeholders, parameterBag) = makePlaceholders portSets "portSet" DbType.Guid

            let query =
                $"""
                SELECT portset, portname FROM poudrierec2.portset_members
                WHERE portset IN ({placeholders})
                ORDER BY portset, portname
                """

            use! reader = conn.ExecuteReaderAsync(query, parameterBag)
            let portSetOrdinal = reader.GetOrdinal("portset")
            let portNameOrdinal = reader.GetOrdinal("portname")

            let rec loop () =
                asyncSeq {
                    let! hasNext = reader.ReadAsync().AsAsync()

                    match hasNext with
                    | false -> ()
                    | true ->
                        yield (reader.GetGuid(portSetOrdinal), reader.GetString(portNameOrdinal))
                        yield! loop ()
                }

            let ports = Dictionary<Guid, string list>()

            do!
                loop ()
                |> AsyncSeq.iter (fun (portSet, portName) ->
                    match ports.ContainsKey portSet with
                    | false -> ports.Add(portSet, [ portName ])
                    | true -> ports.[portSet] <- portName :: ports.[portSet])
            // The above reversed the order of ports, so they need to be reversed.
            let portsCorrectOrder =
                // no List.reverse? Ouch. Fix later.
                ports |> Seq.map (fun (kvp) -> (kvp.Key, kvp.Value |> List.sort)) |> Map.ofSeq

            return portsCorrectOrder
        }

    member this.GetPortSetMembers(portSet: Guid) =
        async {
            let! mapResult = List.singleton portSet |> this.GetPortSetMembers
            return mapResult.[portSet]
        }

    /// Process one or more add/delete actions on a port set as part of a
    /// single transaction.
    member _.UpdatePortSetMembers (portSet: Guid) (actions: PortSetUpdate list) =
        let updateParamsQuery (action: PortSetUpdate) =
            let stringListToRecordList (ports: string list) =
                ports |> List.map (fun port -> {| portset = portSet; portname = port |})

            match action with
            | Add ports ->
                let query =
                    """
                    INSERT INTO poudrierec2.portset_members (portset, portname)
                    VALUES (@portset, @portname)
                    ON CONFLICT DO NOTHING
                    """

                let parameters = stringListToRecordList ports
                (parameters, query)
            | Delete ports ->
                let query =
                    """
                DELETE FROM poudrierec2.portset_members
                WHERE portset = @portset AND portname = @portname
                """

                let parameters = stringListToRecordList ports
                (parameters, query)

        let processAction (conn: NpgsqlConnection) (txn: NpgsqlTransaction) (action: PortSetUpdate) =
            async {
                let (parameters, query) = updateParamsQuery action
                let! result = conn.ExecuteAsync(query, parameters, transaction = txn)
                return result
            }

        async {
            use! conn = ds.OpenConnectionAsync()
            use! txn = conn.BeginTransactionAsync()

            let! result =
                actions
                |> List.map (processAction conn txn)
                |> Async.Sequential
                |> Async.AsTask
                |> DatabaseError.FromQuery
            match result with
            | NoError -> do! txn.CommitAsync()
            | _ -> do! txn.RollbackAsync()
            return result
        }

    member _.CreatePortSet(name: string) =
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
                db.ClearUpdates() |> ignore

            return (result, portSet.Id)
        }

    member _.DeletePortSet(portSet: Guid) =
        async {
            // portset_members is ON DELETE CASCADE, so we don't need to
            // explicitly delete the members.
            let! portSet =
                query {
                    for ps in db.Poudrierec2.Portsets do
                        where (ps.Id = portSet)
                }
                |> Seq.executeQueryAsync

            portSet |> Seq.iter (fun row -> row.Delete())

            let! result = DatabaseError.FromQuery(db.SubmitUpdatesAsync())
            return result
        }
