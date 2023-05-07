namespace Facefault.PoudriereC2

type AuthorizationPolicy =
    | Administrator = 'A'
    | Viewer = 'V'
    | Machine = 'M'

type AuthorizationClaim =
    static member RequiredClaim(policy: AuthorizationPolicy) =
        match policy with
        | AuthorizationPolicy.Administrator -> "PoudriereC2.Administrator"
        | AuthorizationPolicy.Viewer -> "PoudriereC2.Viewer"
        | AuthorizationPolicy.Machine -> "PoudriereC2.Machine"
        | _ -> failwith "Invalid authorization policy."

    static member PolicyIsSatisfiedBy (policy: AuthorizationPolicy) (identities: System.Security.Claims.ClaimsIdentity seq) =
        let existsClaim (predicate: (System.Security.Claims.Claim -> bool)) =
            identities
            |> Seq.exists (fun identity -> identity.Claims |> Seq.exists predicate)

        match policy with
        | AuthorizationPolicy.Viewer ->
            existsClaim (fun claim ->
                claim.Type = "roles"
                && (claim.Value = AuthorizationClaim.RequiredClaim AuthorizationPolicy.Viewer
                    || claim.Value = AuthorizationClaim.RequiredClaim AuthorizationPolicy.Administrator))
        | _ -> existsClaim (fun claim -> claim.Type = "roles" && claim.Value = AuthorizationClaim.RequiredClaim policy)

/// Configure an authorization policy for the decorated function.
[<System.AttributeUsage(System.AttributeTargets.Method)>]
type AuthorizeAttribute(policy: AuthorizationPolicy) =
    inherit System.Attribute()
    member val Policy = policy with get
