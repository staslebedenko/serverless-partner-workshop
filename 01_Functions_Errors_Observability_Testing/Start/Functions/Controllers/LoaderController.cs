using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions
{
    public static class LoaderController
    {
        [FunctionName("LoaderActivation")]
        public static IActionResult LoaderActivation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "loaderio-")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Loader.io validation triggered.");
            return (ActionResult)new OkObjectResult($"loaderio-");
        }
    }
}
