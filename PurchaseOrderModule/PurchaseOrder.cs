using System;

namespace Api.PurchaseOrderModule
{
    public class PurchaseOrder : IEquatable<PurchaseOrder>
    {
        public int PurchaseOrderId { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? OrderedAt { get; set; }
        public DateTime? ExpectedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public PurchaseOrder() { }

        public bool Equals(PurchaseOrder? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return PurchaseOrderId == other.PurchaseOrderId;
        }

        public override bool Equals(object? obj) => Equals(obj as PurchaseOrder);

        public override int GetHashCode() => PurchaseOrderId.GetHashCode();
    }
}
