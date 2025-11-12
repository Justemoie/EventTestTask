using System.Text.Json.Serialization;
using EventTestTask.Api.Extensions;
using EventTestTask.Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using EventTestTask.Infrastructure.ApplicationContext;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Добавьте сервисы в правильном порядке
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// 🔥 ВАЖНО: CORS должен быть добавлен ПЕРВЫМ
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ServiceCollection
builder.Services.AddMappings();
builder.Services.AddServices();
builder.Services.AddValidators();
builder.Services.AddRepositories();
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApiAuthorization(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

Console.WriteLine("Environment: " + builder.Environment.EnvironmentName);

var app = builder.Build();

// 🔥 ВАЖНО: CORS middleware должен быть в начале pipeline
app.UseCors("AllowReactApp");

// Program.cs
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting(); // 🔥 Добавьте это!

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();