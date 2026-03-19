using Microsoft.Extensions.DependencyInjection;
using Patners.Application.Commands.CreatePartner;

namespace Patners.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPartnersApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreatePartnerCommand).Assembly));

        return services;
    }
}
