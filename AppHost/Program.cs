using AppHost;

var appHost = AppHostBuilder.CreateBuilder(new DistributedApplicationOptions() { Args = args });

appHost.Build().Run();

