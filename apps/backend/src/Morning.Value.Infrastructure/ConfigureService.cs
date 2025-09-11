using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure
{
    public static class ConfigureService
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            RegisterDbContexts(services, configuration);
            return services;
        }

        private static void RegisterDbContexts(IServiceCollection services, IConfiguration configuration)
        {
            RegisterAppDbContext(services, configuration);
        }

        private static void RegisterAppDbContext(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IAppDbContext, AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("Local"),
                    sqlOptions =>
                        sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                )
            );
        }
    }
}
