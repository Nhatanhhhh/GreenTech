using System.ComponentModel.DataAnnotations;
using DAL.Context;
using DAL.Models.Enum;
using DAL.Utils.CryptoUtil;
using Microsoft.EntityFrameworkCore;

namespace DAL.ValidationAttributes
{
    /// <summary>
    /// Custom validation attribute to verify current password matches user's password in database
    /// Requires UserId property in the DTO to identify which user to verify
    /// </summary>
    public class VerifyCurrentPasswordAttribute : ValidationAttribute
    {
        public string? UserIdPropertyName { get; set; } = "UserId";

        public VerifyCurrentPasswordAttribute()
        {
            ErrorMessage = "Mật khẩu hiện tại không chính xác";
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

            string currentPassword = value.ToString()!;

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

            if (!userId.HasValue || userId.Value <= 0)
            {
                // If UserId is not available, skip validation (will be validated in service layer)
                return ValidationResult.Success;
            }

            // Get user from database
            var user = dbContext
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId.Value && u.Status == UserStatus.ACTIVE)
                .GetAwaiter()
                .GetResult();

            if (user == null)
            {
                return ValidationResult.Success; // User not found - let service layer handle this
            }

            // Verify password
            bool isValidPassword = CryptoUtil.VerifyPasswordHmacSHA512(
                user.Password,
                currentPassword
            );

            if (!isValidPassword)
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
            }

            return ValidationResult.Success;
        }
    }
}
