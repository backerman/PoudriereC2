module Facefault.PoudriereC2.Database

open Dapper
open FSharp.Data.LiteralProviders
open Npgsql
open System
open Microsoft.Extensions.Logging
open System.Net
open Azure.Core
open System.Data

[<Literal>]
let ConnectionString = Env.PostgresConnection.Value

/// The schemas containing database objects used by this application.
[<Literal>]
let Owner = "poudrierec2"

[<Literal>]
let ResPath = __SOURCE_DIRECTORY__ + "/obj"

// Dapper stuff
let inline (=>) a b = (a, box b)

type PropertySelectorDelegate = delegate of (Type * string) -> Reflection.PropertyInfo

let setupDatabaseMappers() =
    FSharp.PostgreSQL.OptionTypes.register () |> ignore

    DefaultTypeMap.MatchNamesWithUnderscores <- true
    SqlMapper.AddTypeHandler(JailMethodTypeHandler())
    SqlMapper.AddTypeHandler(JailMethodOptionTypeHandler())
    SqlMapper.AddTypeHandler(PortsTreeMethodTypeHandler())
    SqlMapper.AddTypeHandler(ConfigFileTypeHandler())

type DatabaseError =
    | NoError
    | ForeignKeyViolation of PostgresException
    | UniqueViolation of PostgresException
    | Unknown of Exception

    static member FromException(e: Exception) =
        match e.InnerException with
        | :? AggregateException as ex ->
            // recurse again
            DatabaseError.FromException(ex)
        | :? PostgresException as ex ->
            match ex.SqlState with
            | PostgresErrorCodes.ForeignKeyViolation -> ForeignKeyViolation ex
            | PostgresErrorCodes.UniqueViolation -> UniqueViolation ex
            | _ -> Unknown e
        | _ -> Unknown e

    static member FromQuery(q: Threading.Tasks.Task) =
        async {
            let! opResult = Async.AwaitTask q |> Async.Catch

            return
                match opResult with
                | Choice1Of2 _ -> NoError
                | Choice2Of2 e -> DatabaseError.FromException e
        }

    /// <summary>Handle a DatabaseError, returning a FunctionResult.</summary>
    /// <param name="log">The logger to use.</param>
    /// <param name="failureMessage">The (not user-visible) message to log on failure;
    /// may be a format string in message template format.</param>
    /// <param name="failureMessageArgs">The arguments to the format string, if any.</param>
    /// <returns>A record containing the HTTP status code and a FunctionResult
    /// to send to the user.</returns>
    member dbError.Handle
        (
            log: ILogger,
            failureMessage: string,
            [<ParamArray>] failureMessageArgs: obj[]
        ) : {| httpCode: HttpStatusCode
               result: FunctionResult |}
        =
        match dbError with
        | NoError ->
            {| httpCode = HttpStatusCode.OK
               result = OK |}
        | ForeignKeyViolation exn ->
            let userError =
                match exn.ColumnName with
                | null -> "A referenced row does not exist."
                | x -> $"The value of ${x} refers to a nonexistent row."

            log.LogError(exn, failureMessage, failureMessageArgs)

            {| httpCode = HttpStatusCode.BadRequest
               result = Error userError |}
        | UniqueViolation exn ->
            let userError =
                match exn.ColumnName with
                | null -> "This row is a duplicate."
                | x -> $"There is already a row for this ${x}."

            log.LogError(exn, failureMessage, failureMessageArgs)

            {| httpCode = HttpStatusCode.BadRequest
               result = Error userError |}
        | Unknown exn ->
            // Assume this error contains sensitive data.
            log.LogError(exn, failureMessage, failureMessageArgs)

            {| httpCode = HttpStatusCode.InternalServerError
               result = Error "An internal error occurred." |}

/// Get an access token (MSI or otherwise) from AAD.
let getAccessTokenWithScope (scopeUri: string) =
    let tokenProvider = Azure.Identity.DefaultAzureCredential false
    let ctx = TokenRequestContext [| scopeUri |]
    let token = tokenProvider.GetToken(ctx, System.Threading.CancellationToken.None)
    token.Token

/// Get an access token (MSI or otherwise) from AAD for PostgreSQL.
let getAccessToken () =
    getAccessTokenWithScope "https://ossrdbms-aad.database.windows.net"

/// <summary>Generate a string of SQL placeholders and a corresponding <see cref="DynamicParameters"/> object.</summary>
/// <param name="values">The values to be used by the SQL query.</param>
/// <param name="prefix">The prefix to prepend to the placeholder names.</param>
/// <param name="dbType">The <see cref="System.Data.DbType"/> corresponding to the values' type.</param>
/// <returns>A string of placeholder names to be inserted into the SQL query text and a
/// <see cref="DynamicParameters"/> object containing the corresponding values.</returns>
let makePlaceholders (values: 'a list) (prefix: string) (dbType: DbType) =
    let p = DynamicParameters()

    let placeholderArray =
        values
        |> List.mapi (fun i opt ->
            p.Add($"{prefix}{i}", opt, dbType)
            $"@{prefix}{i}")
        |> List.toArray

    let placeholdersString = String.Join(", ", placeholderArray)
    (placeholdersString, p)
