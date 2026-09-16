using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;
using OnMuhasebe.WebUI.Filters;

namespace OnMuhasebe.WebUI.Controllers;

public class RolController : Controller
{
    private readonly IRolService _rolService;

    public RolController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [Yetki(Modul.Rol, Islem.Goruntule)]
    public async Task<IActionResult> Liste(bool sadeceAktif = true)
    {
        ViewBag.SadeceAktif = sadeceAktif;
        return View(await _rolService.ListeleAsync(sadeceAktif));
    }

    [HttpGet]
    [Yetki(Modul.Rol, Islem.Ekle)]
    public IActionResult Ekle()
    {
        return View("Form", new RolFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Rol, Islem.Ekle)]
    public async Task<IActionResult> Ekle(RolFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.Rol, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(int id)
    {
        var model = await _rolService.FormGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Rol, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(RolFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.Rol, Islem.Guncelle)]
    public async Task<IActionResult> Matris(int id)
    {
        var model = await _rolService.MatrisGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Rol, Islem.Guncelle)]
    public async Task<IActionResult> Matris(YetkiMatrisiViewModel model)
    {
        var (basarili, hata) = await _rolService.MatrisKaydetAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            return View(model);
        }

        // Izinler cookie'de tasiniyor; degisiklik ancak yeniden giriste etkili olur.
        TempData["Basarili"] = "Yetkiler kaydedildi. Değişiklik, ilgili kullanıcılar yeniden giriş yaptığında etkili olur.";
        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Rol, Islem.Sil)]
    public async Task<IActionResult> PasifeAl(int id)
    {
        var (basarili, mesaj) = await _rolService.PasifeAlAsync(id);

        if (basarili) TempData["Basarili"] = mesaj ?? "Rol pasife alındı.";
        else          TempData["Hata"]     = mesaj;

        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Rol, Islem.Guncelle)]
    public async Task<IActionResult> AktifYap(int id)
    {
        var (basarili, hata) = await _rolService.AktifYapAsync(id);

        if (basarili) TempData["Basarili"] = "Rol yeniden aktif edildi.";
        else          TempData["Hata"]     = hata;

        return RedirectToAction(nameof(Liste), new { sadeceAktif = false });
    }

    private async Task<IActionResult> KaydetVeYonlendir(RolFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Form", model);

        var (basarili, hata) = await _rolService.KaydetAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            return View("Form", model);
        }

        TempData["Basarili"] = model.Id == 0
            ? "Rol eklendi. Yetkilerini matris ekranından tanımlayın."
            : "Rol güncellendi.";

        return RedirectToAction(nameof(Liste));
    }
}
