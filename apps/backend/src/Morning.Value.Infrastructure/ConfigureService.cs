using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Morning.Value.Application.Common.Services;
using Morning.Value.Domain.Book.Interfaces;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Domain.Loans.Interfaces;
using Morning.Value.Domain.Users.Interfaces;
using Morning.Value.Infrastructure.Persistences.Contexts;
using Morning.Value.Infrastructure.Persistences.Interceptors;
using Morning.Value.Infrastructure.Repositories;
using Morning.Value.Infrastructure.Security;

namespace Morning.Value.Infrastructure
{
    public static class ConfigureService
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            RegisterDbContexts(services, configuration);
            RegisterRepositories(services);
            RegisterSecurities(services);
            return services;
        }

        private static void RegisterSecurities(IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();
        }

        private static void RegisterDbContexts(IServiceCollection services, IConfiguration configuration)
        {
            RegisterAppDbContext(services, configuration);
        }

        private static void RegisterAppDbContext(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<AuditingSaveChangesInterceptor>();
            services.AddDbContext<IAppDbContext, AppDbContext>((sp, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("Local"),
                    sqlOptions =>
                        sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                );

                options.AddInterceptors(sp.GetRequiredService<AuditingSaveChangesInterceptor>());
            }
                
            );
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWorkAsync, UnitOfWorkAsync>();
            services.AddScoped<IBookRepositoryAsync, BookRepositoryAsync>();
            services.AddScoped<ILoanRepositoryAsync, LoanRepositoryAsync>();
            services.AddScoped<IUserRepositoryAsync, UserRepositoryAsync>();
        }
    }
}
