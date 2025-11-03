using System.ComponentModel.DataAnnotations;
using DAL.Context;
using DAL.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace DAL.ValidationAttributes
{
    /// <summary>
    /// Custom validation attribute to check if email is unique in the database
    /// Supports excluding a specific userId (for update scenarios)
    /// </summary>
    public class UniqueEmailAttribute : ValidationAttribute
    {
        public string? ExcludeUserIdProperty { get; set; }

        public UniqueEmailAttribute()
        {
            ErrorMessage = "Email đã được sử dụng bởi tài khoản khác";
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

            string email = value.ToString()!;

            // Get AppDbContext directly from validation context service provider
            var dbContext = validationContext.GetService(typeof(AppDbContext)) as AppDbContext;
            if (dbContext == null)
            {
                // If DbContext is not available (e.g., model binding without service provider), skip validation
                return ValidationResult.Success;
            }

            // Get excludeUserId if specified
            int? excludeUserId = null;
            if (!string.IsNullOrEmpty(ExcludeUserIdProperty))
            {
                var excludeProperty = validationContext
                    .ObjectInstance.GetType()
                    .GetProperty(ExcludeUserIdProperty);

                if (excludeProperty != null)
                {
                    var excludeValue = excludeProperty.GetValue(validationContext.ObjectInstance);
                    if (
                        excludeValue != null
                        && int.TryParse(excludeValue.ToString(), out int userId)
                    )
                    {
                        excludeUserId = userId;
                    }
                }
            }

            // Check if email exists (async operation - using blocking call for validation attribute)
            var query = dbContext.Users.Where(u =>
                u.Email.ToLower() == email.ToLower() && u.Status == UserStatus.ACTIVE
            );

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            bool emailExists = query.AnyAsync().GetAwaiter().GetResult();

            if (emailExists)
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }
    }
}
