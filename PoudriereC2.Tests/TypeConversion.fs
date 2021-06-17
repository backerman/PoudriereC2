module PoudriereC2.Tests.TypeConversion

open NUnit.Framework
open FsUnit
open Facefault.PoudriereC2
open Facefault.PoudriereC2.Data

[<TestFixture>]
type TypeConversionTests() =
    [<Test>]
    member __.TestUnionFromString () =
        let configFileType = "poudriereconf"
        let expected = ConfigFileType.PoudriereConf
        FromString<ConfigFileType> configFileType
        |> should equal expected

    member __.TestUnionToString () =
        let configFileType = PoudriereConf
        let expected = "poudirereconf"
        UnionToString configFileType
        |> should equal expected