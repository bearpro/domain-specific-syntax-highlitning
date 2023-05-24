module LanguageServer.HierarchicalAnalysis.ConfigurationGrammar

open FParsec

type Statement = {
    nesting: int
    identifier: string
    ratio: float
}

type ConfigAstData = {
    threshold: int
    statements: Statement list
}

let ws = spaces
let str_ws s = pstring s .>> ws

let nesting char = many (pchar char) |>> fun chars -> List.length chars
let identifier = identifier (IdentifierOptions()) .>> ws

let threshold = pchar '#' >>. ws >>. pstring "threshold" >>. ws >>. pint32 .>> many1 newline

let statement = 
    let grammar = nesting ' ' .>>. identifier .>> (str_ws "=") .>>. pfloat .>> many1 newline
    grammar |>> fun ((nesting, token), ratio) -> { nesting = nesting; identifier = token; ratio = ratio} 

let grammar = 
    threshold .>>. many statement 
    |>> fun (threshold, statements) -> {threshold = threshold; statements = statements}

let test (input: string) = run grammar input

()