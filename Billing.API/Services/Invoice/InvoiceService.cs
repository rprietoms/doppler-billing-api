using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Billing.API.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sap.Data.Hana;

namespace Billing.API.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;
        private readonly IOptions<InvoiceProviderOptions> _options;

        public InvoiceService(ILogger<InvoiceService> logger, IOptions<InvoiceProviderOptions> options)
        {
            _logger  = logger;
            _options = options;
        }

        public async Task<PaginatedResult<InvoiceListItem>> GetInvoices(string clientPrefix, int clientId, int page, int pageSize, string sortColumn, bool sortAsc)
        {
            var dt = await GetInvoiceRecords(clientPrefix, clientId);

            var invoices = dt.Select().Select(dr => new InvoiceListItem(
                clientPrefix,
                clientId.ToString(),
                dr.Field<DateTime>("CreateDate").ToDateTimeOffSet(),
                dr.Field<DateTime>("DocDueDate").ToDateTimeOffSet(),
                dr.Field<DateTime>("SendDate").ToDateTimeOffSet(),
                dr.Field<string>("DocCur"),
                dr.Field<decimal>("DocTotal").ToDouble(),
                $"invoice_{dr.Field<DateTime>("SendDate"):yyyy-MM-dd}_{dr.Field<int>("AbsEntry")}.{dr.Field<string>("FileExt")}",
                dr.Field<int>("AbsEntry")))
                .AsQueryable();

            //TODO: When we can get the data from database we will try to move the pagination into the sql query
            var invoiceSorted = GetInvoicesSorted(invoices, sortColumn, sortAsc).ToList();
            var paginatedInvoices = invoiceSorted;

            if ((page > 0) && (pageSize > 0))
            {
                paginatedInvoices = invoiceSorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return new PaginatedResult<InvoiceListItem> { Items = paginatedInvoices, TotalItems = invoiceSorted.Count };
        }

        public async Task<byte[]> GetInvoiceFile(string clientPrefix, int clientId, int fileId)
        {
            var dt = await GetInvoiceRecords(clientPrefix, clientId, fileId);

            var dr = dt.Select().FirstOrDefault();

            if (dr == null)
                return null;

            var link = $"{dr.Field<string>("trgtPath")}\\{dr.Field<string>("FileName")}.{dr.Field<string>("FileExt")}";

            return await File.ReadAllBytesAsync(link);
        }

        public async Task<string> TestSapConnection()
        {
            try
            {
                using (var conn = new HanaConnection(_options.Value.DbConnectionString))
                {

                    conn.Open();

                    var schema = _options.Value.Schema;

                    var query = $"select * from {schema}.oeml";

                    var da = new HanaDataAdapter(query, conn);

                    var dt = new DataTable("Invoices");

                    da.Fill(dt);

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Hana connection");

                return ex.Message;
            }

            return "Successfull";
        }

        private static IEnumerable<InvoiceListItem> GetInvoicesSorted(IQueryable<InvoiceListItem> invoices, string sortColumn, bool sortAsc)
        {
            return invoices.OrderBy(sortColumn + (!sortAsc ? " descending" : ""));
        }

        private async Task<DataTable> GetInvoiceRecords(string clientPrefix, int clientId, int? fileId = null)
        {
            var schema = _options.Value.Schema;

            using (var conn = new HanaConnection(_options.Value.DbConnectionString))
            {
                conn.Open();

                var query = string.Empty;

                query += $" SELECT";
                query += $"     AT1.\"AbsEntry\" ,";
                query += $"     OEM.\"CardCode\" ,";
                query += $"     OEM.\"CardName\" ,";
                query += $"     OEM.\"SendDate\" ,";
                query += $"     OEM.\"SendTime\" ,";
                query += $"     cast(OEM.\"DocEntry\" AS NVARCHAR) AS \"DocEntry\" ,";
                query += $"     OI.\"DocTotal\" ,";
                query += $"     OI.\"PaidToDate\" ,";
                query += "      OI.\"CreateDate\" ,";
                query += "      OI.\"DocDueDate\" ,";
                query += $"     cast(OI.\"DocCur\" AS NVARCHAR)   AS \"DocCur\" ,";
                query += $"     cast(AT1.\"trgtPath\" AS NVARCHAR) AS \"trgtPath\" ,";
                query += $"     cast(AT1.\"FileName\" AS NVARCHAR) AS \"FileName\" ,";
                query += $"     cast(AT1.\"FileExt\" AS NVARCHAR)  AS \"FileExt\" ";
                query += $" FROM";
                query += $"     {schema}.OINV OI ";
                query += $"     INNER JOIN ( SELECT";
                query += $"         T0.\"DocEntry\" ,";
                query += $"         min(T0.\"AtcEntry\") AS \"AtcEntry\" ,";
                query += $"         min(T0.\"AbsEntry\") AS \"AbsEntry\" ";
                query += $"         FROM {schema}.oeml T0 ";
                query += $"         WHERE T0.\"ObjType\" = '13' ";
                query += $" AND (T0.\"CardCode\" = '{clientPrefix}{clientId:0000000000000}' OR T0.\"CardCode\" LIKE '{clientPrefix}{clientId:00000000000}.%') ";
                query += $"         GROUP BY T0.\"DocEntry\" ) x ON x.\"DocEntry\" = OI.\"DocEntry\" ";
                query += $"     INNER JOIN {schema}.ATC1 AT1 ON x.\"AtcEntry\" = AT1.\"AbsEntry\" ";
                query += $"     INNER JOIN {schema}.oeml OEM ON OEM.\"AbsEntry\" = x.\"AbsEntry\" ";
                query += $" WHERE";
                query += $"     OI.\"ObjType\" = '13'";
                query += $" AND (OI.\"CardCode\" = '{clientPrefix}{clientId:0000000000000}' OR OI.\"CardCode\" LIKE '{clientPrefix}{clientId:00000000000}.%')";

                if (fileId.HasValue)
                    query += $" AND (AT1.\"AbsEntry\" = '{fileId}')";

                var da = new HanaDataAdapter(query, conn);

                var dt = new DataTable("Invoices");

                da.Fill(dt);

                conn.Close();

                return dt;
            }
        }
    }
}
