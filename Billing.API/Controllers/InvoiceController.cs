using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Billing.API.DopplerSecurity;
using Billing.API.Models;
using Billing.API.Services.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Billing.API.Controllers
{
    [Authorize]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private const string INVOICE_FILENAME_REGEX = @"^invoice_\d{4}-\d{2}-\d{2}_(\d+)\.pdf$";

        private readonly ILogger<InvoiceController> _logger;
        private readonly IInvoiceService _invoiceService;
        private readonly CryptoHelper _cryptoHelper;
        private readonly IOptions<InvoiceProviderOptions> _options;

        public InvoiceController(ILogger<InvoiceController> logger, IInvoiceService invoiceService, CryptoHelper cryptoHelper, IOptions<InvoiceProviderOptions> options)
        {
            _logger = logger;
            _invoiceService = invoiceService;
            _cryptoHelper = cryptoHelper;
            _options = options;
        }

        [HttpGet("/accounts/{origin}/{clientId:int:min(0)}/invoices/")]
        public async Task<IActionResult> GetInvoices([FromRoute] string origin,
            [FromRoute] int clientId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortColumn = "AccountId",
            [FromQuery] bool sortAsc = true)
        {
            _logger.LogDebug("Getting invoices for {0} client {1}", origin, clientId);

            if (!TryGetClientPrefix(origin, out var clientPrefix))
                return BadRequest();

            var response = await _invoiceService.GetInvoices(clientPrefix, clientId, page, pageSize, sortColumn, sortAsc);

            foreach (var invoice in response.Items)
            {
                invoice.Links.Add(new Link
                {
                    Rel = "file",
                    Href = $"{_options.Value.BaseUrl}/accounts/{origin}/{clientId}/invoices/{invoice.Filename}?s={_cryptoHelper.GenerateInvoiceSign($"{clientPrefix}{clientId}{invoice.FileId}")}",
                    Description = "Download invoice"
                });
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("/accounts/{origin}/{clientId:int:min(1)}/invoices/{filename}")]
        public async Task<IActionResult> GetInvoiceFile([FromRoute] string origin, [FromRoute] int clientId, [FromRoute] string filename, [FromQuery(Name = "s")] string signature)
        {
            _logger.LogDebug("Getting invoice for {0} client {1} filename {2}", origin, clientId, filename);

            if (!TryGetClientPrefix(origin, out var clientPrefix))
                return BadRequest();

            var match = Regex.Match(filename, INVOICE_FILENAME_REGEX);

            if (!match.Success)
                return BadRequest();

            var fileId = match.Groups[1].Value.ToInt32();

            var response = await _invoiceService.GetInvoiceFile(clientPrefix, clientId, fileId);

            if (response == null)
                return NotFound();

            return File(response, "application/pdf", filename);
        }

        [HttpGet("/testSap")]
        public async Task<IActionResult> TestSapConnection()
        {
            var response = await _invoiceService.TestSapConnection();

            return Ok(response);
        }

        private static bool TryGetClientPrefix(string origin, out string clientPrefix)
        {
            switch (origin.ToLowerInvariant())
            {
                case "doppler":
                    clientPrefix = "CD";

                    break;
                case "relay":
                    clientPrefix = "CR";

                    break;
                case "clientmanager":
                    clientPrefix = "CM";

                    break;
                default:
                    clientPrefix = string.Empty;

                    return false;
            }

            return true;
        }
    }
}
