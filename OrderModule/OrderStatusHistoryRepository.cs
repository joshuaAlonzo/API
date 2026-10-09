using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.OrderModule
{
    public class OrderStatusHistoryRepository : BaseRepository, IOrderStatusHistoryRepository
    {
        public OrderStatusHistoryRepository(MyCon dbConnection) : base(dbConnection) { }

        private OrderStatusHistory MapReaderToEntity(DbDataReader reader)
        {
            return new OrderStatusHistory
            {
                StatusHistoryId = ReadValue<int>(reader, "status_history_id", 0),
                OrderId = ReadValue<int?>(reader, "order_id", null),
                OrderIdSnapshot = ReadValue<int>(reader, "order_id_snapshot", 0),
                OldStatus = ReadValue<string?>(reader, "old_status", null),
                NewStatus = ReadValue<string>(reader, "new_status", string.Empty),
                ChangedByUserId = ReadValue<int?>(reader, "changed_by_user_id", null),
                ChangedAt = ReadValue<DateTime>(reader, "changed_at", DateTime.UtcNow),
                Notes = ReadValue<string?>(reader, "notes", null)
            };
        }

        public async Task<IEnumerable<OrderStatusHistory>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync("SELECT * FROM \"order_status_history\" ORDER BY \"status_history_id\" DESC", MapReaderToEntity);
        }

        public async Task<OrderStatusHistory?> GetByIdAsync(int id)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"order_status_history\" WHERE \"status_history_id\" = @id",
                MapReaderToEntity,
                new[] { CreateParameter("id", id) });
            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<OrderStatusHistory>> GetByOrderIdAsync(int orderId)
        {
            return await ExecuteReaderToListAsync(
                "SELECT * FROM \"order_status_history\" WHERE \"order_id\" = @order_id ORDER BY \"status_history_id\" DESC",
                MapReaderToEntity,
                new[] { CreateParameter("order_id", orderId) });
        }

        public async Task AddAsync(OrderStatusHistory entity)
        {
            const string sql = "INSERT INTO \"order_status_history\" (\"order_id\", \"order_id_snapshot\", \"old_status\", \"new_status\", \"changed_by_user_id\", \"notes\") VALUES (@order_id, @snapshot, @old, @new, @user_id, @notes)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("order_id", (object?)entity.OrderId ?? DBNull.Value),
                CreateParameter("snapshot", entity.OrderIdSnapshot),
                CreateParameter("old", (object?)entity.OldStatus ?? DBNull.Value),
                CreateParameter("new", entity.NewStatus),
                CreateParameter("user_id", (object?)entity.ChangedByUserId ?? DBNull.Value),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task UpdateAsync(OrderStatusHistory entity)
        {
            const string sql = "UPDATE \"order_status_history\" SET \"order_id\" = @order_id, \"order_id_snapshot\" = @snapshot, \"old_status\" = @old, \"new_status\" = @new, \"changed_by_user_id\" = @user_id, \"notes\" = @notes WHERE \"status_history_id\" = @id";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("id", entity.StatusHistoryId),
                CreateParameter("order_id", (object?)entity.OrderId ?? DBNull.Value),
                CreateParameter("snapshot", entity.OrderIdSnapshot),
                CreateParameter("old", (object?)entity.OldStatus ?? DBNull.Value),
                CreateParameter("new", entity.NewStatus),
                CreateParameter("user_id", (object?)entity.ChangedByUserId ?? DBNull.Value),
                CreateParameter("notes", (object?)entity.Notes ?? DBNull.Value)
            });
        }

        public async Task DeleteAsync(int id)
        {
            await ExecuteNonQueryAsync("DELETE FROM \"order_status_history\" WHERE \"status_history_id\" = @id", new[] { CreateParameter("id", id) });
        }
    }
}
