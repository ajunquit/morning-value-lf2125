using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Morning.Value.Application.Books.Services;
using Morning.Value.Application.Loans.Services;
using Morning.Value.Application.Users.Services;

namespace Morning.Value.Application
{
    public static class ConfigureService
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            RegisterServices(services);
            return services;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IBookAppService, BookAppService>();
            services.AddScoped<ILoanAppService, LoanAppService>();
            services.AddScoped<IAuthAppService, AuthAppService>();
        }
    }
}
