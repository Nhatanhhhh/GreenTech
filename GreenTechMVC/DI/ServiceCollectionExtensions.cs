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
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBlogRepository, BlogRepository>();
            return services;
        }

        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICouponTemplateService, CouponTemplateService>();
            services.AddScoped<IFileStorageService, CloudinaryStorageService>();
            services.AddScoped<IBannerService, BannerService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBlogService, BlogService>();
            return services;
        }
    }
}
