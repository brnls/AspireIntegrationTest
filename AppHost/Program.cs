using AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL server resource
IResourceBuilder<PostgresDatabaseResource> postgresdb = builder.AddAppDatabase();

// Reference the database from the App project
builder.AddProject<App>("app")
    .WithReference(postgresdb)
    .WaitFor(postgresdb);

builder.Build().Run();
