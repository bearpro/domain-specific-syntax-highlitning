using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using LanguageServer;

var logger = Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File("log.txt")
    .MinimumLevel.Verbose()
    .CreateLogger();

logger.Information("Setting up server");

var server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options => options
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
            services.AddSingleton<LanguageServer.HierarchicalAnalysis.ITokenPriorityAnalyzer, LanguageServer.HierarchicalAnalysis.TokenPriorityAnalyzer>();
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
