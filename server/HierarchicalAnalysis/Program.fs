let (^) a b = a b

module Manifest =
    type СompatibilityTag = 
        Color | Underline | Strikethrough | Italic
    type Attribute = { 
        id: string; compatibilityTag: СompatibilityTag; domain: string }
    type PackageManifest = { 
        id: string; attributes: Attribute list }

open Manifest

type Fragment = Character of int | Range of int * int 
type FragmentHighlited = Fragment -> Attribute -> unit

module HierarchicalAnalysis =
    type HighlitningData = list<Fragment * Attribute * string>
    type Alternative = {
        domain: string
        packageId: string
        attributeId: string
        compatibilityTag: СompatibilityTag 
    }

    type CriteriaLevel = Domain | Package | Attribute

    type Criteria = { criteriaLevel: CriteriaLevel; name: string; wieght: float; childs: list<Criteria> }

    let bg (hd: HighlitningData) tree = 
        let allAlternatives =
            hd |> List.map (fun (_, attr, packageId) -> { 
                attributeId = attr.id
                packageId = packageId
                domain = attr.domain
                compatibilityTag = attr.compatibilityTag 
            }) |> List.distinct

        let criteria :: tail = allAlternatives

        ()

    let buildHierarchy highlitningData =
        let allAlternatives (hd: HighlitningData) =
            hd |> List.map (fun (_, attr, packageId) -> { 
                attributeId = attr.id
                packageId = packageId
                domain = attr.domain
                compatibilityTag = attr.compatibilityTag 
            }) |> List.distinct

        let allAlternatives = allAlternatives highlitningData

        let rec findUniqueEntries (domains, packages, attributes) = function
            | [] -> (domains, packages, attributes)
            | alternative::tail -> 
                findUniqueEntries 
                    ((Set.add alternative.domain domains),
                     (Set.add alternative.packageId packages), 
                     (Set.add alternative.attributeId attributes))
                    tail
                    
        let (domains, packages, attributes) = findUniqueEntries (Set.empty, Set.empty, Set.empty) allAlternatives
        
        let buildLayer entries criteriaLevel =
            entries
            |> Seq.map ^ fun entry -> { 
                criteriaLevel = criteriaLevel
                name = entry
                wieght = 1. / float (Set.count entries)
                childs = []
            }
            |> List.ofSeq

        let domains = buildLayer domains Domain

        let packages = buildLayer packages Package

        let attributes = buildLayer attributes Attribute
        
        [domains; packages; attributes]

    let evaluateHierarchy criterias = 
        let rec _evaluateHierarchy criterias (parentWeights: float list) =
            [for criteria in criterias -> 
                let parentWeights = criteria.wieght :: parentWeights
                match criteria.childs with
                | [] ->
                    let n = float (List.length parentWeights)
                    let total = List.reduce (fun a b -> a * b) parentWeights
                    [criteria.name, total ** (1. / n)]

                | childs ->
                    let childEstimates = 
                        _evaluateHierarchy childs parentWeights 
                        |> List.reduce (fun a b -> a @ b)
                    [ for (path, estimate) in childEstimates -> ($"{criteria.name}.{path}", estimate) ]
            ]

        _evaluateHierarchy criterias [] |> List.reduce (fun a b -> a @ b)
