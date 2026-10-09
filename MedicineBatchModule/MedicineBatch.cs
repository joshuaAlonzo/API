using System;

namespace Api.MedicineBatchModule
{
    public class MedicineBatch : IEquatable<MedicineBatch>
    {
        public int BatchId { get; set; }
        public int MedicineId { get; set; }
        public int? SupplierId { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int QuantityReceived { get; set; }
        public int QuantityRemaining { get; set; }
        public double UnitCost { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
        public string BatchStatus { get; set; } = "Active";
        public string? Notes { get; set; }

        public MedicineBatch() { }

        public bool Equals(MedicineBatch? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return BatchId == other.BatchId;
        }

        public override bool Equals(object? obj) => Equals(obj as MedicineBatch);

        public override int GetHashCode() => BatchId.GetHashCode();
    }
}
