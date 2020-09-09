using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Billing.API.Services.Invoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Billing.API.Controllers
{
    [Authorize]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private static readonly string _invoiceFilenameRegex = @"^invoice_\d{4}-\d{2}-\d{2}_(\d+)\.pdf$";

        private readonly ILogger<InvoiceController> _logger;
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(ILogger<InvoiceController> logger, IInvoiceService invoiceService)
        {
            _logger = logger;
            _invoiceService = invoiceService;
        }

        [HttpGet("/accounts/{origin}/{clientId:int:min(0)}/invoices/")]
        public async Task<IActionResult> GetInvoices([FromRoute] string origin, [FromRoute] int clientId)
        {
            _logger.LogDebug("Getting invoices for {0} client {1}", origin, clientId);

            if (!TryGetClientPrefix(origin, out var clientPrefix))
                return BadRequest();

            var response = await _invoiceService.GetInvoices(clientPrefix, clientId);

            var list = new
            {
                Invoices = response
            };

            return Ok(list);
        }

        [HttpGet("/accounts/{origin}/{clientId:int:min(1)}/invoice/{filename}")]
        public async Task<IActionResult> GetInvoices([FromRoute] string origin, [FromRoute] int clientId, [FromRoute] string filename)
        {
            _logger.LogDebug("Getting invoice for {0} client {1} filename {2}", origin, clientId, filename);

            if (!TryGetClientPrefix(origin, out var clientPrefix))
                return BadRequest();

            var match = Regex.Match(filename, _invoiceFilenameRegex);

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
