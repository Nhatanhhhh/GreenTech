using BLL.Service;
using BLL.Service.Interface;
using DAL.Repositories;
using DAL.Repositories.Interface;

namespace GreenTechMVC.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register all repositories
            services.AddRepositories();

            // Register all services
            services.AddBusinessServices();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ICouponTemplateRepository, CouponTemplateRepository>();
            // Add another repository at here
            // services.AddScoped<IUserRepository, UserRepository>();
            // services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }

        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICouponTemplateService, CouponTemplateService>();
            services.AddScoped<IFileStorageService, CloudinaryStorageService>();
            // Add another service at here
            // services.AddScoped<IUserService, UserService>();
            // services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}
