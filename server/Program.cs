using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Newtonsoft.Json.Linq;
// using OmniSharp.Extensions.LanguageServer.Protocol.Models;

var logger = Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File("log.txt")
    .MinimumLevel.Verbose()
    .CreateLogger();

logger.Information("Setupping server");

var server = await LanguageServer.From(options => options
    .WithInput(Console.OpenStandardInput())
    .WithOutput(Console.OpenStandardOutput())
    .ConfigureLogging(log => log
        .AddSerilog()
        .AddLanguageProtocolLogging()
        .SetMinimumLevel(LogLevel.Debug))
    .WithHandler<SemanticTokensHandler>()
    .WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))
    .WithServices(
        services =>
        {
            logger.Information("Configuring services");
        }
    )
    .OnInitialize(
        async (server, request, token) =>
        {
            logger.Information("Initialize...");
            await Task.Yield();
        }
    )
    .OnInitialized(
        async (server, request, response, token) =>
        {
            logger.Information("Initialized");
            await Task.Yield();
        }
    )
    .OnStarted(
        async (languageServer, token) =>
        {
            logger.Information("Started");
            await Task.Yield();
        }
    )
).ConfigureAwait(false);

await server.WaitForExit.ConfigureAwait(false);

logger.Information("Exit");
