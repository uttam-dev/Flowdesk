using FlowDesk.Api;
using FlowDesk.Api.Hubs;
using FlowDesk.Api.Middleware;
using FlowDesk.Domain.DTOs;
using FlowDesk.Infrastructure.Data;
using FlowDesk.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new UtcToIstJsonConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Inject AddApi Dependencies
builder.Services.AddApi(builder.Configuration);

//Dto's Validation Error Response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value?.Errors!)
            .Select(x => x.ErrorMessage)
            .ToList();

        var response = new ErrorResponseDto
        {
            StatusCode = 400,
            Message = "Validation failed",
            Errors = errors,
            TraceId = context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(response);
    };
});


// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


// Configure Serilog
builder.Host.UseSerilog();

var app = builder.Build();

// Configure seeding data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "FlowDesk API";
    });
}

app.MapHub<RequestHub>("/hubs/request").RequireCors("AllowAgent");

app.MapGet("/health", () => Results.Ok("OK"));

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
