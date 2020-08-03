using System.IO;
using System.Linq;
using System.Text;
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

        [HttpGet("/invoices/{clientId}")]
        public async Task<IActionResult> GetInvoices([FromRoute] string clientId)
        {
            if (clientId.Length != 15)
                return BadRequest();

            _logger.LogDebug("Getting invoices for client {0}", clientId);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Sample file"));

            var response = await _invoiceService.GetInvoices(clientId);

            if (response == null || !response.Any())
                return NotFound();

            return File(stream, "application/octet-stream");
        }

        [HttpGet("/invoices/test")]
        public Task<IActionResult> TestSapConnection()
        {
            return Task.FromResult<IActionResult>(Ok("Message"));
        }
    }
}
