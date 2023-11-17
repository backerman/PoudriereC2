namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open System
open Npgsql
open Dapper
open FSharp.Control
open System.Collections.Generic
open System.Data

type PortSetRepository(ds: NpgsqlDataSource) =
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

    /// Perform the requested update in a provided transaction. This call will
    /// not commit or roll back the transaction.
    member _.UpdatePortSetMembersWithTransaction
        (conn: IDbConnection)
        (txn: IDbTransaction)
        (portSet: Guid)
        (action: PortSetUpdate)
        =
        let updateParamsQuery (anAction: PortSetUpdate) =
            let stringListToRecordList (ports: string list) =
                ports |> List.map (fun port -> {| portset = portSet; portname = port |})

            match anAction with
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

        async {
            let (parameters, query) = updateParamsQuery action

            let! result =
                conn.ExecuteAsync(query, parameters, transaction = txn)
                |> DatabaseError.FromQuery

            return result
        }

    /// Process one or more add/delete actions on a port set as part of a
    /// single transaction.
    member this.UpdatePortSetMembers (portSet: Guid) (actions: PortSetUpdate list) =

        async {
            use! conn = ds.OpenConnectionAsync()
            use! txn = conn.BeginTransactionAsync()

            let! result =
                actions
                |> List.map (this.UpdatePortSetMembersWithTransaction conn txn portSet)
                |> Async.Sequential
                |> Async.AsTask
                |> DatabaseError.FromQuery

            match result with
            | NoError -> do! txn.CommitAsync()
            | _ -> do! txn.RollbackAsync()

            return result
        }

    member this.CreatePortSet(ps: PortSet) =
        async {
            let psGuid = Guid.NewGuid()

            let query =
                $"""
                INSERT INTO poudrierec2.portsets (id, name, portable_name)
                VALUES (@id, @name, @portableName)
                ON CONFLICT DO NOTHING
                """

            use! conn = ds.OpenConnectionAsync()
            use! txn = conn.BeginTransactionAsync()
            // Nah. Get portable name as input in PortsSet object.
            let! result =
                conn.ExecuteAsync(query, { ps with Id = Some psGuid }, transaction = txn)
                |> DatabaseError.FromQuery

            match result with
            | NoError ->
                // Add the initial members
                let! result = Add ps.Origins |> this.UpdatePortSetMembersWithTransaction conn txn psGuid

                match result with
                | NoError -> do! txn.CommitAsync()
                | _ -> do! txn.RollbackAsync()
            | _ -> do! txn.RollbackAsync()

            return (result, psGuid)
        }

    member _.DeletePortSet(portSet: Guid) =
        async {
            // portset_members is ON DELETE CASCADE, so we don't need to
            // explicitly delete the members.
            let query =
                """
                DELETE FROM poudrierec2.portsets
                WHERE id = @id
                """

            use! conn = ds.OpenConnectionAsync()
            let! result = conn.ExecuteAsync(query, {| Id = portSet |}) |> DatabaseError.FromQuery
            return result
        }
