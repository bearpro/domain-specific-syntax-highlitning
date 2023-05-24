module LanguageServer.HierarchicalAnalysis.HierarchicalAnalysis

open ConfigurationProcessing

let evaluateHierarchy criterias = 
    let rec _evaluateHierarchy criterias (parentWeights: float list) =
        [for criteria in criterias -> 
            let parentWeights = criteria.ratio :: parentWeights
            match criteria.childItems with
            | [] ->
                let n = float (List.length parentWeights)
                let total = List.reduce (fun a b -> a * b) parentWeights
                [criteria.key, total ** (1. / n)]

            | childs ->
                let childEstimates = 
                    _evaluateHierarchy childs parentWeights 
                    |> List.reduce (fun a b -> a @ b)
                [ for (path, estimate) in childEstimates -> ($"{criteria.key}-{path}", estimate) ]
        ]

    _evaluateHierarchy criterias [] |> List.reduce (fun a b -> a @ b)