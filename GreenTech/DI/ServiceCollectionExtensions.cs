using BLL.Service.Auth;
using BLL.Service.Auth.Interface;
using BLL.Service.Banner;
using BLL.Service.Banner.Interface;
using BLL.Service.Blog;
using BLL.Service.Blog.Interface;
using BLL.Service.Cart;
using BLL.Service.Cart.Interface;
using BLL.Service.Category;
using BLL.Service.Category.Interface;
using BLL.Service.Cloudinary;
using BLL.Service.Cloudinary.Interface;
using BLL.Service.CouponTemplate;
using BLL.Service.CouponTemplate.Interface;
using BLL.Service.Email;
using BLL.Service.Email.Interface;
using BLL.Service.Notification;
using BLL.Service.Notification.Interface;
using BLL.Service.Order;
using BLL.Service.Order.Interface;
using BLL.Service.OTP;
using BLL.Service.OTP.Interface;
using BLL.Service.Payments;
using BLL.Service.Payments.Interface;
using BLL.Service.Point;
using BLL.Service.Point.Interface;
using BLL.Service.Product;
using BLL.Service.Product.Interface;
using BLL.Service.Review;
using BLL.Service.Review.Interface;
using BLL.Service.ReviewReply;
using BLL.Service.ReviewReply.Interface;
using BLL.Service.Supplier;
using BLL.Service.Supplier.Interface;
using BLL.Service.User;
using BLL.Service.User.Interface;
using BLL.Service.Wallet;
using BLL.Service.Wallet.Interface;
using DAL.Repositories.Auth;
using DAL.Repositories.Auth.Interface;
using DAL.Repositories.Banner;
using DAL.Repositories.Banner.Interface;
using DAL.Repositories.Blog;
using DAL.Repositories.Blog.Interface;
using DAL.Repositories.Cart;
using DAL.Repositories.Cart.Interface;
using DAL.Repositories.Category;
using DAL.Repositories.Category.Interface;
using DAL.Repositories.Coupon;
using DAL.Repositories.Coupon.Interface;
using DAL.Repositories.CouponTemplate;
using DAL.Repositories.CouponTemplate.Interface;
using DAL.Repositories.Notification;
using DAL.Repositories.Notification.Interface;
using DAL.Repositories.Order;
using DAL.Repositories.Order.Interface;
using DAL.Repositories.Point;
using DAL.Repositories.Point.Interface;
using DAL.Repositories.Product;
using DAL.Repositories.Product.Interface;
using DAL.Repositories.Review;
using DAL.Repositories.Review.Interface;
using DAL.Repositories.ReviewReply;
using DAL.Repositories.ReviewReply.Interface;
using DAL.Repositories.Supplier;
using DAL.Repositories.Supplier.Interface;
using DAL.Repositories.User;
using DAL.Repositories.User.Interface;
using DAL.Repositories.Wallet;
using DAL.Repositories.Wallet.Interface;

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
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<IBannerRepository, BannerRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IPointsRepository, PointsRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IReviewReplyRepository, ReviewReplyRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
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
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IFileStorageService, CloudinaryStorageService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IPointsService, PointsService>();
            services.AddScoped<IUserService, UserService>();

            // Email and OTP services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOTPService, OTPService>();

            // Payment processors & factory
            services.AddSingleton<IPaymentGatewayProcessor, MoMoPaymentProcessor>();
            services.AddSingleton<IPaymentGatewayProcessor, VnPayPaymentProcessor>();
            services.AddSingleton<IPaymentGatewayFactory, PaymentGatewayFactory>();

            // Review
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IReviewReplyService, ReviewReplyService>();

            // Order
            services.AddScoped<IOrderService, OrderService>();

            // Notification
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
