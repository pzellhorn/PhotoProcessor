using Microsoft.EntityFrameworkCore;
using PhotoProcessor.Logic.Extensions;
using PhotoProcessor.State;
using PhotoProcessor.State.Extensions;
using pzellhorn.Core.Messaging.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddStateServices(builder.Configuration);
builder.Services.AddLogicServices();
builder.Services.AddDistributedQueueRabbitMq(builder.Configuration);

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
