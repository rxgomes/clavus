using FinancialRecord.Application.Commands.CreateFinancialRecord;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialRecord.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinancialRecordApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateFinancialRecordCommand).Assembly));

        return services;
    }
}
