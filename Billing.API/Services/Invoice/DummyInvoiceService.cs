using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Billing.API.Models;

namespace Billing.API.Services.Invoice
{
    public class DummyInvoiceService : IInvoiceService
    {
        public async Task<PaginatedResult<InvoiceListItem>> GetInvoices(string clientPrefix, int clientId, int page, int pageSize, string sortColumn, bool sortAsc)
        {
            if (clientId <= 0)
                throw new ArgumentException();

            var invoices = GetDummyInvoices(clientPrefix, clientId, page, pageSize, sortColumn, sortAsc);

            return await Task.FromResult(invoices);
        }

        public Task<byte[]> GetInvoiceFile(string clientPrefix, int clientId, int fileId)
        {
            var bytes = File.ReadAllBytes("sample.pdf");

            return Task.FromResult(bytes);
        }

        public async Task<string> TestSapConnection()
        {
            return await Task.FromResult("Successfull");
        }

        private PaginatedResult<InvoiceListItem> GetDummyInvoices(string clientPrefix, int clientId, int page, int pageSize, string sortColumn, bool sortAsc)
        {
            var invoices = Enumerable.Range(1, 50).Select(x => new InvoiceListItem(
                $"Prod {x}",
                $"{clientPrefix}{clientId:0000000000000}",
                DateTime.Today.AddDays(x),
                "ARS",
                100,
                $"invoice_{DateTime.Today.AddDays(x):yyyy-MM-dd}_{x}.pdf")
            ).ToList();

            var paginatedInvoices = invoices;

            invoices = GetInvoicesSorted(invoices, sortColumn, sortAsc);

            if ((page > 0) && (pageSize > 0))
            {
                paginatedInvoices = invoices.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return new PaginatedResult<InvoiceListItem> { Items = paginatedInvoices, TotalItems = invoices.Count };
        }

        private List<InvoiceListItem> GetInvoicesSorted(List<InvoiceListItem> invoices, string sortColumn, bool sortAsc)
        {
            var property = typeof(InvoiceListItem).GetProperty(sortColumn, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) ?? typeof(InvoiceListItem).GetProperty("AccountId");

            if (property != null)
            {
                return sortAsc ? invoices.OrderBy(x => property.GetValue(x, null)).ToList()
                    : invoices.OrderByDescending(x => property.GetValue(x, null)).ToList();
            }

            return invoices;
        }
    }
}
