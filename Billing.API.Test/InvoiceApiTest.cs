using AutoFixture.Xunit2;
using Flurl.Http.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Billing.API.Test
{
    public class InvoiceApiTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly HttpTest _httpTest;

        public InvoiceApiTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _httpTest = new HttpTest();
        }

        public void Dispose()
        {
            _httpTest.Dispose();
        }

        [Theory]
        [AutoData]
        public async Task GetInvoices_WhenInvalidPath_ReturnsNotFound(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"https://custom.domain.com/{url}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [AutoData]
        public async Task GetInvoices_WhenValidTokenProd_ReturnsOk(string host, string expectedUserName, string expectedPassword)
        {
            // Arrange
            const string clientId = "000000000000001";
            var expectedUrl = $"http://{host}:33333/api/Invoices?clientId=000000000000001";

            _httpTest.RespondWithJson(string.Empty);

            using var appFactory = _factory.WithDisabledLifeTimeValidation()
                .AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "false",
                    ["Invoice:Host"] = host,
                    ["Invoice:UserName"] = expectedUserName,
                    ["Invoice:Password"] = expectedPassword
                });
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/invoices/{clientId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjg4NDY5LCJ1bmlxdWVfbmFtZSI6ImFtb3NjaGluaUBtYWtpbmdzZW5zZS5jb20iLCJpc1N1IjpmYWxzZSwic3ViIjoiYW1vc2NoaW5pQG1ha2luZ3NlbnNlLmNvbSIsImN1c3RvbWVySWQiOiIxMzY3IiwiY2RoX2N1c3RvbWVySWQiOiIxMzY3Iiwicm9sZSI6IlVTRVIiLCJpYXQiOjE1OTQxNTUwMjYsImV4cCI6MTU5NDE1NjgyNn0.a4eVqSBptPJk0y9V5Id1yXEzkSroX7j9712W6HOYzb-9irc3pVFQrdWboHcZPLlbpHUdsuoHmFOU-l14N_CjVF9mwjz0Qp9x88JP2KD1x8YtlxUl4BkIneX6ODQ5q_hDeQX-yIUGoU2-cIXzle-JzRssg-XIbaf34fXnUSiUGnQRAuWg3IkmpeLu9fVSbYrY-qW1os1gBSq4NEESz4T87hJblJv3HWNQFJxAtvhG4MLX2ITm8vYNtX39pwI5gdkLY7bNzWmJ1Uphz1hR-sdCdM2oUWKmRmL7txsoD04w5ca7YbdHQGwCI92We4muOs0-N7a4JHYjuDM9lL_TbJGw2w");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_ShouldNotCallBackend_ReturnsOk()
        {
            // Arrange
            using var appFactory = _factory.WithBypassAuthorization()
                .AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/invoices/000000000000001");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            _httpTest.ShouldNotHaveMadeACall();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_ReturnsFile()
        {
            // Arrange
            using var appFactory = _factory.WithBypassAuthorization()
                .AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });
            appFactory.Server.PreserveExecutionContext = true;
            var client = appFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/invoices/000000000000001");

            // Act
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            // Validate indentation
            Assert.Matches("Sample file", content);
        }

        [Fact]
        public async Task GetInvoices_WhenValidToken_ReturnsOk()
        {
            // Arrange
            _httpTest.RespondWithJson(string.Empty);

            var appFactory = _factory.WithDisabledLifeTimeValidation();
            appFactory.Server.PreserveExecutionContext = true;

            var client = appFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/invoices/000000000000001");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjUxMjAxLCJ1bmlxdWVfbmFtZSI6ImFtb3NjaGluaUBtYWtpbmdzZW5zZS5jb20iLCJpc1N1IjpmYWxzZSwic3ViIjoiYW1vc2NoaW5pQG1ha2luZ3NlbnNlLmNvbSIsImN1c3RvbWVySWQiOm51bGwsImNkaF9jdXN0b21lcklkIjpudWxsLCJyb2xlIjoiVVNFUiIsImlhdCI6MTU5NDE1NjIzNSwiZXhwIjoxNTk0MTU4MDM1fQ.iZ40PoFgqmVXBGGBdUABmewvx6byKXaM9pIkJhdlsbcs9i4TUoXZrC0TaWq3-MrFneuVhOFBXy1n5Entr9_x1JGFu_9hpxuHbh266VvmcqmTDJUO0F3fR2tc-3nwPUQzWSTZC6ArJAdHpnXhB3ysvpZVi22l0dDUOeaHNbrQEkbHc61Zo4RlSU20HQSQQ2NJKw6wUfC3iOznHyTUTLFVlJ4REbTnbOzyUKZYyBKRy_aAseJbKphmT9Lh-mjgVsY3_S6WWHzczhk3eqmb8o8QJ3O_NbQxmHR964aRQutljFv_80cc5A61YbTtgmfoEsu-7HV2FaSZtztk6jesr-3rTg");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_WhenInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.WithDisabledLifeTimeValidation()
                .CreateClient();

            // Act
            var response = await client.GetAsync("https://custom.domain.com/invoices/000000000000001");

            // Assert
            var authenticateHeader = Assert.Single(response.Headers.WwwAuthenticate);
            Assert.Equal("Bearer", authenticateHeader.Scheme);
            Assert.Null(authenticateHeader.Parameter);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_WhenTokenExpired_ReturnsUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/invoices/000000000000001");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjg4NDY5LCJ1bmlxdWVfbmFtZSI6ImFtb3NjaGluaUBtYWtpbmdzZW5zZS5jb20iLCJpc1N1IjpmYWxzZSwic3ViIjoiYW1vc2NoaW5pQG1ha2luZ3NlbnNlLmNvbSIsImN1c3RvbWVySWQiOiIxMzY3IiwiY2RoX2N1c3RvbWVySWQiOiIxMzY3Iiwicm9sZSI6IlVTRVIiLCJpYXQiOjE1OTQxNTUwMjYsImV4cCI6MTU5NDE1NjgyNn0.a4eVqSBptPJk0y9V5Id1yXEzkSroX7j9712W6HOYzb-9irc3pVFQrdWboHcZPLlbpHUdsuoHmFOU-l14N_CjVF9mwjz0Qp9x88JP2KD1x8YtlxUl4BkIneX6ODQ5q_hDeQX-yIUGoU2-cIXzle-JzRssg-XIbaf34fXnUSiUGnQRAuWg3IkmpeLu9fVSbYrY-qW1os1gBSq4NEESz4T87hJblJv3HWNQFJxAtvhG4MLX2ITm8vYNtX39pwI5gdkLY7bNzWmJ1Uphz1hR-sdCdM2oUWKmRmL7txsoD04w5ca7YbdHQGwCI92We4muOs0-N7a4JHYjuDM9lL_TbJGw2w");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var authenticateHeader = Assert.Single(response.Headers.WwwAuthenticate);
            Assert.Equal("Bearer", authenticateHeader.Scheme);
            Assert.Contains("error=\"invalid_token\"", authenticateHeader.Parameter);
            Assert.Contains("error_description=\"The token expired at ", authenticateHeader.Parameter);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_WhenNoParameters_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("https://custom.domain.com/Invoices/");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
