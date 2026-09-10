using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;
using OnMuhasebe.WebUI.Filters;

namespace OnMuhasebe.WebUI.Controllers;

public class CariController : Controller
{
    private readonly ICariService _cariService;

    public CariController(ICariService cariService)
    {
        _cariService = cariService;
    }

    [Yetki(Modul.Cari, Islem.Goruntule)]
    public async Task<IActionResult> Liste(
        string? arama, bool sadeceAktif = true, int sayfa = 1,
        string sirala = "kod", string yon = "asc")
    {
        var liste = await _cariService.ListeleAsync(arama, sadeceAktif, sayfa, 20, sirala, yon);

        // Sayfa ve siralama baglantilarinin suzgeci koruyabilmesi icin geri gonderiliyor.
        ViewBag.Arama       = arama;
        ViewBag.SadeceAktif = sadeceAktif;
        ViewBag.Sirala      = sirala;
        ViewBag.Yon         = yon;

        return View(liste);
    }

    [Yetki(Modul.Cari, Islem.Goruntule)]
    public async Task<IActionResult> Detay(int id)
    {
        var model = await _cariService.DetayGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        return View(model);
    }

    [HttpGet]
    [Yetki(Modul.Cari, Islem.Ekle)]
    public async Task<IActionResult> Ekle()
    {
        // Ekleme ve guncelleme ayni view'i paylasir; Id = 0 "yeni kayit" demek.
        // Kod yalnizca oneri; kullanici degistirebilir, benzersizlik yine serviste kontrol edilir.
        return View("Form", new CariFormViewModel
        {
            CariKodu = await _cariService.SonrakiKodOnerAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Cari, Islem.Ekle)]
    public async Task<IActionResult> Ekle(CariFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.Cari, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(int id)
    {
        var model = await _cariService.FormGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Cari, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(CariFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Cari, Islem.Sil)]
    public async Task<IActionResult> PasifeAl(int id)
    {
        var (basarili, mesaj) = await _cariService.PasifeAlAsync(id);

        if (basarili) TempData["Basarili"] = mesaj ?? "Cari pasife alındı.";
        else          TempData["Hata"]     = mesaj;

        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Cari, Islem.Guncelle)]
    public async Task<IActionResult> AktifYap(int id)
    {
        var (basarili, hata) = await _cariService.AktifYapAsync(id);

        if (basarili) TempData["Basarili"] = "Cari yeniden aktif edildi.";
        else          TempData["Hata"]     = hata;

        // Aktif edilen kaydin gorunmesi icin pasifler de listede kalsin.
        return RedirectToAction(nameof(Liste), new { sadeceAktif = false });
    }

    private async Task<IActionResult> KaydetVeYonlendir(CariFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Form", model);

        var (basarili, hata) = await _cariService.KaydetAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            return View("Form", model);
        }

        // POST -> Redirect -> GET: yenilemede kayit tekrar eklenmesin.
        TempData["Basarili"] = model.Id == 0 ? "Cari eklendi." : "Cari güncellendi.";
        return RedirectToAction(nameof(Liste));
    }
}
