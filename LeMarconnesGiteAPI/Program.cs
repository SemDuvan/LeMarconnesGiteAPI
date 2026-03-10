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
    app.UseSwagger(); // genereert het OpenAPI JSON-schema op /swagger/v1/swagger.json
    app.MapScalarApiReference(); // Scalar UI beschikbaar op /scalar/v1
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
