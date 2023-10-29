namespace Facefault.PoudriereC2

open Facefault.PoudriereC2
open Facefault.PoudriereC2.Database
open System
open FSharp.Data.Sql
open Npgsql
open Dapper
open System.Data

type ConfigRepository(ds: NpgsqlDataSource) =

    member _.GetConfigFileOptions(configFile: Guid) =
        async {
            use! conn = ds.OpenConnectionAsync()

            let querySql =
                """
                SELECT    cfo.name, cfo.value
                FROM      poudrierec2.configoptions cfo
                WHERE     cfo.configfile = @configFile
                ORDER BY  cfo.name
                """

            let! opts = conn.QueryAsync<ConfigOption>(querySql, dict [ "configFile" => configFile ])
            return opts
        }

    member _.GetConfigFiles(?configFile: Guid) =
        async {
            use! conn = ds.OpenConnectionAsync()

            let querySql =
                """
                SELECT    cf.id, cf.deleted, cf.name,
                          ps.id PortSet, ps.name PortSetName, pt.id PortsTree, pt.name PortsTreeName,
                          j.id Jail, j.name JailName, cf.configtype FileType
                FROM      poudrierec2.configfiles cf
                LEFT JOIN poudrierec2.portsets ps ON cf.portset = ps.id
                LEFT JOIN poudrierec2.portstrees pt ON cf.portstree = pt.id
                LEFT JOIN poudrierec2.jails j ON cf.jail = j.id
                WHERE     (%%filter%%)
                ORDER BY  cf.id
                """

            let filterSql =
                match configFile with
                | None -> "1 = 1"
                | Some f -> "cf.id = @configFile"

            let sqlParams =
                match configFile with
                | None -> []
                | Some f -> [ "configFile" => f ]
                |> dict

            let! result =
                let executeSql = querySql.Replace("%%filter%%", filterSql)
                conn.QueryAsync<ConfigFileMetadata>(executeSql, sqlParams)

            return result
        }

    member _.NewConfigFile(metadata: ConfigFileMetadata) =
        async {
            use! conn = ds.OpenConnectionAsync()
            let guid = Guid.NewGuid()

            let sql =
                """
                INSERT INTO poudrierec2.configfiles
                (id, deleted, name, portset, portstree, jail, configtype)
                VALUES
                (@id, @deleted, @name, @portset, @portstree, @jail, @filetype)
                """

            let! result =
                conn.ExecuteAsync(sql, { metadata with Id = Some guid })
                |> DatabaseError.FromQuery

            return (result, guid)
        }

    member _.UpdateConfigFile(metadata: ConfigFileMetadata) : Async<DatabaseError> =
        async {
            use! conn = ds.OpenConnectionAsync()

            let rowGuid =
                match metadata.Id with
                | Some id -> id
                | None -> raise (ArgumentException("Config file metadata must have an ID"))

            let sql =
                """
                UPDATE poudrierec2.configfiles
                SET    deleted = @deleted, name = @name, portset = @portset,
                       portstree = @portstree, jail = @jail, configtype = @filetype
                WHERE id = @id
                """

            let! result =
                conn.ExecuteAsync(sql, { metadata with Id = Some rowGuid })
                |> DatabaseError.FromQuery

            return result
        }

    member _.DeleteConfigFile(configFile: Guid) : Async<DatabaseError> =
        async {
            use! conn = ds.OpenConnectionAsync()

            let sql =
                """
                UPDATE poudrierec2.configfiles
                SET    deleted = true
                WHERE  id = @id
                """

            let! result = conn.ExecuteAsync(sql, dict [ "id" => configFile ]) |> DatabaseError.FromQuery

            return result
        }

    member _.UpdateConfigFileOptions (configFile: Guid) (updates: ConfigOptionUpdate list) =
        let processAction (conn: NpgsqlConnection) (txn: NpgsqlTransaction) (action: ConfigOptionUpdate) =
            let (sql, ps) =
                match action with
                | ConfigOptionUpdate.Add opts ->
                    let sqlInsert =
                        """
                        INSERT INTO poudrierec2.configoptions
                        (name, value, configfile)
                        VALUES
                        """

                    let sqlValues (j: int) : string =
                        $"""
                        (@name{j}, @value{j}, @configfile)
                        """

                    let sqlPredicates = Collections.Generic.List<string>()
                    let p = DynamicParameters()

                    opts
                    |> List.iteri (fun j opt ->
                        p.Add($"name{j}", opt.Name, DbType.String)
                        p.Add($"value{j}", opt.Value)
                        sqlPredicates.Add(sqlValues j))

                    p.Add("configfile", configFile, DbType.Guid)
                    let sqlQuery = sqlInsert + String.Join(", ", sqlPredicates)
                    (sqlQuery, p)
                | ConfigOptionUpdate.Delete opts ->
                    let p = DynamicParameters()

                    let namesPlaceholders =
                        opts
                        |> List.mapi (fun i opt ->
                            p.Add($"name{i}", opt, DbType.String)
                            $"@name{i}")
                        |> List.toArray

                    let namesPlaceholders2 = String.Join(", ", namesPlaceholders)

                    let sqlDelete =
                        $"""
                        DELETE
                        FROM     poudrierec2.configoptions WHERE configfile = @configFile
                        AND      name IN ({namesPlaceholders2})
                        """

                    p.Add("configFile", configFile, DbType.Guid)
                    p.Add("names", opts |> Array.ofList)
                    (sqlDelete, p)

            async {
                let! result = conn.ExecuteAsync(sql, ps, transaction = txn)
                return result
            }

        async {
            use! conn = ds.OpenConnectionAsync()
            use! txn = conn.BeginTransactionAsync()

            let! result =
                updates
                |> List.map (fun action -> processAction conn txn action)
                |> Async.Sequential
                |> Async.AsTask
                |> DatabaseError.FromQuery

            if result = NoError then
                do! txn.CommitAsync()
            else
                do! txn.RollbackAsync()

            return result
        }
