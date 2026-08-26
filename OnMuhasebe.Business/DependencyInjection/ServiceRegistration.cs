using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnMuhasebe.DataAccess.Context;

namespace OnMuhasebe.Business.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<OnMuhasebeContext>(options =>
            options.UseSqlServer(connectionString));

        // Servisler ileride buraya eklenecek:
        // services.AddScoped<ICariService, CariService>();
        // services.AddScoped<IStokService, StokService>();
        // services.AddScoped<IFaturaService, FaturaService>();

        return services;
    }
}
