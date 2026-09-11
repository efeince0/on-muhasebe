using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.WebUI.Filters;

/// <summary>
/// Action calismadan once, kullanicinin cookie'sinde ilgili izin claim'i var mi kontrol eder.
/// Kullanimi: [Yetki(Modul.Cari, Islem.Ekle)]
/// </summary>
public class YetkiAttribute : ActionFilterAttribute
{
    private readonly string _gerekliIzin;

    public YetkiAttribute(Modul modul, Islem islem)
    {
        _gerekliIzin = $"{modul}.{islem}";
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var kullanici = context.HttpContext.User;

        // Elle Login'e yonlendirmek yerine Challenge: cookie ara katmani
        // LoginPath'e gonderirken gelinen adresi returnUrl olarak ekliyor.
        if (kullanici.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        // Forbid da ayni sekilde AccessDeniedPath'e gonderiyor.
        if (!kullanici.HasClaim("Izin", _gerekliIzin))
        {
            context.Result = new ForbidResult();
            return;
        }

        base.OnActionExecuting(context);
    }
}
