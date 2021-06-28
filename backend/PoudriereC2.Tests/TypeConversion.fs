module PoudriereC2.Tests.TypeConversion

open NUnit.Framework
open FsUnit
open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data

[<TestFixture>]
type TypeConversionTests() =
    [<Test>]
    member _.TestUnionFromString () =
        let configFileType = "poudriereconf"
        let expected = ConfigFileType.PoudriereConf
        FromString<ConfigFileType> configFileType
        |> should equal expected

    member _.TestUnionToString () =
        let configFileType = PoudriereConf
        let expected = "poudirereconf"
        UnionToString configFileType
        |> should equal expected