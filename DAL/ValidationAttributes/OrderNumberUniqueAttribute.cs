using System.ComponentModel.DataAnnotations;
using DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace DAL.ValidationAttributes
{
    /// <summary>
    /// Custom validation attribute to check if order number is unique in the database
    /// Useful for validating manually entered order numbers
    /// </summary>
    public class OrderNumberUniqueAttribute : ValidationAttribute
    {
        public string? ExcludeOrderIdProperty { get; set; }

        public OrderNumberUniqueAttribute()
        {
            ErrorMessage = "Mã đơn hàng đã tồn tại trong hệ thống";
        }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Let [Required] handle null/empty
            }

            string orderNumber = value.ToString()!;

            // Get AppDbContext from validation context service provider
            var dbContext = validationContext.GetService(typeof(AppDbContext)) as AppDbContext;
            if (dbContext == null)
            {
                // If DbContext is not available, skip validation
                return ValidationResult.Success;
            }

            // Get excludeOrderId if specified (for update scenarios)
            int? excludeOrderId = null;
            if (!string.IsNullOrEmpty(ExcludeOrderIdProperty))
            {
                var excludeProperty = validationContext
                    .ObjectInstance.GetType()
                    .GetProperty(ExcludeOrderIdProperty);

                if (excludeProperty != null)
                {
                    var excludeValue = excludeProperty.GetValue(validationContext.ObjectInstance);
                    if (
                        excludeValue != null
                        && int.TryParse(excludeValue.ToString(), out int orderId)
                    )
                    {
                        excludeOrderId = orderId;
                    }
                }
            }

            // Check if order number exists
            var query = dbContext.Orders.Where(o => o.OrderNumber == orderNumber);

            if (excludeOrderId.HasValue)
            {
                query = query.Where(o => o.Id != excludeOrderId.Value);
            }

            bool orderNumberExists = query.AnyAsync().GetAwaiter().GetResult();

            if (orderNumberExists)
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }
    }
}
