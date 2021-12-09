using System;
using Data;
using Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

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

            var signingKey = string.Empty;
            string sqlString = string.Empty;
            string password = string.Empty;
            string cosmosUrl = string.Empty;
            string cosmosKey = string.Empty;
            string cosmosDb = string.Empty;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                SecretClient client = new SecretClient(new Uri(Environment.GetEnvironmentVariable("KeyVaultEndpoint")), new DefaultAzureCredential());

                sqlString = client.GetSecret("PartnerSqlString").Value.ToString();
                password = client.GetSecret("PartnerSqlPassword").Value.ToString();
                cosmosUrl = client.GetSecret("PartnerCosmosUrl").Value.ToString();
                cosmosKey = client.GetSecret("PartnerCosmosKey").Value.ToString();
                cosmosDb = client.GetSecret("PartnerCosmosDatabase").Value.ToString();
            }
            else
            {
                sqlString = Environment.GetEnvironmentVariable("PartnerSqlString");
                password = Environment.GetEnvironmentVariable("PartnerSqlPassword");
                cosmosUrl = Environment.GetEnvironmentVariable("PartnerCosmosUrl");
                cosmosKey = Environment.GetEnvironmentVariable("PartnerCosmosKey");
                cosmosDb = Environment.GetEnvironmentVariable("PartnerCosmosDatabase");
            }

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
