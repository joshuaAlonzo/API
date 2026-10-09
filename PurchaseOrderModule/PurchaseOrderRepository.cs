using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.PurchaseOrderModule
{
    public class PurchaseOrderRepository : BaseRepository, IPurchaseOrderRepository
    {
        public PurchaseOrderRepository(MyCon dbConnection) : base(dbConnection) { }

        private PurchaseOrder MapReaderToEntity(DbDataReader reader)
        {
            return new PurchaseOrder
            {
                PurchaseOrderId = ReadValue<int>(reader, "purchase_order_id", 0),
                PurchaseOrderNumber = ReadValue<string>(reader, "purchase_order_number", string.Empty),
                SupplierId = ReadValue<int>(reader, "supplier_id", 0),
                Status = ReadValue<string>(reader, "status", "Draft"),
                OrderedAt = ReadValue<DateTime?>(reader, "ordered_at", null),
                ExpectedAt = ReadValue<DateTime?>(reader, "expected_at", null),
                ReceivedAt = ReadValue<DateTime?>(reader, "received_at", null),
                CreatedByUserId = ReadValue<int?>(reader, "created_by_user_id", null),
                Notes = ReadValue<string?>(reader, "notes", null),
                CreatedAt = ReadValue<DateTime>(reader, "created_at", DateTime.UtcNow),
                UpdatedAt = ReadValue<DateTime>(reader, "updated_at", DateTime.UtcNow)
            };
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync("SELECT * FROM \"purchase_orders\" ORDER BY \"purchase_order_id\" DESC", MapReaderToEntity);
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int purchaseOrderId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"purchase_orders\" WHERE \"purchase_order_id\" = @id",
                MapReaderToEntity,
                new[] { CreateParameter("id", purchaseOrderId) });
            return results.FirstOrDefault();
        }

        public async Task<PurchaseOrder?> GetByNumberAsync(string purchaseOrderNumber)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"purchase_orders\" WHERE \"purchase_order_number\" = @number",
                MapReaderToEntity,
                new[] { CreateParameter("number", purchaseOrderNumber) });
            return results.FirstOrDefault();
        }

        public async Task AddAsync(PurchaseOrder entity)
        {
            const string sql = "INSERT INTO \"purchase_orders\" (\"purchase_order_number\", \"supplier_id\", \"status\", \"ordered_at\", \"expected_at\", \"received_at\", \"created_by_user_id\", \"notes\") VALUES (@number, @supplier, @status, @ordered, @expected, @received, @created_by, @notes)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("number", entity.PurchaseOrderNumber),
                CreateParameter("supplier", entity.SupplierId),
                CreateParameter("status", entity.Status),
                CreateParameter("ordered", (object?)entity.OrderedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value),
                CreateParameter("expected", (object?)entity.ExpectedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value),
                CreateParameter("received", (object?)entity.ReceivedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value),
                CreateParameter("created_by", (object?)entity.CreatedByUserId ?? DBNull.Value),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task UpdateAsync(PurchaseOrder entity)
        {
            const string sql = "UPDATE \"purchase_orders\" SET \"purchase_order_number\" = @number, \"supplier_id\" = @supplier, \"status\" = @status, \"ordered_at\" = @ordered, \"expected_at\" = @expected, \"received_at\" = @received, \"created_by_user_id\" = @created_by, \"notes\" = @notes, \"updated_at\" = CURRENT_TIMESTAMP WHERE \"purchase_order_id\" = @id";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("id", entity.PurchaseOrderId),
                CreateParameter("number", entity.PurchaseOrderNumber),
                CreateParameter("supplier", entity.SupplierId),
                CreateParameter("status", entity.Status),
                CreateParameter("ordered", (object?)entity.OrderedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value),
                CreateParameter("expected", (object?)entity.ExpectedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value),
                CreateParameter("received", (object?)entity.ReceivedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value),
                CreateParameter("created_by", (object?)entity.CreatedByUserId ?? DBNull.Value),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task DeleteAsync(int purchaseOrderId)
        {
            await ExecuteNonQueryAsync("DELETE FROM \"purchase_orders\" WHERE \"purchase_order_id\" = @id", new[] { CreateParameter("id", purchaseOrderId) });
        }
    }
}
