using CAMS.Business.Services.Cache;
using CAMS.Core.IServices.Cache;

namespace CAMS.DependencyInjection
{
    public static class Container
    {
        public static IServiceCollection RegisterConfig(this IServiceCollection services, IConfiguration configuration)
        {

            services.RegisterConnection(configuration);

            services.RegisterServices();

            services.AddScoped<IUnitOfWorkAsync, UnitOfWorkAsync>();

            services.RegisterRepositories();

            services.AddAutoMapper(typeof(Mapping).Assembly);

            return services;
        }
        private static IServiceCollection RegisterConnection(this IServiceCollection services, IConfiguration configuration)
        {
            var connection = configuration["ConnectionStrings:DefaultConnectionString"];
            services.AddDbContext<AppDbContext>(x => x.UseSqlServer(connection, options =>
            {
                options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                options.CommandTimeout(60);

            }));

            services.AddScoped<AppDbContext, AppDbContext>();

            return services;
        }

        private static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped(typeof(IEmailBodyBuilder),typeof(EmailBodyBuilder));
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IFixedDepositService, FixedDepositService>();
            services.AddScoped<IJointAccountService, JointAccountService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<ILoanPricingService, LoanPricingService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ISavingsService, SavingsService>();
            services.AddScoped<ICurrentService, CurrentService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IAccountNumberGeneratorService, AccountNumberGeneratorService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IUserPermissionService, UserPermissionService>();

            return services;
        }

        private static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepositoryAsync<,>), typeof(GenericRepositoryAsync<,>));
            services.AddScoped(typeof(IClientRepositoryAsync), typeof(ClientRepositoryAsync));

            return services;
        }
    }
}
