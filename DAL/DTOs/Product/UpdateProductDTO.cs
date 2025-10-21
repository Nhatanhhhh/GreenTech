using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Product
{
    public class UpdateProductDTO
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm không được dài quá 255 ký tự.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [StringLength(500)]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Nhà cung cấp là bắt buộc.")]
        public int SupplierId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá vốn phải là một số không âm.")]
        public decimal CostPrice { get; set; }

        [Required(ErrorMessage = "Giá bán là bắt buộc.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0.")]
        public decimal SellPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là một số không âm.")]
        public int Quantity { get; set; }

        public string CareInstructions { get; set; }

        [StringLength(100)]
        public string PlantSize { get; set; }

        public decimal? Weight { get; set; }

        [StringLength(100)]
        public string Dimensions { get; set; }

        [StringLength(500)]
        public string Tags { get; set; }

        public int PointsEarned { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
    }
}
