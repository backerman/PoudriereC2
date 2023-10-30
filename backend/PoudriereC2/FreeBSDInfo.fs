namespace Facefault.PoudriereC2

open FSharp.Data
open System
open System.Runtime.Caching
open System.Text.RegularExpressions
open Microsoft.Extensions.Logging

[<AllowNullLiteral>]
type CacheEntry(entries: string list) =
    new() = CacheEntry([])
    member val entries: string list = entries with get, set

type FreeBSDInfo() =
    static let archRegex = Regex("^([a-z][a-z0-9]+)/$", RegexOptions.Compiled)

    static let releaseRegex =
        Regex(
            "^([0-9.][0-9A-Za-z.]+-(RELEASE|STABLE|CURRENT|SNAPSHOT|PRERELEASE|RC\d+))/$",
            RegexOptions.Compiled
        )

    static let downloadPrefix =
        match Environment.GetEnvironmentVariable "FREEBSD_MIRROR_BASE" with
        | null
        | "" -> "https://download.freebsd.org/"
        | x -> x

    static let cache = MemoryCache.Default

    /// Get a downloads.freebsd.org directory listing and parse entries.
    member private _.getAndParse (regex: Regex) (url: string) =
        async {
            let! maybeDoc = HtmlDocument.AsyncLoad(url) |> Async.Catch

            let entries =
                match maybeDoc with
                | Choice2Of2 _ -> []
                | Choice1Of2 doc ->
                    "td.link > a[href]"
                    |> doc.CssSelect
                    |> List.map (fun x ->
                        let hrefValue = (x.Attribute "href").Value()

                        match regex.Match(hrefValue) with
                        | m when m.Success -> Some m.Groups[1].Value
                        | _ -> None)
                    |> List.filter (fun x -> x.IsSome)
                    |> List.map (fun x -> x.Value)

            return entries
        }

    member private this.getArchitectureListing (log: ILogger) (url: string) =
        async {
            let! architectures = this.getAndParse archRegex url
            return architectures
        }

    member private this.getReleaseListing(url: string) =
        async {
            let! releases = this.getAndParse releaseRegex url
            cache.Set(url, CacheEntry(releases), DateTimeOffset.Now.AddMinutes(5.0))
            return releases
        }

    member private this.getArchitecturesOneLevel (log: ILogger) (levels: string list) =
        async {
            let! architectures =
                [ "snapshots/"; "releases/" ]
                |> List.map (
                    (fun releaseType ->
                        let builder = UriBuilder(downloadPrefix)
                        builder.Path <- releaseType
                        List.iter (fun x -> builder.Path <- builder.Path + x + "/") levels
                        builder.ToString())
                    >> this.getArchitectureListing log
                )
                |> Async.Parallel

            // If we're on TARGET_ARCH, prefix it with the TARGET and a period.
            // e.g., arm.armv6
            let archPrefix = levels |> List.fold (fun acc x -> acc + x + ".") ""

            let returnedValue =
                architectures
                |> List.concat
                |> List.sort
                |> List.distinct
                |> List.map (fun x -> archPrefix + x)

            return returnedValue
        }

    member this.getFreeBSDArchitectures(log: ILogger) =
        async {
            let cacheEntry = cache.[downloadPrefix] :?> CacheEntry

            if not (isNull cacheEntry) then
                log.LogDebug("Using cached architectures")
                return cacheEntry.entries
            else
                log.LogDebug("Fetching architectures")
                let! primaryArchitectures = this.getArchitecturesOneLevel log []

                let! variantsArray =
                    primaryArchitectures
                    |> List.map (fun x -> this.getArchitecturesOneLevel log [ x ])
                    |> Async.Sequential

                let variants = variantsArray |> List.concat |> List.sort |> List.distinct

                cache.Set(downloadPrefix, CacheEntry(variants), DateTimeOffset.Now.AddMinutes(5.0))
                return variants
        }

    member this.getFreeBSDReleases (log: ILogger) (arch: string) =
        async {
            let archPath = arch.Replace(".", "/")

            let! releases =
                [ "snapshots"; "releases" ]
                |> List.map ((fun x -> downloadPrefix + x + "/" + archPath + "/") >> this.getReleaseListing)
                |> Async.Parallel

            return releases |> List.concat |> List.sort |> List.distinct
        }
