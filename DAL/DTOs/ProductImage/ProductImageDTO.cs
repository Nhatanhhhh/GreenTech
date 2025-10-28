namespace DAL.DTOs.ProductImage
{
    public class ProductImageDTO
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public bool IsPrimary { get; set; }
    }
}
