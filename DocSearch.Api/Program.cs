using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//SeriLog Settings+Coreletion Id
builder.Services.AddHttpContextAccessor();
builder.Services.AddHeaderPropagation(options => options.Headers.Add("X-Correlation-ID"));

builder.Host.UseSerilog((context, config) => config
    .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] [CorrId: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationIdHeader("X-Correlation-ID"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//Adding swagger service
builder.Services.AddSwaggerGen();
//Adding health check
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHeaderPropagation();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();
app.Run();