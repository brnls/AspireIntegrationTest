using App;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("appdb")));

var app = builder.Build();

app.MapGet("/", async (AppDbContext db) =>
{
    return Results.Ok(await db.People.ToListAsync());
});

app.Run();

public partial class Program
{
    
}
