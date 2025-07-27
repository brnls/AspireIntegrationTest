using Npgsql;

namespace AppHost;

public static class PostgresExtensions
{
    public static IResourceBuilder<PostgresDatabaseResource> AddAppDatabase(this IDistributedApplicationBuilder builder)
    {
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
        return postgresdb;
    }
}
