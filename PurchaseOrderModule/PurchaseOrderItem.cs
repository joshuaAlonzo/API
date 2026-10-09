using System;

namespace Api.PurchaseOrderModule
{
    public class PurchaseOrderItem : IEquatable<PurchaseOrderItem>
    {
        public int PurchaseOrderItemId { get; set; }
        public int PurchaseOrderId { get; set; }
        public int MedicineId { get; set; }
        public int QuantityOrdered { get; set; }
        public int QuantityReceived { get; set; }
        public double UnitCost { get; set; }
        public string? Notes { get; set; }

        public PurchaseOrderItem() { }

        public bool Equals(PurchaseOrderItem? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return PurchaseOrderItemId == other.PurchaseOrderItemId;
        }

        public override bool Equals(object? obj) => Equals(obj as PurchaseOrderItem);

        public override int GetHashCode() => PurchaseOrderItemId.GetHashCode();
    }
}
