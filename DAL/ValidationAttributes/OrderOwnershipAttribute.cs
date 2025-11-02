using System.ComponentModel.DataAnnotations;
using DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace DAL.ValidationAttributes
{
    /// <summary>
    /// Custom validation attribute to verify that an OrderId belongs to a specific UserId
    /// Requires UserIdPropertyName to specify which property contains the userId
    /// </summary>
    public class OrderOwnershipAttribute : ValidationAttribute
    {
        public string UserIdPropertyName { get; set; } = "UserId";

        public OrderOwnershipAttribute()
        {
            ErrorMessage = "Bạn không có quyền thao tác đơn hàng này";
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

            // Try to parse OrderId as int
            int orderId;
            if (value is int)
            {
                orderId = (int)value;
            }
            else if (!int.TryParse(value.ToString(), out orderId) || orderId <= 0)
            {
                return ValidationResult.Success; // Invalid format
            }

            // Get AppDbContext from validation context service provider
            var dbContext = validationContext.GetService(typeof(AppDbContext)) as AppDbContext;
            if (dbContext == null)
            {
                // If DbContext is not available, skip validation
                return ValidationResult.Success;
            }

            // Get UserId from the DTO
            int? userId = null;
            if (!string.IsNullOrEmpty(UserIdPropertyName))
            {
                var userIdProperty = validationContext
                    .ObjectInstance.GetType()
                    .GetProperty(UserIdPropertyName);

                if (userIdProperty != null)
                {
                    var userIdValue = userIdProperty.GetValue(validationContext.ObjectInstance);
                    if (
                        userIdValue != null
                        && int.TryParse(userIdValue.ToString(), out int parsedUserId)
                    )
                    {
                        userId = parsedUserId;
                    }
                }
            }

            // If UserId is not provided, skip validation (optional check)
            if (!userId.HasValue || userId.Value <= 0)
            {
                return ValidationResult.Success;
            }

            // Check if order belongs to user (async operation - using blocking call)
            bool orderBelongsToUser = dbContext
                .Orders.AnyAsync(o => o.Id == orderId && o.UserId == userId.Value)
                .GetAwaiter()
                .GetResult();

            if (!orderBelongsToUser)
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }
    }
}
