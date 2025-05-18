using System.Text;
using EventTestTask.Application.Authentication;
using EventTestTask.Application.Services;
using EventTestTask.Application.Validators;
using EventTestTask.Core.DTOs.Event;
using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Interfaces.PasswordHasher;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Mappings;
using EventTestTask.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EventTestTask.Api.Extensions;

public static class ServiceCollectionExtension
{
    #region Services

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IEventsService, EventsService>();
        services.AddScoped<IRegistrationsService, RegistartionsService>();
        services.AddScoped<IJwtTokensService, JwtTokensService>();
        
        return services;
    }

    #endregion

    #region Repositories

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IEventsRepository, EventsRepository>();
        services.AddScoped<IRegistrationsRepository, RegistrationsRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        
        return services;
    }

    #endregion

    #region Mappings

    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(
            typeof(UserProfile).Assembly,
            typeof(EventProfile).Assembly,
            typeof(RegistrationProfile).Assembly);

        return services;
    }

    #endregion

    #region Validators

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<UserRequest>, UserValidator>();
        services.AddScoped<IValidator<EventRequest>, EventValidator>();

        return services;
    }

    #endregion

    #region Authentication

    public static void AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var jwtOptions = configuration.GetSection("JWT").Get<JwtOptions>();

                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions!.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.ContainsKey("_at"))
                        {
                            context.Token = context.Request.Cookies["_at"];
                        }

                        return Task.CompletedTask;
                    }
                };
            });
    }

    #endregion

    #region Authorization

    public static void AddApiAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("JWT"));

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireClaim("Admin"));
            options.AddPolicy("User", policy => policy.RequireClaim("User"));
        });
    }

    #endregion
}