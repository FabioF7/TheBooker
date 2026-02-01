using TheBooker.Application;
using TheBooker.Infrastructure;
using TheBooker.Api.Endpoints;
using TheBooker.Api.Common;
using TheBooker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs
builder.WebHost.UseUrls("http://0.0.0.0:8001");

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "The Booker API", Version = "v1" });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations automatically in development
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Middleware pipeline
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

// Global exception handler
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
    });
});

// Map endpoints
app.MapHealthEndpoints();
app.MapAvailabilityEndpoints();
app.MapAppointmentEndpoints();
app.MapTenantEndpoints();

app.Run();
