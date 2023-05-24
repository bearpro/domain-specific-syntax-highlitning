module LanguageServer.HierarchicalAnalysis.Tests

open System
open Xunit

open LanguageServer.HierarchicalAnalysis.ConfigurationProcessing
open LanguageServer.HierarchicalAnalysis.ConfigurationGrammar
open LanguageServer.HierarchicalAnalysis.HierarchicalAnalysis

[<Fact>]
let ``Configuration statements sucessfully parsed to a tree`` () =
    let input = [
        { nesting = 0; identifier = "a"; ratio = 0. }
        { nesting = 4; identifier = "a.1"; ratio = 0. }
        { nesting = 4; identifier = "a.2"; ratio = 0. }
        { nesting = 8; identifier = "a.2.1"; ratio = 0. }
        { nesting = 0; identifier = "c"; ratio = 0. }
        { nesting = 4; identifier = "c.1"; ratio = 0. }
    ]
    
    let result = listToTreeHelper input

    let expected = [
        { key = "a"; ratio = 0.; childItems = [
            { key = "a.1"; ratio = 0.; childItems = []}
            { key = "a.2"; ratio = 0.; childItems = [
                { key = "a.2.1"; ratio = 0.; childItems = [] }
            ]}
        ]};
        { key = "c"; ratio = 0.; childItems = [
            { key = "c.1"; ratio = 0.; childItems = []}
        ]}
    ]

    Assert.Equal<ConfigurationItem>(expected, result)

[<Fact>]
let ``Tree evaluation result ordered as expected`` () =
    let input = [
        { key = "a"; ratio = 1.; childItems = [
            { key = "1"; ratio = 0.5; childItems = []}
            { key = "2"; ratio = 0.8; childItems = []}
        ]}
        { key = "b"; ratio = 1.; childItems = [
            { key = "1"; ratio = 1.; childItems = []}
        ]}
        { key = "c"; ratio = 0.; childItems = [
            { key = "1"; ratio = 1.; childItems = []}
        ]}
    ]

    let result = evaluateHierarchy input
    let tokensOrderedByPriority = 
        result
        |> List.sortByDescending (fun (_, ratio) -> ratio)
        |> List.map (fun (token, _) -> token)
    
    let expectedTokensOrder = [
        "b-1"
        "a-2"
        "a-1"
        "c-1"
    ]
    
    Assert.Equal<string>(expectedTokensOrder, tokensOrderedByPriority)


[<Fact>]
let ``TokenPriorityAnalyzer drops zero-weighted modifiers when domain set to zero`` () =
    let configTree = [
        { key = "a"; ratio = 1.; childItems = [
            { key = "1"; ratio = 0.5; childItems = []}
            { key = "2"; ratio = 0.8; childItems = []}
        ]}
        { key = "b"; ratio = 1.; childItems = [
            { key = "1"; ratio = 1.; childItems = []}
        ]}
        { key = "c"; ratio = 0.; childItems = [
            { key = "1"; ratio = 1.; childItems = []}
        ]}
    ]
    let config = { threshold = 300; configurationTree = configTree }
    let tokenAnalyzer = TokenPriorityAnalyzer()
    tokenAnalyzer.SetConfiguration config

    let tokenAnalyzer = tokenAnalyzer :> ITokenPriorityAnalyzer
    
    let modifier = tokenAnalyzer.SelectPreferredModifier [| "c-1" |]
    Assert.Null modifier

[<Theory>]
[<InlineData("c-1,b-1,a-1", "b-1")>]
[<InlineData("c-1", null)>]
[<InlineData("c-1,a-1,a-2", "a-2")>]
let ``TokenPriorityAnalyzer preferes most waigthed modifier`` (givenModifiers: string, expectedModifier: string) =
    let configTree = [
        { key = "a"; ratio = 1.; childItems = [
            { key = "1"; ratio = 0.5; childItems = []}
            { key = "2"; ratio = 0.8; childItems = []}
        ]}
        { key = "b"; ratio = 1.; childItems = [
            { key = "1"; ratio = 1.; childItems = []}
        ]}
        { key = "c"; ratio = 0.; childItems = [
            { key = "1"; ratio = 1.; childItems = []}
        ]}
    ]
    let config = { threshold = 300; configurationTree = configTree }
    let tokenAnalyzer = TokenPriorityAnalyzer()
    tokenAnalyzer.SetConfiguration config

    let tokenAnalyzer = tokenAnalyzer :> ITokenPriorityAnalyzer

    let givenModifiers = givenModifiers.Split(',')
    let modifier = tokenAnalyzer.SelectPreferredModifier givenModifiers
    Assert.Equal (expectedModifier, modifier)

[<Fact>]
let ``TokenPriorityAnalyzer drops unknown modifiers`` () =
    let configTree = []
    let config = { threshold = 300; configurationTree = configTree }
    let tokenAnalyzer = TokenPriorityAnalyzer()
    tokenAnalyzer.SetConfiguration config

    let tokenAnalyzer = tokenAnalyzer :> ITokenPriorityAnalyzer

    let modifier = tokenAnalyzer.SelectPreferredModifier [|"unknown-modifier"|]
    let expectedModifier: string = null
    Assert.Equal (expectedModifier, modifier)
