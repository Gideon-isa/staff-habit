using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StaffHabit.Api.Database;
using StaffHabit.Api.Extensions;
using StaffHabit.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails(conf =>
{
    conf.CustomizeProblemDetails = contex =>
    {
        contex.ProblemDetails.Extensions.TryAdd("requestId", contex.HttpContext.TraceIdentifier);
    };
 });

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Database"), 
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
    .UseSnakeCaseNamingConvention());


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(trace => trace
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddNpgsql())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation())
    .UseOtlpExporter();

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
});

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(8080); // HTTP port
//    options.ListenAnyIP(8081, listenOptions => // HTTPS port
//    {
//        listenOptions.UseHttps();
//    });
//});

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson()
.AddXmlSerializerFormatters();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync(); 
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
