using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace Data
{
    public class FunctionContextFactory : IDesignTimeDbContextFactory<FunctionDbContext>
    {
        public FunctionDbContext CreateDbContext(string[] args)
        {
            string sqlString = Environment.GetEnvironmentVariable("PartnerSqlString");
            string password = Environment.GetEnvironmentVariable("PartnerSqlPassword");
            string connectionString = SqlConnectionBuilder.GetConnectionString(sqlString, password);

            var optionsBuilder = new DbContextOptionsBuilder<FunctionDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FunctionDbContext(optionsBuilder.Options);
        }
    }
}
