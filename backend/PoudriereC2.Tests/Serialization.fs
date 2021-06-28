module PoudriereC2.Tests.Serialization

open NUnit.Framework
open FsUnit
open Facefault.PoudriereC2.Serialization

[<TestFixture>]
type SerializationTests() =

    [<Test>]
    member _.TestShellQuoting() =
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