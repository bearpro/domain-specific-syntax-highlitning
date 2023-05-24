module LanguageServer.HierarchicalAnalysis.ConfigurationProcessing

open System.IO
open ConfigurationGrammar
open FParsec
open System.Text

type ConfigurationItem = {
    key: string
    ratio: float
    childItems: ConfigurationItem list
}

type Configuration = {
    threshold: int
    configurationTree: ConfigurationItem list
}

let rec listToTree (lst: Statement list) =
  match lst with
  | [] -> 
    failwith "Empty list"
    { key = ""; ratio = 0.; childItems = [] }, []
  | head :: rest ->
    let index = rest |> List.tryFindIndex (fun x -> x.nesting = head.nesting)
    let branch, remaining =
      match index with
      | None -> rest, []
      | Some i -> rest |> List.splitAt i
    let subtrees = listToTreeAux head.nesting branch
    { key = head.identifier; ratio = head.ratio; childItems = subtrees }, remaining
and listToTreeAux (level: int) (lst: Statement list) : ConfigurationItem list =
  match lst with
  | [] -> []
  | statement :: _ when statement.nesting < level -> []
  | _ ->
    let subtree, remaining = listToTree lst
    let subtrees = listToTreeAux level remaining
    subtree :: subtrees

let rec listToTreeHelper statements = 
    match listToTree statements with
    | x, [] -> [ x ]
    | x, rest -> x :: listToTreeHelper rest

let parseConfig path = task {
    let configAst = runParserOnFile grammar () path (Encoding.UTF8)
    
    match configAst with
    | Success (config, _, _) -> 
        let configTree = listToTreeHelper config.statements
        let result = { threshold = config.threshold; configurationTree = configTree }
        return Result.Ok result
    | Failure (x, e, _) -> 
        return Result.Error (x, e)
    
}

let rec foldStack stopAt stack = 
    match stack with
    | [] -> []
    | [single] -> [single]
    | (_, n) :: _ when n < stopAt -> stack
    | (_, n1) :: (_, n2) :: _ when n1 = n2 -> stack
    | (i1, n1) :: (i2, n2) :: rest when n1 > n2 ->
        let i2 = { i2 with childItems = i1 :: i2.childItems}
        let stack = (i2, n2) :: rest
        foldStack stopAt stack

let rec listToTree2 (items: Statement list) stack =
    match items, stack with
    | head :: tail, [] ->
        let newStackTop = { key = head.identifier; ratio = head.ratio; childItems = [] }, head.nesting
        listToTree2 tail [newStackTop]
    | head :: tail, (stackTop, stackTopNesting) :: stackTail 
        when head.nesting > stackTopNesting ->
            let newStackTop = { key = head.identifier; ratio = head.ratio; childItems = [] }, head.nesting
            let stack = newStackTop :: (stackTop, stackTopNesting) :: stackTail
            listToTree2 tail stack
    | head :: tail, (_, stackTopNesting) :: _ 
        when head.nesting <= stackTopNesting ->
            let foldedStack = foldStack head.nesting stack 
            let newStackTop = { key = head.identifier; ratio = head.ratio; childItems = [] }, head.nesting
            let stack = newStackTop :: foldedStack
            listToTree2 tail stack
    | _ -> stack |> foldStack 0 |> List.map fst
