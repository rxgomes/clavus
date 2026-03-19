using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Patners.Domain.Repositories;
using Patners.Infrastructure.Data;
using Patners.Infrastructure.Repositories;

namespace Patners.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPartnersInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PartnersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPartnersRepository, PartnersRepository>();

        return services;
    }
}
