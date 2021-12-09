using System;
using Data;
using Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(options =>
            {
                options.AddFilter("Functions", LogLevel.Information);
            });
            string sqlString = Environment.GetEnvironmentVariable("PartnerSqlString");
            string password = Environment.GetEnvironmentVariable("PartnerSqlPassword");
            string connectionString = SqlConnectionBuilder.GetConnectionString(sqlString, password);

            //builder.Services.AddDbContextPool<FunctionDbContext>(
            builder.Services.AddDbContext<FunctionDbContext>(
               options =>
               {
                   if (!string.IsNullOrEmpty(connectionString))
                   {
                       options.UseSqlServer(connectionString, providerOptions => providerOptions.EnableRetryOnFailure());
                   }
               });

            FunctionDbContext.ExecuteMigrations(connectionString);

            string cosmosUrl = Environment.GetEnvironmentVariable("PartnerCosmosUrl");
            string cosmosKey = Environment.GetEnvironmentVariable("PartnerCosmosKey");
            string cosmosDb = Environment.GetEnvironmentVariable("PartnerCosmosDatabase");

            builder.Services.AddDbContext<CosmosDbContext>(options =>
            {
                options.UseCosmos(
                    cosmosUrl,
                    cosmosKey,
                    cosmosDb);
            });

            CosmosDbContext.ExecuteMigrations(cosmosUrl, cosmosKey, cosmosDb);
        }
    }
}
