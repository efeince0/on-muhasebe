using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using OnMuhasebe.Business.DependencyInjection;
using OnMuhasebe.Business.Seed;
using OnMuhasebe.WebUI.Logging;

var builder = WebApplication.CreateBuilder(args);

// Varsayilan kural: her action giris ister. Istisnalar [AllowAnonymous] tasir.
// Tersi (her action serbest, korumali olanlar isaretli) tek bir unutulmus
// attribute'un ucu herkese acik birakmasi demek olurdu.
var girisZorunlu = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddControllersWithViews(secenekler =>
{
    secenekler.Filters.Add(new AuthorizeFilter(girisZorunlu));
});

builder.Services.AddBusinessServices(
    builder.Configuration.GetConnectionString("OnMuhasebeDb")!);

// Serilog vb. bir pakete gerek kalmadan "teknik hatalar loglanmali" gereksinimini
// karsilamak icin: konsolun yaninda kalici bir dosyaya da Warning/Error loglar.
builder.Logging.AddProvider(new DosyaLoggerProvider(
    Path.Combine(builder.Environment.ContentRootPath, "Loglar")));

var app = builder.Build();
await app.Services.VeritabaniniHazirlaAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Sayi ve tarih bicimleri sunucunun isletim sistemine gore degismesin.
var kultur = new CultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(kultur),
    SupportedCultures     = [kultur],
    SupportedUICultures   = [kultur]
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
