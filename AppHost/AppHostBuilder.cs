using Npgsql;
using Projects;
namespace AppHost;

public static class AppHostBuilder
{
    public static IDistributedApplicationBuilder CreateBuilder(DistributedApplicationOptions options)
    {
        var builder = DistributedApplication.CreateBuilder(options);

        var postgres = builder.AddPostgres("postgres");
        postgres.WithPgAdmin();
        var postgresdb = postgres.AddDatabase("appdb");

        postgresdb.OnResourceReady(async (db, e, ct) =>
        {
            var conn = await db.ConnectionStringExpression.GetValueAsync(ct);
            using var dataSource = NpgsqlDataSource.Create(conn!);
            await Task.Delay(5000); // make this delay longer to manifest the issue
            await dataSource.CreateCommand("""
                create table person (
                    id bigint primary key,
                    name text
                );
                INSERT INTO person (id, name) VALUES (1, 'Alice');
                INSERT INTO person (id, name) VALUES (2, 'Bob');
                """).ExecuteNonQueryAsync(ct);
        });

        builder.AddProject<App>("app")
            .WithReference(postgresdb)
            .WaitFor(postgresdb);

        return builder;
    }
}
