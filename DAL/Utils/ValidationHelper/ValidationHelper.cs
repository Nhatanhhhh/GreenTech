using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Utils.ValidationHelper
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Performs DataAnnotations validation for any DTO object.
        /// Throws a ValidationException if one or more validation errors are found.
        /// </summary>
        /// <typeparam name="T">The type of the object being validated.</typeparam>
        /// <param name="model">The model instance to validate.</param>
        /// <param name="serviceProvider">Optional service provider for dependency injection in validation attributes.</param>
        /// <exception cref="ArgumentNullException">Thrown when the model is null.</exception>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        public static void ValidateModel<T>(T model, IServiceProvider? serviceProvider = null)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var context = new ValidationContext(
                model,
                serviceProvider: serviceProvider,
                items: null
            );
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, context, results, true))
            {
                var errors = string.Join("; ", results.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }
        }
    }
}
