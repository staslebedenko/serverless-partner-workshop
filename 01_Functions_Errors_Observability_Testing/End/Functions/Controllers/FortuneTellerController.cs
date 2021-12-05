using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class FortuneTellerController
    {
        private readonly ILogger log;

        public FortuneTellerController(ILogger<FortuneTellerController> log)
        {
            this.log = log;
        }

        [FunctionName("AskZoltar")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route =  "api/AskZoltar/{name}")]
            HttpRequest req,
            string name)
        {
            var rate = RatePrediction();

            var prediction = $"Zoltar speaks! {name}, your rate will be '{rate}'.";

            log.LogInformation($"Prediction is done => {prediction}");

            return (ActionResult)new OkObjectResult(prediction);
        }

        private static int RatePrediction()
        {
            var random = new Random();
            return random.Next(40, 90);
        }
    }
}
