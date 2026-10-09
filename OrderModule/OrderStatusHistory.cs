using System;

namespace Api.OrderModule
{
    public class OrderStatusHistory : IEquatable<OrderStatusHistory>
    {
        public int StatusHistoryId { get; set; }
        public int? OrderId { get; set; }
        public int OrderIdSnapshot { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public int? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public OrderStatusHistory() { }

        public bool Equals(OrderStatusHistory? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return StatusHistoryId == other.StatusHistoryId;
        }

        public override bool Equals(object? obj) => Equals(obj as OrderStatusHistory);

        public override int GetHashCode() => StatusHistoryId.GetHashCode();
    }
}
