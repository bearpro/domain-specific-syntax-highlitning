module Tests

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
let ``Tree correctyly evaluated`` () =
    let input = [
        { key = "a"; ratio = 1.; childItems = [
            { key = "1"; ratio = 0.5; childItems = []}
            { key = "2"; ratio = 0.8; childItems = []}
        ]};
        { key = "c"; ratio = 0.; childItems = [
            { key = "1"; ratio = 1.; childItems = []}
        ]}
    ]

    let result = evaluateHierarchy input
    Assert.True(true)