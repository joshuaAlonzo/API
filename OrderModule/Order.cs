using System;
using System.Collections.Generic;

namespace Api.OrderModule
{
    public class Order : IEquatable<Order>
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public double TotalAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? DeliveryAddress { get; set; }
        public string OrderStatus { get; set; } = "Pending";

        /// <summary>Navigation property for items included in this order.</summary>
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public Order() { }

        public Order(int orderId, int userId, DateTime orderDate, double totalAmount, string? paymentMethod, string? deliveryAddress, string orderStatus)
        {
            OrderId = orderId;
            UserId = userId;
            OrderDate = orderDate;
            TotalAmount = totalAmount;
            PaymentMethod = paymentMethod;
            DeliveryAddress = deliveryAddress;
            OrderStatus = orderStatus;
        }

        public bool Equals(Order? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (OrderId > 0 || other.OrderId > 0) return OrderId == other.OrderId;
            return UserId == other.UserId && OrderDate == other.OrderDate;
        }

        public override bool Equals(object? obj) => Equals(obj as Order);

        public override int GetHashCode()
        {
            if (OrderId > 0) return OrderId.GetHashCode();
            return HashCode.Combine(UserId, OrderDate);
        }

        public override string ToString() =>
            $"Order[OrderId={OrderId}, UserId={UserId}, Total={TotalAmount}, Status={OrderStatus}, Date={OrderDate}]";
    }
}
