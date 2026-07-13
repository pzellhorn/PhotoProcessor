using Microsoft.EntityFrameworkCore;
using PhotoProcessor.API.BackgroundServices;
using PhotoProcessor.Logic.Extensions;
using PhotoProcessor.State;
using PhotoProcessor.State.Extensions;
using pzellhorn.Core.Messaging;
using pzellhorn.Core.Messaging.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//Don't cap request upload size (so we can send super large photos)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    options.MultipartBodyLengthLimit = long.MaxValue);

string[] corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddStateServices(builder.Configuration);
builder.Services.AddLogicServices();
builder.Services.AddDistributedQueueRabbitMq(builder.Configuration);

List<UploadEventOptions> uploadEvents = builder.Configuration.GetSection("UploadEvents").Get<List<UploadEventOptions>>() ?? throw new Exception("Can't find UploadEvents in config");

foreach (UploadEventOptions uploadEvent in uploadEvents)
{
    builder.Services.AddSingleton<IHostedService>(sp => new UploadEventConsumer(
        sp.GetRequiredService<IQueueConsumer>(),
        sp.GetRequiredService<IServiceScopeFactory>(),
        uploadEvent,
        sp.GetRequiredService<ILogger<UploadEventConsumer>>()));
}

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (KeyNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync(ex.Message);
    }
    catch (FileNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync(ex.Message);
    }
    catch (ArgumentException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync(ex.Message);
    }
});

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
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
