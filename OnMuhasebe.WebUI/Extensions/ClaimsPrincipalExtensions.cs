using System.Security.Claims;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.WebUI.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>View'larda menu ve buton gorunurlugu icin. Sunucu tarafi korumanin yerini tutmaz.</summary>
    public static bool IzniVar(this ClaimsPrincipal kullanici, Modul modul, Islem islem)
    {
        return kullanici.HasClaim("Izin", $"{modul}.{islem}");
    }
}
