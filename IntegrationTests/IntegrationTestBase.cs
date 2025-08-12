using AppHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public PostgresDatabaseResource Postgresdb { get; private set; }

    private DistributedApplication _app;
    private string _postgresConnectionString;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                { $"ConnectionStrings:{Postgresdb.Name}", _postgresConnectionString },
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
        var builder = AppHostBuilder.CreateBuilder(new DistributedApplicationOptions()
        {
            DisableDashboard = true,
            AssemblyName = typeof(CustomWebApplicationFactory).Assembly.FullName
        });

        Postgresdb = builder.Resources.FirstOrDefault(x => x is PostgresDatabaseResource) as PostgresDatabaseResource;

        var app = builder.Resources.FirstOrDefault(x => x.Name == "app");
        builder.Resources.Remove(app);

        _app = builder.Build();

        await _app.StartAsync(CancellationToken.None);
        var resourceNotification = _app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotification.WaitForDependenciesAsync(app, default);
        _postgresConnectionString = await Postgresdb.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _app.DisposeAsync();
    }

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

