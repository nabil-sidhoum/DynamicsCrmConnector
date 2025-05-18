using Tools.DynamicsCRM.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tools.DynamicsCRM
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDynamicsCRM(this IServiceCollection services, IConfiguration configuration)
        {
            var crmConfig = configuration.GetSection("DynamicsCRM").Get<DynamicsCrmConfig>();

            services.AddHttpClient("DynamicCrmAuthenticationHttpClient")
                    .AddTypedClient<DynamicsCrmAuthentication>(httpclient => new(httpclient, crmConfig));

            DynamicsCrmAuthentication dynamicCrmAuthentication = services.BuildServiceProvider().GetService<DynamicsCrmAuthentication>();

            services.AddHttpClient("DynamicsCRMHttpClient")
                    .AddTypedClient<IDynamicsCrmClient>(httpclient => new DynamicsCrmClient(httpclient, dynamicCrmAuthentication, crmConfig));

            return services;
        }
    }
}
