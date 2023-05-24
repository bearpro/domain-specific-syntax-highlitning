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