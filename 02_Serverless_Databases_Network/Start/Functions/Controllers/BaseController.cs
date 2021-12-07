using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Functions
{
#pragma warning disable CS0618 // Type or member is obsolete
    public abstract class BaseController : IFunctionExceptionFilter
#pragma warning restore CS0618 // Type or member is obsolete
    {
        private readonly ILogger<BaseController> logger;

        private readonly IHttpContextAccessor httpContextAccessor;

        protected BaseController(
            ILogger<BaseController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public async Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cts)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            string errorMessage = string.Empty;
            this.RegisterException(exceptionContext.Exception, errorMessage);

            HttpResponse response = this.httpContextAccessor.HttpContext.Response;
            response.ContentType = "text/plain";

            switch (exceptionContext.Exception.InnerException)
            {
                default:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorMessage = GetErrorMessage(exceptionContext.FunctionName);
                    break;
            }

            response.ContentLength = errorMessage.Length;

            await response.WriteAsync(errorMessage, Encoding.UTF8, cts);
        }

        protected void RegisterException(Exception ex, string message)
        {
            this.logger.LogError(ex, message);
        }

        protected void LogInformation(string message)
        {
            this.logger.LogInformation(message);
        }

        private static string GetErrorMessage(string functionName)
        {
            return $"Error in {functionName}";
        }
    }
}