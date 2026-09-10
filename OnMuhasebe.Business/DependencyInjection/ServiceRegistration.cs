using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnMuhasebe.DataAccess.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Business.Concrete;


namespace OnMuhasebe.Business.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessServices(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<OnMuhasebeContext>(options =>
        options.UseSqlServer(connectionString));
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
      {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Yetkisiz";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);   
      });


        // Servisler denetim alanlari icin aktif kullanici Id'sini buradan okur.
        services.AddHttpContextAccessor();

        services.AddScoped<IKimlikService, KimlikService>();
        services.AddScoped<ICariService, CariService>();
        services.AddScoped<ICariIslemService, CariIslemService>();
        services.AddScoped<IStokService, StokService>();
        services.AddScoped<IStokHareketService, StokHareketService>();



        return services;
    }
}
