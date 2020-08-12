using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Billing.API.Test
{
    public class InvoiceApiValidationTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpTest _httpTest;

        public InvoiceApiValidationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _httpTest = new HttpTest();
        }

        public void Dispose()
        {
            _httpTest.Dispose();
        }

        [Theory]
        [InlineData("")]
        [InlineData("00000000000000")]
        [InlineData("0000000000000000")]
        public async Task GetInvoices_InexistentClient_ReturnsNotFound(string clientId)
        {
            // Arrange
            var appFactory = _factory.WithBypassAuthorization();
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/invoices/{clientId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            _httpTest.ShouldNotHaveMadeACall();
        }

        [Theory]
        [InlineData("000000000000000")]
        [InlineData("999999999999999")]
        public async Task GetInvoices_InexistentClient_ReturnsInternalServerError(string clientId)
        {
            // Arrange
            var appFactory = _factory.WithBypassAuthorization();
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/invoices/{clientId}");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            _httpTest.ShouldNotHaveMadeACall();
        }
    }
}
