module PoudriereC2.Tests.TypeConversion

open NUnit.Framework
open FsUnit
open Facefault.PoudriereC2
open System
open System.Text.Json
open Facefault.PoudriereC2.Data

[<TestFixture>]
type TypeConversionTests() =
    [<Test>]
    member __.TestUnionFromString () =
        let configFileType = "poudriereconf"
        let expected = ConfigFileType.PoudriereConf
        let actual = FromString<ConfigFileType> configFileType
        Assert.That(actual, Is.EqualTo(expected))

    member __.TestUnionToString () =
        let configFileType = PoudriereConf
        let expected = "poudirereconf"
        let actual = UnionToString configFileType
        Assert.That(actual, Is.EqualTo(expected))