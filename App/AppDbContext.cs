using Microsoft.EntityFrameworkCore;

namespace App
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(p =>
            {
                p.ToTable("person");
                p.Property(x => x.Id).HasColumnName("id");
                p.Property(x => x.Name).HasColumnName("name");
            });
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
