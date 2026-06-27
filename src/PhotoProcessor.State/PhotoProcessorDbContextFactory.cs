using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pgvector.EntityFrameworkCore;

namespace PhotoProcessor.State;

public class PhotoProcessorDbContextFactory : IDesignTimeDbContextFactory<PhotoProcessorDbContext>
{
    public PhotoProcessorDbContext CreateDbContext(string[] args)
    {
        string solutionRoot = Directory.GetCurrentDirectory();
        string apiPath = Path.Combine(solutionRoot, "src", "PhotoProcessor");
        string basePath = Directory.Exists(apiPath) ? apiPath : solutionRoot;

        IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string conn = config.GetConnectionString("Local") ?? throw new ArgumentException("Local connection string not found");

        DbContextOptions<PhotoProcessorDbContext> options = new DbContextOptionsBuilder<PhotoProcessorDbContext>()
            .UseNpgsql(conn, npgsql =>
            {
                npgsql.MigrationsAssembly("PhotoProcessor.State");
                npgsql.UseVector();
            })
            .UseSnakeCaseNamingConvention()
            .Options;

        return new PhotoProcessorDbContext(options);
    }
}
