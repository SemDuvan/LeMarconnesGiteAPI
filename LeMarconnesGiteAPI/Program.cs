using LeMarconnesGiteAPI.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core registreren
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// OpenAPI/Swagger (alleen voor het JSON-schema, Scalar is de UI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Scalar UI — werkt automatisch op HTTPS
if (app.Environment.IsDevelopment())
{
    // Serveer het OpenAPI JSON-schema op /openapi/v1.json zodat Scalar het kan vinden
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });
    // Scalar UI beschikbaar op /scalar/v1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
