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
                // TokenTypes = capability.TokenTypes,
                TokenTypes = new Container<SemanticTokenType>(
                    SemanticTokenType.Comment,
                    SemanticTokenType.Keyword,
                    SemanticTokenType.Function
                ),
                TokenModifiers = capability.TokenModifiers
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
            if (lineContent.StartsWith('#'))
            {
                builder.Push(
                    line: lineNumber, 
                    @char: 0, 
                    length: lineContent.Length, 
                    tokenType: (string)SemanticTokenType.Comment);
            }
            else if (lineContent.StartsWith("do"))
            {
                builder.Push(
                    line: lineNumber,
                    @char: 0,
                    length: 2,
                    tokenType: (string)SemanticTokenType.Keyword);
            }
        }
    }
}