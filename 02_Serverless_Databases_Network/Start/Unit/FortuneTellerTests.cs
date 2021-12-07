using AutoFixture;
using Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Unit
{
    public class FortuneTellerTests
    {
        private Fixture? fixture;

        [SetUp]
        public void Setup()
        {
            this.fixture = new Fixture();
        }

        [Test]
        public async Task AskZoltar_WithNameParameter_ReturnOkResult()
        {
            var tellerLogger = new Mock<ILogger<FortuneTellerController>>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
             
            var instance = new FortuneTellerController(tellerLogger.Object, httpContextAccessor.Object);

            var result = await instance.AskZoltar(null, "Test");

            Assert.That(result, Is.TypeOf<OkObjectResult>());
        }
    }
}