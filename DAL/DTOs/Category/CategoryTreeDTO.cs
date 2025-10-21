
namespace DAL.DTOs.Category
{
    public class CategoryTreeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public int ProductsCount { get; set; }
        public List<CategoryTreeDTO> SubCategories { get; set; } = new();
    }
}
