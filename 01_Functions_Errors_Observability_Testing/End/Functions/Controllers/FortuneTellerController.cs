using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class FortuneTellerController : BaseController
    {
        public FortuneTellerController(
            ILogger<FortuneTellerController> logger,
            IHttpContextAccessor httpContextAccessor) : base(logger, httpContextAccessor)
        {

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

            // throw new NotImplementedException();

            return (ActionResult)new OkObjectResult(prediction);
        }

        private static int RatePrediction()
        {
            var random = new Random();
            return random.Next(40, 90);
        }
    }
}
