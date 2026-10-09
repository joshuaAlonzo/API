using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.PurchaseOrderModule
{
    public class PurchaseOrderItemRepository : BaseRepository, IPurchaseOrderItemRepository
    {
        public PurchaseOrderItemRepository(MyCon dbConnection) : base(dbConnection) { }

        private PurchaseOrderItem MapReaderToEntity(DbDataReader reader)
        {
            return new PurchaseOrderItem
            {
                PurchaseOrderItemId = ReadValue<int>(reader, "purchase_order_item_id", 0),
                PurchaseOrderId = ReadValue<int>(reader, "purchase_order_id", 0),
                MedicineId = ReadValue<int>(reader, "medicine_id", 0),
                QuantityOrdered = ReadValue<int>(reader, "quantity_ordered", 0),
                QuantityReceived = ReadValue<int>(reader, "quantity_received", 0),
                UnitCost = ReadValue<double>(reader, "unit_cost", 0.0),
                Notes = ReadValue<string?>(reader, "notes", null)
            };
        }

        public async Task<IEnumerable<PurchaseOrderItem>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync("SELECT * FROM \"purchase_order_items\" ORDER BY \"purchase_order_item_id\" DESC", MapReaderToEntity);
        }

        public async Task<PurchaseOrderItem?> GetByIdAsync(int id)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"purchase_order_items\" WHERE \"purchase_order_item_id\" = @id",
                MapReaderToEntity,
                new[] { CreateParameter("id", id) });
            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<PurchaseOrderItem>> GetByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"purchase_order_items\" WHERE \"purchase_order_id\" = @po_id ORDER BY \"purchase_order_item_id\" ASC",
                MapReaderToEntity,
                new[] { CreateParameter("po_id", purchaseOrderId) });
        }

        public async Task AddAsync(PurchaseOrderItem entity)
        {
            const string sql = "INSERT INTO \"purchase_order_items\" (\"purchase_order_id\", \"medicine_id\", \"quantity_ordered\", \"quantity_received\", \"unit_cost\", \"notes\") VALUES (@po_id, @med_id, @ordered, @received, @cost, @notes)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("po_id", entity.PurchaseOrderId),
                CreateParameter("med_id", entity.MedicineId),
                CreateParameter("ordered", entity.QuantityOrdered),
                CreateParameter("received", entity.QuantityReceived),
                CreateParameter("cost", entity.UnitCost),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task UpdateAsync(PurchaseOrderItem entity)
        {
            const string sql = "UPDATE \"purchase_order_items\" SET \"purchase_order_id\" = @po_id, \"medicine_id\" = @med_id, \"quantity_ordered\" = @ordered, \"quantity_received\" = @received, \"unit_cost\" = @cost, \"notes\" = @notes WHERE \"purchase_order_item_id\" = @id";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("id", entity.PurchaseOrderItemId),
                CreateParameter("po_id", entity.PurchaseOrderId),
                CreateParameter("med_id", entity.MedicineId),
                CreateParameter("ordered", entity.QuantityOrdered),
                CreateParameter("received", entity.QuantityReceived),
                CreateParameter("cost", entity.UnitCost),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteNonQueryAsync("DELETE FROM \"purchase_order_items\" WHERE \"purchase_order_item_id\" = @id", new[] { CreateParameter("id", id) });
        }
    }
}
