using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using LanguageServer.HierarchicalAnalysis;
using System.Threading;

namespace LanguageServer;

static class ConfigurationUpdateHandler
{
    public static async Task<Unit> HandleConfigurationUpdateAsync(
        ITokenPriorityAnalyzer tokenPriorityAnalyzer, 
        string path,
        ILogger logger)
    {
        try
        {
            var config = await tokenPriorityAnalyzer.UpdateConfiguration(path);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to parse configuration");
        }
        
        return Unit.Value;
    }

}