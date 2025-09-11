using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Morning.Value.Application
{
    public static class ConfigureService
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            return services;
        }
    }
}
