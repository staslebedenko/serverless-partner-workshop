using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Domain;
using Data;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;

namespace Functions
{
    public class FortuneTellerController : BaseController
    {
        private readonly FunctionDbContext context;

        private readonly CosmosDbContext cosmosContext;

        private record NameParameter(string Name);

        public FortuneTellerController(
            ILogger<FortuneTellerController> logger,
            IHttpContextAccessor httpContextAccessor,
            FunctionDbContext context,
            CosmosDbContext cosmosContext) : base(logger, httpContextAccessor)
        {
            this.context = context;
            this.cosmosContext = cosmosContext;
        }

        [FunctionName("AskZoltar")]
        public async Task<IActionResult> AskZoltar(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/AskZoltar/{name}")] HttpRequest req,
            string name,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            string instanceId = await starter.StartNewAsync("ZoltarOrchestrator", input: name);

            base.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("ZoltarOrchestrator")]
        public static async Task<List<string>> ZoltarOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var rate = await context.CallActivityAsync<int>("ZoltarActivitySql", context.GetInput<string>());
            outputs.Add(rate.ToString());

            outputs.Add(await context.CallActivityAsync<string>("ZoltarActivityCosmos", rate));

            return outputs;
        }

        [FunctionName("ZoltarActivitySql")]
        public async Task<string> ZoltarActivitySql([ActivityTrigger] string name)
        {
            var rate = RatePrediction();
            var person = new Person() { Name = name, Prediction = rate };
            await this.context.AddAsync(person);
            await this.context.SaveChangesAsync();

            base.LogInformation($"Saved a new rate {rate} for {name}.");
            return rate.ToString();
        }

        [FunctionName("ZoltarActivityCosmos")]
        public async Task<string> ZoltarActivityCosmos([ActivityTrigger] string rate)
        {
            Int32.TryParse(rate, out int correctRate);
            var prediction = new Prediction() { Id = new Guid(), Rate = correctRate, PartitionKey = "SmartStat" };
            this.cosmosContext.Add(prediction);
            await this.cosmosContext.SaveChangesAsync();

            return $"Saved for statistics {rate}";
        }

        private static int RatePrediction()
        {
            var random = new Random();
            return random.Next(40, 90);
        }
    }
}
