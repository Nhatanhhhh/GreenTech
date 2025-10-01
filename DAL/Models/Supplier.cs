using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    [Table("suppliers")]
    public class Supplier
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; }

        [Column("contact_person")]
        [MaxLength(100)]
        public string ContactPerson { get; set; }

        [Column("phone")]
        [MaxLength(15)]
        public string Phone { get; set; }

        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("tax_code")]
        [MaxLength(50)]
        public string TaxCode { get; set; }

        [Column("payment_terms")]
        [MaxLength(255)]
        public string PaymentTerms { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Product> Products { get; set; }
    }
}
