using AppHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public IResourceBuilder<PostgresDatabaseResource> Postgresdb { get; }
    private IResourceBuilder<WaitResource> InitResource { get; }

    private readonly DistributedApplication _app;
    private string _postgresConnectionString;

    public CustomWebApplicationFactory()
    {
        var appBuilder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions()
        {
            DisableDashboard = true,
            AssemblyName = typeof(CustomWebApplicationFactory).Assembly.FullName
        });
        Postgresdb = appBuilder.AddAppDatabase();
        InitResource = appBuilder.AddResource(new WaitResource("init"))
            .WaitFor(Postgresdb);

        _app = appBuilder.Build();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"ConnectionStrings:{Postgresdb.Resource.Name}", _postgresConnectionString },
            });
        });

        var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDir);
        // Add file logger provider
        builder.ConfigureServices(services =>
        {
            var logPath = Path.Combine(logDir, "App.log");
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerProvider>(_ => new FileLoggerProvider(logPath));
        });

        return base.CreateHost(builder);
    }

    public async Task InitializeAsync()
    {
        var resourceNotifyService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync(CancellationToken.None);
        await resourceNotifyService.WaitForDependenciesAsync(InitResource.Resource, default);
        _postgresConnectionString = await Postgresdb.Resource.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _app.DisposeAsync();
    }

    private class WaitResource(string name) : Resource(name), IResourceWithWaitSupport { }
}

[CollectionDefinition("WebAppFactoryCollection")]
public class WebAppFactoryCollection : ICollectionFixture<CustomWebApplicationFactory> { }

[Collection("WebAppFactoryCollection")]
public class IntegrationTestBase
{
    private protected CustomWebApplicationFactory Factory;

    public IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }
}

