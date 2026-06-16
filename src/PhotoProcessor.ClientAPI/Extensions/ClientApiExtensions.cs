using Microsoft.Extensions.DependencyInjection;
using pzellhorn.Core.ClientAPI.ServiceExtensions;

namespace PhotoProcessor.ClientAPI.Extensions
{
    public static class ClientApiExtensions
    { 
        public static IServiceCollection AddClientApiServices(this IServiceCollection services, Uri baseAddress)
        {
            services.AddClientApiBaseExtensions(baseAddress);

            services.AddScoped<MediaItemApi>();
            services.AddScoped<JobApi>();
            services.AddScoped<TagApi>();
            services.AddScoped<TagItemApi>();
            services.AddScoped<TagTypeApi>();

            return services;
        }
    }
}
