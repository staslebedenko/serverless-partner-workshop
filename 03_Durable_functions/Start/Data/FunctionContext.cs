using Domain;

using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class FunctionDbContext  : DbContext
    {
        public FunctionDbContext(DbContextOptions<FunctionDbContext> options) : base(options)
        {
        }

        public DbSet<Person> Person { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            FunctionDbContextConfig.Configure(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        /*
          * run all migrations via command line
          * dotnet ef migrations add InitialMigration --context FunctionDbContext
          * dotnet ef database update
         */

        public static void ExecuteMigrations(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FunctionDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new FunctionDbContext(optionsBuilder.Options);

            try
            {
                context.Database.Migrate();
            }
            catch (Exception e)
            {
                throw new Exception($"Error when migrating database: {e.Message}");
            }
        }
    }


}
