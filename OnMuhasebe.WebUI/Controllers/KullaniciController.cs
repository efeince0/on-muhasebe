using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;
using OnMuhasebe.WebUI.Filters;

namespace OnMuhasebe.WebUI.Controllers;

public class KullaniciController : Controller
{
    private readonly IKullaniciService _kullaniciService;

    public KullaniciController(IKullaniciService kullaniciService)
    {
        _kullaniciService = kullaniciService;
    }

    [Yetki(Modul.Kullanici, Islem.Goruntule)]
    public async Task<IActionResult> Liste(
        string? arama, bool sadeceAktif = true, int sayfa = 1,
        string sirala = "kullaniciAdi", string yon = "asc")
    {
        var liste = await _kullaniciService.ListeleAsync(arama, sadeceAktif, sayfa, 20, sirala, yon);

        // Sayfa ve siralama baglantilarinin suzgeci koruyabilmesi icin geri gonderiliyor.
        ViewBag.Arama       = arama;
        ViewBag.SadeceAktif = sadeceAktif;
        ViewBag.Sirala      = sirala;
        ViewBag.Yon         = yon;

        return View(liste);
    }

    [HttpGet]
    [Yetki(Modul.Kullanici, Islem.Ekle)]
    public async Task<IActionResult> Ekle()
    {
        await RolleriDoldur();
        return View("Form", new KullaniciFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Kullanici, Islem.Ekle)]
    public async Task<IActionResult> Ekle(KullaniciFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.Kullanici, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(int id)
    {
        var model = await _kullaniciService.FormGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        await RolleriDoldur();
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Kullanici, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(KullaniciFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.Kullanici, Islem.Guncelle)]
    public async Task<IActionResult> SifreSifirla(int id)
    {
        var model = await _kullaniciService.SifreFormuGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Kullanici, Islem.Guncelle)]
    public async Task<IActionResult> SifreSifirla(SifreSifirlaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (basarili, hata) = await _kullaniciService.SifreSifirlaAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            return View(model);
        }

        TempData["Basarili"] = $"{model.KullaniciAdi} kullanıcısının şifresi sıfırlandı.";
        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Kullanici, Islem.Sil)]
    public async Task<IActionResult> PasifeAl(int id)
    {
        var (basarili, hata) = await _kullaniciService.PasifeAlAsync(id);

        if (basarili) TempData["Basarili"] = "Kullanıcı pasife alındı.";
        else          TempData["Hata"]     = hata;

        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Kullanici, Islem.Guncelle)]
    public async Task<IActionResult> AktifYap(int id)
    {
        var (basarili, hata) = await _kullaniciService.AktifYapAsync(id);

        if (basarili) TempData["Basarili"] = "Kullanıcı yeniden aktif edildi.";
        else          TempData["Hata"]     = hata;

        return RedirectToAction(nameof(Liste), new { sadeceAktif = false });
    }

    private async Task<IActionResult> KaydetVeYonlendir(KullaniciFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Dogrulama hatasinda form yeniden cizilir; acilir liste bos kalmasin.
            await RolleriDoldur();
            return View("Form", model);
        }

        var (basarili, hata) = await _kullaniciService.KaydetAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            await RolleriDoldur();
            return View("Form", model);
        }

        TempData["Basarili"] = model.Id == 0 ? "Kullanıcı eklendi." : "Kullanıcı güncellendi.";
        return RedirectToAction(nameof(Liste));
    }

    private async Task RolleriDoldur()
    {
        ViewBag.Roller = await _kullaniciService.RolSecimListesiAsync();
    }
}
