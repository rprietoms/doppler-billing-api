using System.Linq;
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

        [HttpGet("/invoices/{clientId:length(15)}")]
        public async Task<IActionResult> GetInvoices([FromRoute] string clientId)
        {
            _logger.LogDebug("Getting invoices for client {0}", clientId);

            var response = await _invoiceService.GetInvoices(clientId);

            if (response == null || !response.Any())
                return NotFound();

            return Ok(response);
        }

        [HttpGet("/invoices/test")]
        public async Task<IActionResult> TestSapConnection()
        {
            var response = await _invoiceService.TestSapConnection();

            return Ok(response);
        }
    }
}
