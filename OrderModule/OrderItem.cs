using System;

namespace Api.OrderModule
{
    public class OrderItem : IEquatable<OrderItem>
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Subtotal { get; set; }

        public OrderItem() { }

        public OrderItem(int orderItemId, int orderId, int medicineId, int quantity, double unitPrice, double subtotal)
        {
            OrderItemId = orderItemId;
            OrderId = orderId;
            MedicineId = medicineId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Subtotal = subtotal;
        }

        public bool Equals(OrderItem? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (OrderItemId > 0 || other.OrderItemId > 0) return OrderItemId == other.OrderItemId;
            return OrderId == other.OrderId && MedicineId == other.MedicineId;
        }

        public override bool Equals(object? obj) => Equals(obj as OrderItem);

        public override int GetHashCode()
        {
            if (OrderItemId > 0) return OrderItemId.GetHashCode();
            return HashCode.Combine(OrderId, MedicineId);
        }

        public override string ToString() =>
            $"OrderItem[OrderItemId={OrderItemId}, OrderId={OrderId}, MedicineId={MedicineId}, Qty={Quantity}, UnitPrice={UnitPrice}, Subtotal={Subtotal}]";
    }
}
