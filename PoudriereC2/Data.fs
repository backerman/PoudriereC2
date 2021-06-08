module Facefault.PoudriereC2.Data

open Microsoft.FSharp.Reflection

let FromString<'T> (s: string) =
    let lowerInput = s.ToLowerInvariant()
    let myCase =
        FSharpType.GetUnionCases typeof<'T>
        |> Array.find (fun c -> c.Name.ToLowerInvariant() = lowerInput)
    FSharpValue.MakeUnion(myCase, [||]) :?> 'T

let UnionToString (v: 'T) : string =
    match FSharpValue.GetUnionFields(v, typeof<'T>) with
        | case, _ -> case.Name.ToLowerInvariant()