using Microsoft.EntityFrameworkCore;
using PhotoProcessor.State;
using pzellhorn.Core.State.Base.DBContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PhotoProcessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Local"),
        migrations => migrations.MigrationsAssembly("PhotoProcessor.State"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<BaseDbContext>(ctx => ctx.GetRequiredService<PhotoProcessorDbContext>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (args.Contains("migrate") || Environment.GetEnvironmentVariable("RUN_MIGRATIONS") == "true")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PhotoProcessorDbContext>();
    await db.Database.MigrateAsync();
    return;
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
