using System;

namespace Api.InventoryTransactionModule
{
    public class InventoryTransaction : IEquatable<InventoryTransaction>
    {
        public int TransactionId { get; set; }
        public int MedicineId { get; set; }
        public int? BatchId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public double? UnitCost { get; set; }
        public int? RelatedOrderId { get; set; }
        public int? PurchaseOrderItemId { get; set; }
        public int? PerformedByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public InventoryTransaction() { }

        public bool Equals(InventoryTransaction? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return TransactionId == other.TransactionId;
        }

        public override bool Equals(object? obj) => Equals(obj as InventoryTransaction);

        public override int GetHashCode() => TransactionId.GetHashCode();
    }
}
