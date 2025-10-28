using BLL.Service;
using BLL.Service.Interface;
using DAL.Repositories;
using DAL.Repositories.Interface;

namespace GreenTech.DI
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
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();

            return services;
        }

        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICouponTemplateService, CouponTemplateService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<IBannerService, BannerService>();
            services.AddScoped<IFileStorageService, CloudinaryStorageService>();

            return services;
        }
    }
}
