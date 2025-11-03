using System.ComponentModel.DataAnnotations;
using DAL.Models.Enum;

namespace DAL.ValidationAttributes
{
    /// <summary>
    /// Custom validation attribute for DiscountValue based on DiscountType
    /// - PERCENT: Max 100%
    /// - FIXED_AMOUNT: Max 500,000 VND
    /// </summary>
    public class DiscountValueAttribute : ValidationAttribute
    {
        private const decimal MAX_PERCENT = 100m;
        private const decimal MAX_FIXED_AMOUNT = 500000m;

        public DiscountValueAttribute()
        {
            ErrorMessage = "Giá trị giảm giá không hợp lệ";
        }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (value == null)
            {
                return ValidationResult.Success; // Let [Required] handle null
            }

            var discountValue = (decimal)value;

            // Get DiscountType from the model
            var discountTypeProperty = validationContext
                .ObjectInstance.GetType()
                .GetProperty("DiscountType");

            if (discountTypeProperty == null)
            {
                return new ValidationResult("Không tìm thấy thuộc tính DiscountType");
            }

            var discountTypeValue = discountTypeProperty.GetValue(validationContext.ObjectInstance);

            // If DiscountType is null or invalid (enum binding failed), skip this validation
            // The [Required] attribute on DiscountType will handle that error
            if (
                discountTypeValue == null
                || !Enum.IsDefined(typeof(DiscountType), discountTypeValue)
            )
            {
                return ValidationResult.Success;
            }

            var discountType = (DiscountType)discountTypeValue;

            // Validate based on DiscountType
            if (discountType == DiscountType.PERCENT)
            {
                if (discountValue < 0 || discountValue > MAX_PERCENT)
                {
                    return new ValidationResult(
                        $"Giảm giá theo phần trăm phải từ 0% đến {MAX_PERCENT}%",
                        new[] { validationContext.MemberName }
                    );
                }
            }
            else if (discountType == DiscountType.FIXED_AMOUNT)
            {
                if (discountValue < 0 || discountValue > MAX_FIXED_AMOUNT)
                {
                    return new ValidationResult(
                        $"Giảm giá theo số tiền cố định phải từ 0 đến {MAX_FIXED_AMOUNT:N0} VND",
                        new[] { validationContext.MemberName }
                    );
                }
            }

            return ValidationResult.Success;
        }
    }
}
