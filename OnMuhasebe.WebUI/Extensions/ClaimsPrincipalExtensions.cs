using System.Security.Claims;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.WebUI.Extensions;

/// <summary>
/// View'larda ve controller'larda izin sorgulamayi kisaltir.
/// Kullanimi:  @if (User.IzniVar(Modul.Cari, Islem.Goruntule)) { ... }
/// </summary>
public static class ClaimsPrincipalExtensions
{
    // "this" ilk parametrede -> uzanti metodu olur,
    // ClaimsPrincipal sinifina sonradan metot eklemis gibi cagirilir.
    public static bool IzniVar(this ClaimsPrincipal kullanici, Modul modul, Islem islem)
    {
        return kullanici.HasClaim("Izin", $"{modul}.{islem}");
    }
}
