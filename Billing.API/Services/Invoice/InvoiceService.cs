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

        private async Task<DataTable> GetInvoiceRecords(string clientPrefix, int? clientId, int? fileId = null)
        {
            var schema = _options.Value.Schema;

            using (var conn = new HanaConnection(_options.Value.DbConnectionString))
            {
                conn.Open();

                var query = string.Empty;

                query += $" SELECT DISTINCT";
                query += $"     T0.\"AbsEntry\" ,";
                query += $"     T0.\"CardCode\" ,";
                query += $"     T0.\"CardName\" ,";
                query += $"     T0.\"SendDate\" ,";
                query += $"     T0.\"SendTime\" ,";
                query += $"     cast(T0.\"DocEntry\" AS NVARCHAR) AS \"DocEntry\" ,";
                query += $"     T2.\"DocTotal\" ,";
                query += $"     T2.\"PaidToDate\" ,";
                query += $"     cast(T2.\"DocCur\" AS NVARCHAR)   AS \"DocCur\" ,";
                query += $"     cast(T1.\"trgtPath\" AS NVARCHAR) AS \"trgtPath\" ,";
                query += $"     cast(T1.\"FileName\" AS NVARCHAR) AS \"FileName\" ,";
                query += $"     cast(T1.\"FileExt\" AS NVARCHAR)  AS \"FileExt\" ";
                query += $" FROM";
                query += $"     {schema}.oeml T0 ";
                query += $"     INNER JOIN {schema}.ATC1 T1 ON T0.\"AtcEntry\" = T1.\"AbsEntry\" ";
                query += $"     INNER JOIN {schema}.OINV T2 ON T0.\"DocNum\" = T2.\"DocNum\" ";
                query += $"     INNER JOIN ( SELECT";
                query += $"         T0.\"DocEntry\" ,";
                query += $"         min(T0.\"SendTime\") AS \"SendTime\" ";
                query += $"         FROM {schema}.oeml T0 ";
                query += $"         WHERE T0.\"ObjType\" = '13' ";
                query += $"         GROUP BY T0.\"DocEntry\" ) x ON x.\"DocEntry\" = T0.\"DocEntry\" AND x.\"SendTime\" = T0.\"SendTime\"";
                query += $" WHERE";
                query += $"     T0.\"ObjType\" = '13'";

                if (clientPrefix.IsNotNullOrEmpty() && clientId.HasValue)
                    query += $" AND (T0.\"CardCode\" = '{clientPrefix}{clientId:0000000000000}' OR T0.\"CardCode\" LIKE '{clientPrefix}{clientId:00000000000}.%')";

                if (fileId.HasValue)
                    query += $" AND (T0.\"AbsEntry\" = '{fileId}')";

                var da = new HanaDataAdapter(query, conn);

                var dt = new DataTable("Invoices");

                da.Fill(dt);

                conn.Close();

                return dt;
            }
        }
    }
}
