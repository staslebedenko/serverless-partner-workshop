using Domain;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class CosmosDbContext : DbContext
    {
        public DbSet<Prediction> Prediction { get; set; }

        public CosmosDbContext(DbContextOptions<CosmosDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("Prediction");

            modelBuilder.Entity<Prediction>().ToContainer("Prediction");

            modelBuilder.Entity<Prediction>().HasNoDiscriminator();

            modelBuilder.Entity<Prediction>().HasKey(d => d.Id); 

            modelBuilder.Entity<Prediction>().HasPartitionKey(o => o.PartitionKey);

            modelBuilder.Entity<Prediction>().UseETagConcurrency();
        }

        public static void ExecuteMigrations(string cosmosUrl, string cosmosKey, string cosmosDb)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CosmosDbContext>();
            optionsBuilder.UseCosmos(cosmosUrl, cosmosKey, cosmosDb);

            using var context = new CosmosDbContext(optionsBuilder.Options);

            try
            {
                context.Database.EnsureCreatedAsync();
            }
            catch (Exception e)
            {
                throw new Exception($"Error when migrating database: {e.Message}");
            }
        }
    }
}
