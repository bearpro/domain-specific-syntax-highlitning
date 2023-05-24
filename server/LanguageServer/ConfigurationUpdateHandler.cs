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
            var isSuccess = await tokenPriorityAnalyzer
                .ReadConfigurationFromFile(path);

            if (!isSuccess)
            {
                logger.LogError("Configuration update not completed with success");
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update configuration from file");
        }
        
        return Unit.Value;
    }

}