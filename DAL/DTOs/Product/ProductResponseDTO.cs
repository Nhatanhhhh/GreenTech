
using DAL.DTOs.ProductImage;

namespace DAL.DTOs.Product
{
    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellPrice { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; } // Main image
        public string CareInstructions { get; set; }
        public string PlantSize { get; set; }
        public decimal? Weight { get; set; }
        public string Dimensions { get; set; }
        public string Tags { get; set; }
        public int PointsEarned { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public List<ProductImageDTO> ProductImages { get; set; }
    }
}
