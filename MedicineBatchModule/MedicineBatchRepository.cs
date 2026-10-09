using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.MedicineBatchModule
{
    public class MedicineBatchRepository : BaseRepository, IMedicineBatchRepository
    {
        public MedicineBatchRepository(MyCon dbConnection) : base(dbConnection) { }

        private MedicineBatch MapReaderToEntity(DbDataReader reader)
        {
            return new MedicineBatch
            {
                BatchId = ReadValue<int>(reader, "batch_id", 0),
                MedicineId = ReadValue<int>(reader, "medicine_id", 0),
                SupplierId = ReadValue<int?>(reader, "supplier_id", null),
                LotNumber = ReadValue<string>(reader, "lot_number", string.Empty),
                ExpirationDate = ReadValue<DateTime>(reader, "expiration_date", DateTime.UtcNow),
                QuantityReceived = ReadValue<int>(reader, "quantity_received", 0),
                QuantityRemaining = ReadValue<int>(reader, "quantity_remaining", 0),
                UnitCost = ReadValue<double>(reader, "unit_cost", 0.0),
                ReceivedAt = ReadValue<DateTime>(reader, "received_at", DateTime.UtcNow),
                BatchStatus = ReadValue<string>(reader, "batch_status", "Active"),
                Notes = ReadValue<string?>(reader, "notes", null)
            };
        }

        public async Task<IEnumerable<MedicineBatch>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync("SELECT * FROM \"medicine_batches\" ORDER BY \"batch_id\" DESC", MapReaderToEntity);
        }

        public async Task<MedicineBatch?> GetByIdAsync(int batchId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"medicine_batches\" WHERE \"batch_id\" = @batch_id",
                MapReaderToEntity,
                new[] { CreateParameter("batch_id", batchId) });
            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<MedicineBatch>> GetByMedicineIdAsync(int medicineId)
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"medicine_batches\" WHERE \"medicine_id\" = @medicine_id ORDER BY \"batch_id\" DESC",
                MapReaderToEntity,
                new[] { CreateParameter("medicine_id", medicineId) });
        }

        public async Task AddAsync(MedicineBatch entity)
        {
            const string sql = "INSERT INTO \"medicine_batches\" (\"medicine_id\", \"supplier_id\", \"lot_number\", \"expiration_date\", \"quantity_received\", \"quantity_remaining\", \"unit_cost\", \"batch_status\", \"notes\") VALUES (@medicine_id, @supplier_id, @lot_number, @expiration_date, @quantity_received, @quantity_remaining, @unit_cost, @batch_status, @notes)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("medicine_id", entity.MedicineId),
                CreateParameter("supplier_id", (object?)entity.SupplierId ?? DBNull.Value),
                CreateParameter("lot_number", entity.LotNumber),
                CreateParameter("expiration_date", entity.ExpirationDate.ToString("yyyy-MM-dd")),
                CreateParameter("quantity_received", entity.QuantityReceived),
                CreateParameter("quantity_remaining", entity.QuantityRemaining),
                CreateParameter("unit_cost", entity.UnitCost),
                CreateParameter("batch_status", entity.BatchStatus),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task UpdateAsync(MedicineBatch entity)
        {
            const string sql = "UPDATE \"medicine_batches\" SET \"medicine_id\" = @medicine_id, \"supplier_id\" = @supplier_id, \"lot_number\" = @lot_number, \"expiration_date\" = @expiration_date, \"quantity_received\" = @quantity_received, \"quantity_remaining\" = @quantity_remaining, \"unit_cost\" = @unit_cost, \"batch_status\" = @batch_status, \"notes\" = @notes WHERE \"batch_id\" = @batch_id";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("batch_id", entity.BatchId),
                CreateParameter("medicine_id", entity.MedicineId),
                CreateParameter("supplier_id", (object?)entity.SupplierId ?? DBNull.Value),
                CreateParameter("lot_number", entity.LotNumber),
                CreateParameter("expiration_date", entity.ExpirationDate.ToString("yyyy-MM-dd")),
                CreateParameter("quantity_received", entity.QuantityReceived),
                CreateParameter("quantity_remaining", entity.QuantityRemaining),
                CreateParameter("unit_cost", entity.UnitCost),
                CreateParameter("batch_status", entity.BatchStatus),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task DeleteAsync(int batchId)
        {
            await ExecuteNonQueryAsync("DELETE FROM \"medicine_batches\" WHERE \"batch_id\" = @batch_id", new[] { CreateParameter("batch_id", batchId) });
        }
    }
}
