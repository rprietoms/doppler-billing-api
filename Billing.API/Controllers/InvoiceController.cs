using System.IO;
using System.Text;
using System.Threading.Tasks;
using Billing.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Billing.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[Controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(ILogger<InvoiceController> logger, IInvoiceService invoiceService)
        {
            _logger = logger;
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices(string clientId)
        {
            _logger.LogDebug("Getting invoices for client {0}", clientId);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Sample file"));

            var response = await _invoiceService.GetInvoices(clientId);

            if (response == null)
                return NotFound();

            return File(stream, "application/octet-stream");
        }

        [HttpGet]
        public Task<IActionResult> TestSapConnection()
        {
            return Task.FromResult<IActionResult>(Ok("Message"));
        }
    }
}
