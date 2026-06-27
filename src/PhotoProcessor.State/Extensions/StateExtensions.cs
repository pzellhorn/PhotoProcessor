using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pzellhorn.Core.State.Base;
using pzellhorn.Core.State.Base.DBContext;
using pzellhorn.Core.State.Base.Interfaces;
using pzellhorn.Core.State.Storage;

namespace PhotoProcessor.State.Extensions
{
    public static class StateExtensions
    { 
        public static IServiceCollection AddStateServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PhotoProcessorDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Local"),
                    npgsql =>
                    {
                        npgsql.MigrationsAssembly("PhotoProcessor.State");
                        npgsql.UseVector();
                    })
                    .UseSnakeCaseNamingConvention());

            services.AddScoped<BaseDbContext>(ctx => ctx.GetRequiredService<PhotoProcessorDbContext>());
             
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // Filesystem media storage; root configurable via Storage:DiskRoot.
            services.AddDiskStorage(configuration["Storage:DiskRoot"] ?? @"G:\dev\photos");

            return services;
        }
    }
}
