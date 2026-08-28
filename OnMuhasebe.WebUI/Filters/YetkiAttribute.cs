using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.WebUI.Filters;

/// <summary>
/// Action calismadan ONCE araya girer ve kullanicinin cookie'sinde
/// gerekli izin claim'i var mi diye bakar.
/// Kullanimi:  [Yetki(Modul.Cari, Islem.Ekle)]
/// </summary>
public class YetkiAttribute : ActionFilterAttribute
{
    // Cookie'de aradigimiz metin. Ornek: "Cari.Ekle"
    private readonly string _gerekliIzin;

    public YetkiAttribute(Modul modul, Islem islem)
    {
        _gerekliIzin = $"{modul}.{islem}";
    }

    // Action'dan hemen once calisir.
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var kullanici = context.HttpContext.User;

        // 1) Hic giris yapmamis -> giris sayfasina
        if (kullanici.Identity?.IsAuthenticated != true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // 2) Giris yapmis ama bu izne sahip degil -> yetkisiz sayfasina
        if (!kullanici.HasClaim("Izin", _gerekliIzin))
        {
            context.Result = new RedirectToActionResult("Yetkisiz", "Account", null);
            return;
        }

        // 3) Yetkisi var -> boru hattina devam et, action calissin
        base.OnActionExecuting(context);
    }
}
