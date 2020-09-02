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

            string clientPrefix;

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
                    return BadRequest();
            }

            var response = await _invoiceService.GetInvoices(clientPrefix, clientId);

            var list = new
            {
                Invoices = response
            };

            return Ok(list);
        }

        [HttpGet("/testSap")]
        public async Task<IActionResult> TestSapConnection()
        {
            var response = await _invoiceService.TestSapConnection();

            return Ok(response);
        }
    }
}
