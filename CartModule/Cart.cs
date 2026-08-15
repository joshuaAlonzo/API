using System;

namespace Api.CartModule
{
    public class Cart : IEquatable<Cart>
    {
        public int      CartId      { get; set; }
        public int      UserId      { get; set; }
        public int      MedicineId  { get; set; }
        public int      Quantity    { get; set; }
        public double   Subtotal    { get; set; }
        public DateTime AddedAt     { get; set; }

        public Cart() { }

        public Cart(int cartId, int userId, int medicineId, int quantity, double subtotal, DateTime addedAt)
        {
            CartId     = cartId;
            UserId     = userId;
            MedicineId = medicineId;
            Quantity   = quantity;
            Subtotal   = subtotal;
            AddedAt    = addedAt;
        }

        public bool Equals(Cart? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (CartId > 0 || other.CartId > 0) return CartId == other.CartId;
            return string.Equals(ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as Cart);

        public override int GetHashCode()
        {
            if (CartId > 0) return CartId.GetHashCode();
            return StringComparer.OrdinalIgnoreCase.GetHashCode(ToString());
        }

        public override string ToString() =>
            $"Cart[CartId={CartId}, UserId={UserId}, MedicineId={MedicineId}, Qty={Quantity}, Subtotal={Subtotal}]";
    }
}
