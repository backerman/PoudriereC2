namespace Facefault.PoudriereC2

open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker.Middleware
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Logging
open System.Threading.Tasks

type AADMiddleware() =
    let upnClaimType = System.Security.Claims.ClaimTypes.Upn
    let userGuidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier"

    interface IFunctionsWorkerMiddleware with
        member _.Invoke(context: FunctionContext, next: FunctionExecutionDelegate) : Task =
            task {
                // Pre-invocation
                let! requestData = context.GetHttpRequestDataAsync()
                let log = context.GetLogger("AADMiddleware")
                let getClaimByType claimType =
                    let claimIdentity =
                        requestData.Identities
                        |> Seq.tryFind (fun identity ->
                            identity.Claims |> Seq.exists (fun claim -> claim.Type = claimType))
                    match claimIdentity with
                    | None ->
                        failwithf "Claim of type %s not found" claimType
                    | Some claimId ->
                            claimId.Claims |> Seq.find (fun claim -> claim.Type = claimType)
                // Identify the current assembly.
                let thisAssembly = System.Reflection.Assembly.GetExecutingAssembly()
                log.LogDebug("Entry point: {EntryPoint}", context.FunctionDefinition.EntryPoint)
                let entryPointParts = context.FunctionDefinition.EntryPoint.Split(".")
                let workerTypeName = String.concat "." entryPointParts[..^1]
                let workerMethodName = entryPointParts.[^0]
                let functionType = thisAssembly.GetType(workerTypeName)
                let functionMethod = functionType.GetMethod(workerMethodName)

                let authAttributes =
                    System.Attribute.GetCustomAttributes(functionMethod)
                    |> Array.filter (fun x -> x.GetType() = typeof<AuthorizeAttribute>)
                    |> Array.map (fun x -> x :?> AuthorizeAttribute)

                let policy =
                    match authAttributes.Length with
                    // If no authorization attributes are present, default to Admin.
                    | 0 -> AuthorizationPolicy.Administrator
                    | 1 -> authAttributes[0].Policy
                    | _ -> failwith "Multiple authorization attributes on function"

                log.LogDebug("Attempting to validate authorization policy: {Policy}", policy)

                let policySatisfied =
                    AuthorizationClaim.PolicyIsSatisfiedBy policy requestData.Identities

                match policySatisfied with
                | true -> do! next.Invoke(context)
                | false ->
                    let upn =
                        try
                            (getClaimByType upnClaimType).Value
                        with
                        | _ ->
                            "Unknown UPN"
                    let userGuid =
                        try
                            (getClaimByType userGuidClaimType).Value
                        with
                        | _ -> "Unknown GUID"

                    log.LogError(
                        "User {User} ({UserGuid}) does not possess required claim {Claim}",
                        upn,
                        userGuid,
                        AuthorizationClaim.RequiredClaim policy
                    )

                    let resp = requestData.CreateResponse()
                    resp.StatusCode <- System.Net.HttpStatusCode.Unauthorized
                    resp.writeJsonResponse (Error "Unauthorized") |> ignore
                    context.GetInvocationResult().Value <- resp
                // Post-invocation
                ()
            }
