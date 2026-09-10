using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;
using OnMuhasebe.WebUI.Filters;

namespace OnMuhasebe.WebUI.Controllers;

public class StokController : Controller
{
    private readonly IStokService _stokService;

    public StokController(IStokService stokService)
    {
        _stokService = stokService;
    }

    [Yetki(Modul.Stok, Islem.Goruntule)]
    public async Task<IActionResult> Liste(
        string? arama, bool sadeceAktif = true, bool sadeceKritik = false,
        int sayfa = 1, string sirala = "kod", string yon = "asc")
    {
        var liste = await _stokService.ListeleAsync(
            arama, sadeceAktif, sadeceKritik, sayfa, 20, sirala, yon);

        ViewBag.Arama        = arama;
        ViewBag.SadeceAktif  = sadeceAktif;
        ViewBag.SadeceKritik = sadeceKritik;
        ViewBag.Sirala       = sirala;
        ViewBag.Yon          = yon;

        return View(liste);
    }

    
    [Yetki(Modul.Stok, Islem.Goruntule)]
    public async Task<IActionResult> Detay(int id)
    {
        var model = await _stokService.DetayGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        return View(model);
    }


    [HttpGet]
    [Yetki(Modul.Stok, Islem.Ekle)]
    public async Task<IActionResult> Ekle()
    {
        await KategorileriYukle();

        return View("Form", new StokFormViewModel
        {
            StokKodu = await _stokService.SonrakiKodOnerAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Stok, Islem.Ekle)]
    public async Task<IActionResult> Ekle(StokFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.Stok, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(int id)
    {
        var model = await _stokService.FormGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        await KategorileriYukle();
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Stok, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(StokFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Stok, Islem.Sil)]
    public async Task<IActionResult> PasifeAl(int id)
    {
        var (basarili, mesaj) = await _stokService.PasifeAlAsync(id);

        if (basarili) TempData["Basarili"] = mesaj ?? "Stok kartı pasife alındı.";
        else          TempData["Hata"]     = mesaj;

        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Stok, Islem.Guncelle)]
    public async Task<IActionResult> AktifYap(int id)
    {
        var (basarili, hata) = await _stokService.AktifYapAsync(id);

        if (basarili) TempData["Basarili"] = "Stok kartı yeniden aktif edildi.";
        else          TempData["Hata"]     = hata;

        return RedirectToAction(nameof(Liste), new { sadeceAktif = false });
    }

    private async Task<IActionResult> KaydetVeYonlendir(StokFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await KategorileriYukle();
            return View("Form", model);
        }

        var (basarili, hata) = await _stokService.KaydetAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            await KategorileriYukle();
            return View("Form", model);
        }

        TempData["Basarili"] = model.Id == 0 ? "Stok kartı eklendi." : "Stok kartı güncellendi.";
        return RedirectToAction(nameof(Liste));
    }

    // Form her gosterildiginde gerekiyor; hata donuslerinde de unutulmamali.
    private async Task KategorileriYukle()
    {
        ViewBag.Kategoriler = await _stokService.KategorileriGetirAsync();
    }
}
