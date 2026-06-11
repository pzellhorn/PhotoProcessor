using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PhotoProcessor.State;

public class PhotoProcessorDbContextFactory : IDesignTimeDbContextFactory<PhotoProcessorDbContext>
{
    public PhotoProcessorDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string conn =
            config.GetConnectionString("Local")
            ?? throw new ArgumentException("Local connection string not found");

        DbContextOptions<PhotoProcessorDbContext> options = new DbContextOptionsBuilder<PhotoProcessorDbContext>()
            .UseNpgsql(conn, migrations => migrations.MigrationsAssembly("PhotoProcessor.State"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new PhotoProcessorDbContext(options);
    }
}
