using AutoFixture.Xunit2;
using Flurl.Http.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Billing.API.Models;
using Newtonsoft.Json;
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
        [InlineData(HttpStatusCode.OK, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.j1qzmKcnpCCBoXAtK9QuzCcnkIedK_kpwlrQ315VX_bwuxNxDBeEgKCOcjACUaNnf92bStGVYxXusSlnCgWApjlFG4TRgcTNsBC_87ZMuTgjP92Ou_IHi5UVDkiIyeQ3S_-XpYGFksgzI6LhSXu2T4LZLlYUHzr6GN68QWvw19m1yw6LdrNklO5qpwARR4WEJVK-0dw2-t4V9jK2kR8zFkTYtDUFPEQaRXFBpaPWAdI1p_Dk_QDkeBbmN_vTNkF7JwmqXRRAaz5fiMmcgzFmayJFbM0Y9LUeaAYFSZytIiYZuNitVixWZEcXT_jwtfHpyDwZKY1-HlyMmUJJuVsf2A")]
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ3MDksImV4cCI6MTU5Nzc2NDcxOSwiaWF0IjoxNTk3NzY0NzA5LCJpc1NVIjpmYWxzZX0.K7khi_qhvj0eF3ahZzNcRkzrRPDFR_q-5xAujSeFG3GaFhJIhgARX7fsA4iPPhTJtFA1oqF54d-vyNhGAhBDFzSKUHyRegdRJ5FiQwcQ537PbZUfCc702sEi-MjzfpkP1PZrk0Zrn5-ybUDJi-6qjia8_YxvA4px8KGPT10Z6PnrpeCuWtESmMlSre7CgCRpydXZ0XkV0hsn-CD8p5oSV9iMCXS3npJBBhzLvw9B_LienlnJQMVs88ykSDqZNUWdGMVTO4QF4JChd67W7B9I0MmmbtgCZ5yo0EwykYR6RaZYihtKjesmHlBcFaHJc1C-3V8TQ3L0-81PpemqZd_3yQ")]
        [InlineData(HttpStatusCode.Forbidden, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1NzcsImV4cCI6MTU5Nzc2NDU4NywiaWF0IjoxNTk3NzY0NTc3fQ.S3qzN2kCR7VtrkCG-FTq_Hrv377Fn8wevryAhHHKq5SupMsEaa1SYAdNZdlMLZyyZQUe95UYM4_Ba63Kbm9zu6fkh_xKfmLGbiZhEjJM5nVR0HLa7mAPTNY25YrfRtQRyyLvLDJ1KSXIY_iUd1IT1hQAIqMG7pD29eD6RY4_n_z619AgET94F_Jj7w505JvNNR7z5fpW5ZM1XaEPlrCbXVfCKtLLxM8YlNRBOmyJRG2ideaRfqEw7vb3AIW6c4EdHV1c9EBsYGfWkSJZOOpXKoOpUmvhVLmxpctTNNq_iS67JE3AFlkatboq9z8l9DHDIdoveIE6unHq4YgUmltnDg")]
        [InlineData(HttpStatusCode.Unauthorized, "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9")]
        public async Task GetInvoices_WhenToken_ReturnsResponse(HttpStatusCode httpStatusCode, string token)
        {
            // Arrange
            const int clientId = 1;

            _httpTest.RespondWithJson(string.Empty);

            using (var appFactory = _factory.WithDisabledLifeTimeValidation())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true",
                    ["Invoice:Host"] = "localhost",
                    ["Invoice:UserName"] = "someUser",
                    ["Invoice:Password"] = "somePass"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost/accounts/doppler/{clientId}/invoices");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Act
                var response = await client.SendAsync(request);

                // Assert
                Assert.Equal(httpStatusCode, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_ShouldNotCallBackend_ReturnsOk()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/doppler/1/invoices");

                // Act
                var response = await client.SendAsync(request);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_Should_Sort_By_Product_Asc_When_OrderAsc_is_not_Passed_As_Parameter()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?sortColumn=Product");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Collection(result.Items,
                    invoice1 => Assert.Equal("Prod 1", invoice1.Product),
                    invoice2 => Assert.Equal("Prod 10", invoice2.Product),
                    invoice3 => Assert.Equal("Prod 11", invoice3.Product),
                    invoice4 => Assert.Equal("Prod 12", invoice4.Product),
                    invoice5 => Assert.Equal("Prod 13", invoice5.Product),
                    invoice6 => Assert.Equal("Prod 14", invoice6.Product),
                    invoice7 => Assert.Equal("Prod 15", invoice7.Product),
                    invoice8 => Assert.Equal("Prod 16", invoice8.Product),
                    invoice9 => Assert.Equal("Prod 17", invoice9.Product),
                    invoice10 => Assert.Equal("Prod 18", invoice10.Product));
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_Should_Sort_By_Product_Desc_When_OrderAsc_is_False()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?sortColumn=Product&sortAsc=false");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Collection(result.Items,
                    invoice1 => Assert.Equal("Prod 9", invoice1.Product),
                    invoice2 => Assert.Equal("Prod 8", invoice2.Product),
                    invoice3 => Assert.Equal("Prod 7", invoice3.Product),
                    invoice4 => Assert.Equal("Prod 6", invoice4.Product),
                    invoice5 => Assert.Equal("Prod 50", invoice5.Product),
                    invoice6 => Assert.Equal("Prod 5", invoice6.Product),
                    invoice7 => Assert.Equal("Prod 49", invoice7.Product),
                    invoice8 => Assert.Equal("Prod 48", invoice8.Product),
                    invoice9 => Assert.Equal("Prod 47", invoice9.Product),
                    invoice10 => Assert.Equal("Prod 46", invoice10.Product));
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_Should_Sort_By_Default_When_sortColumn_And_sortAsc_Are_Empty()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?pageSize=2");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Equal(2, result.Items.Count);
                Assert.Collection(result.Items,
                    invoice1 => Assert.Equal("CD0000000000001", invoice1.AccountId),
                    invoice2 => Assert.Equal("CD0000000000001", invoice2.AccountId));
            }
        }

        [Fact]
        public async Task GetInvoices_WhenDummyDataIsTrue_Should_Sort_By_Product_Asc_When_Page_is_2()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices?page=2&sortColumn=Product&sortAsc=true");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PaginatedResult<InvoiceListItem>>(content);

                // Assert
                _httpTest.ShouldNotHaveMadeACall();

                Assert.Collection(result.Items,
                    invoice1 => Assert.Equal("Prod 19", invoice1.Product),
                    invoice2 => Assert.Equal("Prod 2", invoice2.Product),
                    invoice3 => Assert.Equal("Prod 20", invoice3.Product),
                    invoice4 => Assert.Equal("Prod 21", invoice4.Product),
                    invoice5 => Assert.Equal("Prod 22", invoice5.Product),
                    invoice6 => Assert.Equal("Prod 23", invoice6.Product),
                    invoice7 => Assert.Equal("Prod 24", invoice7.Product),
                    invoice8 => Assert.Equal("Prod 25", invoice8.Product),
                    invoice9 => Assert.Equal("Prod 26", invoice9.Product),
                    invoice10 => Assert.Equal("Prod 27", invoice10.Product));
            }
        }

        [Fact]
        public async Task GetInvoices_ReturnsData()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/doppler/1/invoices");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.NotNull(content);
            }
        }

        [Fact]
        public async Task GetInvoices_WhenValidToken_ReturnsOk()
        {
            // Arrange
            _httpTest.RespondWithJson(string.Empty);

            var appFactory = _factory.WithDisabledLifeTimeValidation();

            var client = appFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/accounts/doppler/1/invoices");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYmYiOjE1OTc3NjQ1MjIsImV4cCI6MTU5Nzc2NDUzMiwiaWF0IjoxNTk3NzY0NTIyLCJpc1NVIjp0cnVlfQ.j1qzmKcnpCCBoXAtK9QuzCcnkIedK_kpwlrQ315VX_bwuxNxDBeEgKCOcjACUaNnf92bStGVYxXusSlnCgWApjlFG4TRgcTNsBC_87ZMuTgjP92Ou_IHi5UVDkiIyeQ3S_-XpYGFksgzI6LhSXu2T4LZLlYUHzr6GN68QWvw19m1yw6LdrNklO5qpwARR4WEJVK-0dw2-t4V9jK2kR8zFkTYtDUFPEQaRXFBpaPWAdI1p_Dk_QDkeBbmN_vTNkF7JwmqXRRAaz5fiMmcgzFmayJFbM0Y9LUeaAYFSZytIiYZuNitVixWZEcXT_jwtfHpyDwZKY1-HlyMmUJJuVsf2A");

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
            var response = await client.GetAsync("https://custom.domain.com/accounts/doppler/1/invoices");

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
            var request = new HttpRequestMessage(HttpMethod.Get, "https://custom.domain.com/accounts/doppler/1/invoices");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjg4NDY5LCJ1bmlxdWVfbmFtZSI6ImFtb3NjaGluaUBtYWtpbmdzZW5zZS5jb20iLCJpc1N1IjpmYWxzZSwic3ViIjoiYW1vc2NoaW5pQG1ha2luZ3NlbnNlLmNvbSIsImN1c3RvbWVySWQiOiIxMzY3IiwiY2RoX2N1c3RvbWVySWQiOiIxMzY3Iiwicm9sZSI6IlVTRVIiLCJpYXQiOjE1OTQxNTUwMjYsImV4cCI6MTU5NDE1NjgyNn0.a4eVqSBptPJk0y9V5Id1yXEzkSroX7j9712W6HOYzb-9irc3pVFQrdWboHcZPLlbpHUdsuoHmFOU-l14N_CjVF9mwjz0Qp9x88JP2KD1x8YtlxUl4BkIneX6ODQ5q_hDeQX-yIUGoU2-cIXzle-JzRssg-XIbaf34fXnUSiUGnQRAuWg3IkmpeLu9fVSbYrY-qW1os1gBSq4NEESz4T87hJblJv3HWNQFJxAtvhG4MLX2ITm8vYNtX39pwI5gdkLY7bNzWmJ1Uphz1hR-sdCdM2oUWKmRmL7txsoD04w5ca7YbdHQGwCI92We4muOs0-N7a4JHYjuDM9lL_TbJGw2w");

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var authenticateHeader = Assert.Single(response.Headers.WwwAuthenticate);
            Assert.Equal("Bearer", authenticateHeader.Scheme);
            Assert.Contains("error=\"invalid_token\"", authenticateHeader.Parameter);
            Assert.Contains("error_description=\"The token is expired", authenticateHeader.Parameter);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_WhenNoParameters_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("https://localhost/accounts/doppler/invoices");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("accounts/doppler/1/invoice/invoice_2020-01-01_123.pdf")] //Deprecated route
        [InlineData("accounts/doppler/1/invoices/invoice_2020-01-01_123.pdf")]
        public async Task GetInvoiceFile_ShouldReturnPdfFileContents(string path)
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/{path}");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("application/pdf", response.Content.Headers.ContentType.MediaType);
                Assert.NotNull(content);
            }
        }

        [Fact]
        public async Task GetInvoiceFile_WhenInvalidClientOrigin_ReturnsBadRequest()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/invalid_origin/1/invoice/filename.ext");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.NotNull(content);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoiceFile_WhenInvalidClientId_ReturnsNotFound()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/doppler/0/invoice/invoice_2020-01-01_123.pdf");

                // Act
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                Assert.NotNull(content);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task GetInvoiceFile_WhenWrongFilePattern_ReturnsBadRequest()
        {
            // Arrange
            using (var appFactory = _factory.WithBypassAuthorization())
            {
                appFactory.AddConfiguration(new Dictionary<string, string>
                {
                    ["Invoice:UseDummyData"] = "true"
                });

                var client = appFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://custom.domain.com/accounts/doppler/1/invoice/whatever.ext");

                // Act
                var response = await client.SendAsync(request);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }
    }
}
