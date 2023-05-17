using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

public class SemanticTokensHandler : SemanticTokensHandlerBase
{
    private readonly ILogger _logger;

    public SemanticTokensHandler(ILogger<SemanticTokensHandler> logger)
    {
        _logger = logger;
    }

    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(
        SemanticTokensCapability capability, 
        ClientCapabilities clientCapabilities)
    {
        return new SemanticTokensRegistrationOptions
        {
            DocumentSelector = DocumentSelector.ForPattern("**/*.smpl.dssh"),
            Legend = new SemanticTokensLegend
            {
                TokenTypes = new Container<SemanticTokenType>(
                    SemanticTokenType.Comment,
                    SemanticTokenType.Keyword,
                    SemanticTokenType.Function,
                    SemanticTokenType.Method,
                    SemanticTokenType.String,
                    SemanticTokenType.Variable
                ),
                TokenModifiers = new Container<SemanticTokenModifier>(
                    "sql-captured",
                    "concurrency-captured",
                    "concurrency-modified",
                    "ui-captured",
                    SemanticTokenModifier.Async
                )
            },
            Full = true
        };
    }

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(
        ITextDocumentIdentifierParams @params, 
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
    }

    protected override async Task Tokenize(
        SemanticTokensBuilder builder, 
        ITextDocumentIdentifierParams identifier, 
        CancellationToken cancellationToken)
    {
        var path = DocumentUri.GetFileSystemPath(identifier);
        if (path is null)
        {
            _logger.LogWarning("Invalid document identifier: {identifier}", identifier);
            return;
        }

        var content = await File.ReadAllLinesAsync(path, cancellationToken)
            .ConfigureAwait(false);

        foreach (var (lineContent, lineNumber) in content.Select((l, i) => (l, i)))
        {
            var queue = new PriorityQueue<(int, int, string, string[]), int>();

            if (lineContent.StartsWith('#'))
            {
                queue.Enqueue((
                    0,
                    lineContent.Length,
                    SemanticTokenType.Comment,
                    Array.Empty<string>()
                ),
                0);
            }

            var foundKeywords = FindKeywords(lineContent, new [] {"var", "do", "fun"});
            foreach (var (keywordStart, keywordLen, _) in foundKeywords)
            {
                queue.Enqueue((
                    keywordStart,
                    keywordLen,
                    SemanticTokenType.Keyword,
                    Array.Empty<string>()),
                    keywordStart);
            }

            var stringMatches = Regex.Matches(lineContent, "\".*?\"", RegexOptions.Singleline);
            foreach (var match in stringMatches.Select(x => x))
            {
                queue.Enqueue((
                    match.Index,
                    match.Length,
                    SemanticTokenType.String,
                    Array.Empty<string>()),
                    match.Index);
            }
            
            var funcCallMatches = Regex.Matches(lineContent, "\\w+?\\(", RegexOptions.Singleline);
            foreach (var match in funcCallMatches.Select(x => x))
            {
                queue.Enqueue((
                    match.Index,
                    match.Length-1,
                    SemanticTokenType.Method,
                    Array.Empty<string>()), 
                    match.Index);
            }

            var foundVariables = FindKeywords(lineContent, new [] {
                "shared_counter",
                "increment",
                "counter_label",
                "counterId",
                "sql_session",
                "ui"});
            foreach (var (keywordStart, keywordLen, variable) in foundVariables)
            {
                string[] modifiers = variable switch 
                {
                    "shared_counter" => new[] { "concurrency-modified", "concurrency-captured", "ui-captured", "sql-captured" },
                    "increment" => new[] { "concurrency-captured" },
                    "counter_label" => new[] { "ui-captured" },
                    "counterId" => new[] { "sql-captured" },
                    "ui" => Array.Empty<string>(),
                    _ => Array.Empty<string>()
                };

                queue.Enqueue((
                    keywordStart,
                    keywordLen,
                    SemanticTokenType.Variable,
                    modifiers),
                    keywordStart);
            }

            while (queue.TryDequeue(out var item, out var priority))
            {
                var (start, len, token, modifiers) = item;
                builder.Push(
                    lineNumber,
                    start,
                    len,
                    token,
                    modifiers);
            }
        }
    }

    protected List<(int, int, string)> FindKeywords(
        string line, 
        string[] keywords, 
        int startFrom = 0,
        List<(int, int, string)>? foundKeywords = null)
    {
        foundKeywords ??= new();

        var keywordIndex = -1;
        var keywordLenght = 0;
        string? foundKeyword = null;

        foreach (var keyword in keywords)
        {
            var index = line.IndexOf(keyword, startIndex: startFrom);
            if (index != -1)
            {
                keywordIndex = index;
                keywordLenght = keyword.Length;
                foundKeyword = keyword;
                break;
            }
            continue;
        }

        if (keywordIndex == -1)
        {
            return foundKeywords;
        }

        startFrom = keywordIndex + keywordLenght;
        foundKeywords.Add((keywordIndex, keywordLenght, foundKeyword!));
        
        return FindKeywords(line, keywords, startFrom, foundKeywords);
    }
}