namespace Api
{
    public class OrderItemRepository : Api.OrderModule.OrderItemRepository, IOrderItemRepository
    {
        public OrderItemRepository(Api.Main.MyCon dbConnection) : base(dbConnection) { }
    }
}
