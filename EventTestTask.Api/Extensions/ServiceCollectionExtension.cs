using EventTestTask.Application.Services;
using EventTestTask.Application.Validators;
using EventTestTask.Core.DTOs.Event;
using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Mappings;
using EventTestTask.Infrastructure.Repositories;
using FluentValidation;

namespace EventTestTask.Api.Extensions;

public static class ServiceCollectionExtension
{
    #region Services

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEventsService, EventsService>();

        return services;
    }

    #endregion

    #region Repositories

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEventsRepository, EventsRepository>();

        return services;
    }

    #endregion

    #region Mappings

    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(
            typeof(UserProfile).Assembly,
            typeof(EventProfile).Assembly);

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
}