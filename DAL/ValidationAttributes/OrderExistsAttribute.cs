using System.ComponentModel.DataAnnotations;
using DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace DAL.ValidationAttributes
{
    /// <summary>
    /// Custom validation attribute to verify that an OrderId exists in the database
    /// </summary>
    public class OrderExistsAttribute : ValidationAttribute
    {
        public OrderExistsAttribute()
        {
            ErrorMessage = "Đơn hàng không tồn tại";
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

            // Try to parse as int
            int orderId;
            if (value is int)
            {
                orderId = (int)value;
            }
            else if (!int.TryParse(value.ToString(), out orderId) || orderId <= 0)
            {
                return ValidationResult.Success; // Invalid format - let other validators handle
            }

            // Get AppDbContext from validation context service provider
            var dbContext = validationContext.GetService(typeof(AppDbContext)) as AppDbContext;
            if (dbContext == null)
            {
                // If DbContext is not available, skip validation
                return ValidationResult.Success;
            }

            // Check if order exists (async operation - using blocking call for validation attribute)
            bool orderExists = dbContext
                .Orders.AnyAsync(o => o.Id == orderId)
                .GetAwaiter()
                .GetResult();

            if (!orderExists)
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }
    }
}
