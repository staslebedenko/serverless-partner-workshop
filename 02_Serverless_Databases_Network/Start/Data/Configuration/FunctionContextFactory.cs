using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace Data
{
    public class FunctionContextFactory : IDesignTimeDbContextFactory<FunctionDbContext>
    {
        public FunctionDbContext CreateDbContext(string[] args)
        {
            
            var sqlString = Environment.GetEnvironmentVariable("SqlConnectionString");
            var password = Environment.GetEnvironmentVariable("SqlConnectionPassword");
            var connectionString = SqlConnectionBuilder.GetConnectionString(sqlString, password);

            var optionsBuilder = new DbContextOptionsBuilder<FunctionDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FunctionDbContext(optionsBuilder.Options);
        }
    }
}
