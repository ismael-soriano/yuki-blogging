using Authors.Application;
using Authors.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddAuthorsApplication();
builder.Services.AddAuthorsInfrastructure();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program;
