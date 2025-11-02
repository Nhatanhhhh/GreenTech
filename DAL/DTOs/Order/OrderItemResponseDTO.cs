namespace DAL.DTOs.Order
{
    public class OrderItemResponseDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCostPrice { get; set; }
        public decimal UnitSellPrice { get; set; }
        public decimal Total { get; set; }
        public int PointsPerItem { get; set; }
    }
}
