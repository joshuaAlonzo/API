using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.InventoryTransactionModule
{
    public class InventoryTransactionRepository : BaseRepository, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(MyCon dbConnection) : base(dbConnection) { }

        private InventoryTransaction MapReaderToEntity(DbDataReader reader)
        {
            return new InventoryTransaction
            {
                TransactionId = ReadValue<int>(reader, "transaction_id", 0),
                MedicineId = ReadValue<int>(reader, "medicine_id", 0),
                BatchId = ReadValue<int?>(reader, "batch_id", null),
                TransactionType = ReadValue<string>(reader, "transaction_type", string.Empty),
                QuantityChange = ReadValue<int>(reader, "quantity_change", 0),
                UnitCost = ReadValue<double?>(reader, "unit_cost", null),
                RelatedOrderId = ReadValue<int?>(reader, "related_order_id", null),
                PurchaseOrderItemId = ReadValue<int?>(reader, "purchase_order_item_id", null),
                PerformedByUserId = ReadValue<int?>(reader, "performed_by_user_id", null),
                Reason = ReadValue<string>(reader, "reason", string.Empty),
                Notes = ReadValue<string?>(reader, "notes", null),
                CreatedAt = ReadValue<DateTime>(reader, "created_at", DateTime.UtcNow)
            };
        }

        public async Task<IEnumerable<InventoryTransaction>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync("SELECT * FROM \"inventory_transactions\" ORDER BY \"transaction_id\" DESC", MapReaderToEntity);
        }

        public async Task<InventoryTransaction?> GetByIdAsync(int id)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"inventory_transactions\" WHERE \"transaction_id\" = @id",
                MapReaderToEntity,
                new[] { CreateParameter("id", id) });
            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetByMedicineIdAsync(int medicineId)
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"inventory_transactions\" WHERE \"medicine_id\" = @med_id ORDER BY \"transaction_id\" DESC",
                MapReaderToEntity,
                new[] { CreateParameter("med_id", medicineId) });
        }

        public async Task AddAsync(InventoryTransaction entity)
        {
            const string sql = "INSERT INTO \"inventory_transactions\" (\"medicine_id\", \"batch_id\", \"transaction_type\", \"quantity_change\", \"unit_cost\", \"related_order_id\", \"purchase_order_item_id\", \"performed_by_user_id\", \"reason\", \"notes\") VALUES (@med_id, @batch_id, @type, @change, @cost, @order_id, @po_item_id, @user_id, @reason, @notes)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("med_id", entity.MedicineId),
                CreateParameter("batch_id", (object?)entity.BatchId ?? DBNull.Value),
                CreateParameter("type", entity.TransactionType),
                CreateParameter("change", entity.QuantityChange),
                CreateParameter("cost", (object?)entity.UnitCost ?? DBNull.Value),
                CreateParameter("order_id", (object?)entity.RelatedOrderId ?? DBNull.Value),
                CreateParameter("po_item_id", (object?)entity.PurchaseOrderItemId ?? DBNull.Value),
                CreateParameter("user_id", (object?)entity.PerformedByUserId ?? DBNull.Value),
                CreateParameter("reason", entity.Reason),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task UpdateAsync(InventoryTransaction entity)
        {
            const string sql = "UPDATE \"inventory_transactions\" SET \"medicine_id\" = @med_id, \"batch_id\" = @batch_id, \"transaction_type\" = @type, \"quantity_change\" = @change, \"unit_cost\" = @cost, \"related_order_id\" = @order_id, \"purchase_order_item_id\" = @po_item_id, \"performed_by_user_id\" = @user_id, \"reason\" = @reason, \"notes\" = @notes WHERE \"transaction_id\" = @id";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("id", entity.TransactionId),
                CreateParameter("med_id", entity.MedicineId),
                CreateParameter("batch_id", (object?)entity.BatchId ?? DBNull.Value),
                CreateParameter("type", entity.TransactionType),
                CreateParameter("change", entity.QuantityChange),
                CreateParameter("cost", (object?)entity.UnitCost ?? DBNull.Value),
                CreateParameter("order_id", (object?)entity.RelatedOrderId ?? DBNull.Value),
                CreateParameter("po_item_id", (object?)entity.PurchaseOrderItemId ?? DBNull.Value),
                CreateParameter("user_id", (object?)entity.PerformedByUserId ?? DBNull.Value),
                CreateParameter("reason", entity.Reason),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteNonQueryAsync("DELETE FROM \"inventory_transactions\" WHERE \"transaction_id\" = @id", new[] { CreateParameter("id", id) });
        }
    }
}
