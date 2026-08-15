using System;

namespace Api.MedicineModule
{
    public class Medicine : IEquatable<Medicine>
    {
        public int MedicineId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string? Dosage { get; set; }
        public string? Manufacturer { get; set; }
        public double Price { get; set; }
        public int StockQty { get; set; }
        public string? Image { get; set; }
        public string Status { get; set; } = "Available";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Medicine() { }

        public Medicine(
            int medicineId,
            string brandName,
            string genericName,
            int categoryId,
            string? description,
            string? dosage,
            string? manufacturer,
            double price,
            int stockQty,
            string? image,
            string status,
            DateTime createdAt,
            DateTime updatedAt)
        {
            MedicineId = medicineId;
            BrandName = brandName;
            GenericName = genericName;
            CategoryId = categoryId;
            Description = description;
            Dosage = dosage;
            Manufacturer = manufacturer;
            Price = price;
            StockQty = stockQty;
            Image = image;
            Status = status;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public bool Equals(Medicine? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (MedicineId > 0 || other.MedicineId > 0) return MedicineId == other.MedicineId;
            return string.Equals(BrandName, other.BrandName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(GenericName, other.GenericName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Medicine);

        public override int GetHashCode()
        {
            if (MedicineId > 0) return MedicineId.GetHashCode();
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(BrandName),
                StringComparer.OrdinalIgnoreCase.GetHashCode(GenericName));
        }

        public override string ToString() =>
            $"Medicine[MedicineId={MedicineId}, BrandName={BrandName}, GenericName={GenericName}, Price={Price}, Stock={StockQty}]";
    }
}
