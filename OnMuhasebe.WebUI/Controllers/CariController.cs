using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.WebUI.Filters;

namespace OnMuhasebe.WebUI.Controllers;

// GECICI: Gun 5'te yazdigimiz YetkiAttribute'u test etmek icin.
// Gun 6'da icini gercek cari islemleriyle dolduracagiz.
public class CariController : Controller
{
    // Bu action'a girebilmek icin cookie'de "Cari.Goruntule" claim'i olmali.
    [Yetki(Modul.Cari, Islem.Goruntule)]
    public IActionResult Liste()
    {
        return View();
    }

    // Bu action'a girebilmek icin "Cari.Sil" claim'i olmali.
    // Goruntuleyici rolunde bu izin YOK -> /Account/Yetkisiz sayfasina duser.
    [Yetki(Modul.Cari, Islem.Sil)]
    public IActionResult SilTest()
    {
        return Content("Silme yetkin var.");
    }
}
