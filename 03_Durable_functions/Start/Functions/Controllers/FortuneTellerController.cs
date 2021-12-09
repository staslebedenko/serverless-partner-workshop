using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Domain;
using Data;

namespace Functions
{
    public class FortuneTellerController : BaseController
    {
        private readonly FunctionDbContext context;
        private readonly CosmosDbContext cosmosContext;

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
            [HttpTrigger(AuthorizationLevel.Function, "get", Route =  "api/AskZoltar/{name}")]
            HttpRequest req,
            string name)
        {
            var rate = RatePrediction();

            var prediction = $"Zoltar speaks! {name}, your rate will be '{rate}'.";

            base.LogInformation($"Prediction is done => {prediction}");

            await this.SavePerson(name, rate);

            await this.SavePrediction(rate);

            // throw new NotImplementedException();

            return (ActionResult)new OkObjectResult(prediction);
        }

        private static int RatePrediction()
        {
            var random = new Random();
            return random.Next(40, 90);
        }

        private async Task SavePerson(string name, int rate)
        {
            var person = new Person() { Name = name, Prediction = rate };
            await this.context.AddAsync(person);
            await this.context.SaveChangesAsync();
        }

        private async Task SavePrediction(int rate)
        {
            await context.Database.EnsureCreatedAsync();
            var prediction = new Prediction() { Id = new Guid(), Rate = rate, PartitionKey = "SmartStat" };
            this.cosmosContext.Add(prediction);
            await this.cosmosContext.SaveChangesAsync();
        }
    }
}
