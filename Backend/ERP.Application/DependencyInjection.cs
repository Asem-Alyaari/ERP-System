using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using ERP.Application.Common.Behaviors;
using ERP.Application.Services;
using ERP.Domain.Services;

namespace ERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(ClosedFiscalPeriodValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);
        services.RegisterMapsterConfiguration();

        // Domain Services
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();

        return services;
    }
}
