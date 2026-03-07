using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tools.DynamicsCRM.Configuration;

namespace Tools.DynamicsCRM
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDynamicsCRM(this IServiceCollection services, IConfiguration configuration)
        {
            DynamicsCrmConfig crmConfig = configuration.GetSection("DynamicsCRM").Get<DynamicsCrmConfig>();

            services.AddHttpClient("DynamicCrmAuthenticationHttpClient")
                    .AddTypedClient<DynamicsCrmAuthentication>(httpClient => new(httpClient, crmConfig));

            services.AddHttpClient("DynamicsCRMHttpClient")
                    .AddTypedClient<IDynamicsCrmClient>((httpClient, serviceProvider) =>
                    {
                        DynamicsCrmAuthentication auth = serviceProvider.GetRequiredService<DynamicsCrmAuthentication>();
                        return new DynamicsCrmClient(httpClient, auth, crmConfig);
                    });

            return services;
        }
    }
}