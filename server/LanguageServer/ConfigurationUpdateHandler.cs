using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServerSample;

class ConfigurationUpdateHandler : DidSaveTextDocumentHandlerBase
{
    private readonly ILogger<ConfigurationUpdateHandler> logger;
    private readonly ConfigurationStateHolder configState;

    public ConfigurationUpdateHandler(
        ILogger<ConfigurationUpdateHandler> logger,
        ConfigurationStateHolder configState)
    {
        this.logger = logger;
        this.configState = configState;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        logger.LogInformation(request.Text);

        return Task.FromResult(Unit.Value);
    }

    protected override TextDocumentSaveRegistrationOptions CreateRegistrationOptions(
        SynchronizationCapability capability, 
        ClientCapabilities clientCapabilities)
    {
        return new TextDocumentSaveRegistrationOptions()
        {
            DocumentSelector = DocumentSelector.ForPattern("**/*.dsh.conf"),
            IncludeText = true
        };
    }
}