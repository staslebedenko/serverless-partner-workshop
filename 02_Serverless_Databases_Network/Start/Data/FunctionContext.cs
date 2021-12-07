using Domain;

using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class FunctionDbContext  : DbContext
    {
        public FunctionDbContext(DbContextOptions<FunctionDbContext> options) : base(options)
        {
        }

        public DbSet<Person> TestMeterValues { get; set; }

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
    }


}
