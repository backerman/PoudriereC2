module PoudriereC2.Tests.Serialization

open NUnit.Framework
open FsUnit
open AutoFixture
open AutoFixture.AutoFoq
open System
open Facefault.PoudriereC2.Serialization
open Microsoft.Azure.Functions.Worker.Http
open System.IO
open AutoFixture.Kernel

[<TestFixture>]
type SerializationTests() =

    [<Test>]
    member __.TestShellQuoting() =
        let cases =
            [{|Input = "foobarlfajfsidfsad"; Output = "foobarlfajfsidfsad"|};
             {|Input = "foo bar baz"; Output = "\"foo bar baz\""|};
             {|Input = "$fred"; Output = "\"\\$fred\""|};
             {|Input = "steins;gate"; Output = "\"steins;gate\""|};
             {|Input = "#1"; Output = "\"#1\""|}]
        cases
        |> Seq.iter
            (fun testCase ->
                testCase.Input.ShellQuote()
                |> should equal testCase.Output)