using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Supplier
{
    public class CreateSupplierDTO
    {
        [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên nhà cung cấp không được vượt quá 255 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mã nhà cung cấp là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Mã nhà cung cấp không được vượt quá 50 ký tự.")]
        public string Code { get; set; }

        [StringLength(100, ErrorMessage = "Tên người liên hệ không được vượt quá 100 ký tự.")]
        public string ContactPerson { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        public string Email { get; set; }

        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "Mã số thuế không được vượt quá 50 ký tự.")]
        public string TaxCode { get; set; }

        [StringLength(255, ErrorMessage = "Điều khoản thanh toán không được vượt quá 255 ký tự.")]
        public string PaymentTerms { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
