using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.SuppliersModule
{
    public class SupplierRepository : BaseRepository, ISupplierRepository
    {
        public SupplierRepository(MyCon dbConnection) : base(dbConnection) { }

        private Supplier MapReader(DbDataReader reader)
        {
            return new Supplier
            {
                SupplierId     = ReadValue<int>(reader,    "SupplierId",     0),
                SupplierName   = ReadValue<string>(reader, "SupplierName",   string.Empty),
                ContactPerson  = ReadValue<string?>(reader,"ContactPerson",  null),
                ContactNumber  = ReadValue<string?>(reader,"ContactNumber",  null),
                Email          = ReadValue<string?>(reader,"Email",          null),
                Address        = ReadValue<string?>(reader,"Address",        null),
                Status         = ReadValue<string>(reader, "Status",         "Active"),
                CreatedAt      = ReadValue<DateTime>(reader,"CreatedAt",     DateTime.UtcNow),
                UpdatedAt      = ReadValue<DateTime>(reader,"UpdatedAt",     DateTime.UtcNow),
            };
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM suppliers ORDER BY SupplierName ASC;",
                MapReader);
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM suppliers WHERE SupplierId = @SupplierId;",
                MapReader,
                new[] { CreateParameter("SupplierId", id) });
            return results.FirstOrDefault();
        }

        public async Task<int> AddAsync(Supplier supplier)
        {
            var newId = await ExecuteScalarAsync<long>(
                @"INSERT INTO suppliers (SupplierName, ContactPerson, ContactNumber, Email, Address, Status, CreatedAt, UpdatedAt)
                  VALUES (@SupplierName, @ContactPerson, @ContactNumber, @Email, @Address, @Status, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
                  SELECT last_insert_rowid();",
                new[]
                {
                    CreateParameter("SupplierName",   supplier.SupplierName),
                    CreateParameter("ContactPerson",  (object?)supplier.ContactPerson  ?? DBNull.Value),
                    CreateParameter("ContactNumber",  (object?)supplier.ContactNumber  ?? DBNull.Value),
                    CreateParameter("Email",          (object?)supplier.Email          ?? DBNull.Value),
                    CreateParameter("Address",        (object?)supplier.Address        ?? DBNull.Value),
                    CreateParameter("Status",         supplier.Status),
                });
            return (int)newId;
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            await ExecuteNonQueryAsync(
                @"UPDATE suppliers 
                  SET SupplierName   = @SupplierName,
                      ContactPerson  = @ContactPerson,
                      ContactNumber  = @ContactNumber,
                      Email          = @Email,
                      Address        = @Address,
                      Status         = @Status,
                      UpdatedAt      = CURRENT_TIMESTAMP
                  WHERE SupplierId = @SupplierId;",
                new[]
                {
                    CreateParameter("SupplierId",     supplier.SupplierId),
                    CreateParameter("SupplierName",   supplier.SupplierName),
                    CreateParameter("ContactPerson",  (object?)supplier.ContactPerson  ?? DBNull.Value),
                    CreateParameter("ContactNumber",  (object?)supplier.ContactNumber  ?? DBNull.Value),
                    CreateParameter("Email",          (object?)supplier.Email          ?? DBNull.Value),
                    CreateParameter("Address",        (object?)supplier.Address        ?? DBNull.Value),
                    CreateParameter("Status",         supplier.Status),
                });
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteNonQueryAsync(
                "DELETE FROM suppliers WHERE SupplierId = @SupplierId;",
                new[] { CreateParameter("SupplierId", id) });
        }
    }
}
