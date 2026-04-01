using FinancialRecord.Domain.Repositories;
using FinancialRecord.Infrastructure.Data;
using FinancialRecord.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialRecord.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancialRecordInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<FinancialRecordDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IFinancialRecordRepository, FinancialRecordRepository>();

        return services;
    }
}
