using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;
using OnMuhasebe.WebUI.Filters;

namespace OnMuhasebe.WebUI.Controllers;

public class StokHareketController : Controller
{
    private readonly IStokHareketService _stokHareketService;
    private readonly IStokService        _stokService;

    public StokHareketController(IStokHareketService stokHareketService, IStokService stokService)
    {
        _stokHareketService = stokHareketService;
        _stokService        = stokService;
    }

    [Yetki(Modul.StokHareket, Islem.Goruntule)]
    public async Task<IActionResult> Liste(
        int? stokId, DateTime? baslangic, DateTime? bitis, StokHareketTipi? hareketTipi,
        bool sadeceAktif = true, int sayfa = 1,
        string sirala = "tarih", string yon = "desc")
    {
        var liste = await _stokHareketService.ListeleAsync(
            stokId, baslangic, bitis, hareketTipi, sadeceAktif, sayfa, 20, sirala, yon);

        // Sayfa ve siralama baglantilarinin suzgeci koruyabilmesi icin geri gonderiliyor.
        ViewBag.StokId      = stokId;
        ViewBag.Baslangic   = baslangic;
        ViewBag.Bitis       = bitis;
        ViewBag.HareketTipi = hareketTipi;
        ViewBag.SadeceAktif = sadeceAktif;
        ViewBag.Sirala      = sirala;
        ViewBag.Yon         = yon;

        await StoklariDoldur();
        return View(liste);
    }

    [HttpGet]
    [Yetki(Modul.StokHareket, Islem.Ekle)]
    public async Task<IActionResult> Ekle(StokHareketTipi hareketTipi = StokHareketTipi.Giris, int? stokId = null)
    {
        var model = new StokHareketFormViewModel
        {
            HareketTipi = hareketTipi,
            StokId      = stokId ?? 0,
            HareketNo   = await _stokHareketService.SonrakiHareketNoOnerAsync(hareketTipi)
        };

        await StoklariDoldur();
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.StokHareket, Islem.Ekle)]
    public async Task<IActionResult> Ekle(StokHareketFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.StokHareket, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(int id)
    {
        var model = await _stokHareketService.FormGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        await StoklariDoldur();
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.StokHareket, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(StokHareketFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.StokHareket, Islem.Sil)]
    public async Task<IActionResult> PasifeAl(int id)
    {
        var (basarili, mesaj) = await _stokHareketService.PasifeAlAsync(id);

        if (basarili) TempData["Basarili"] = mesaj ?? "Hareket pasife alındı, ürün miktarı güncellendi.";
        else          TempData["Hata"]     = mesaj;

        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.StokHareket, Islem.Guncelle)]
    public async Task<IActionResult> AktifYap(int id)
    {
        var (basarili, hata) = await _stokHareketService.AktifYapAsync(id);

        if (basarili) TempData["Basarili"] = "Hareket yeniden aktif edildi.";
        else          TempData["Hata"]     = hata;

        return RedirectToAction(nameof(Liste), new { sadeceAktif = false });
    }

    /// <summary>
    /// Form uzerinde hareket tipi degistiginde numarayi tazelemek icin.
    /// JSON dondurse de yetki kontrolu diger action'lardan farksiz uygulanir.
    /// </summary>
    [HttpGet]
    [Yetki(Modul.StokHareket, Islem.Ekle)]
    public async Task<IActionResult> SonrakiNo(StokHareketTipi hareketTipi)
    {
        var numara = await _stokHareketService.SonrakiHareketNoOnerAsync(hareketTipi);
        return Json(new { hareketNo = numara });
    }

    /// <summary>
    /// Sayim formunda "su an kayitlarda ne gorunuyor" bilgisini gostermek icin.
    /// Kullanici sayilan miktari girerken neyi duzelttigini gormeli.
    /// </summary>
    [HttpGet]
    [Yetki(Modul.StokHareket, Islem.Goruntule)]
    public async Task<IActionResult> MevcutMiktar(int stokId, int? haricHareketId = null)
    {
        var miktar = await _stokService.MiktarGetirAsync(stokId, haricHareketId);
        return Json(new { miktar });
    }

    private async Task<IActionResult> KaydetVeYonlendir(StokHareketFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Dogrulama hatasinda form yeniden cizilir; acilir liste bos kalmasin.
            await StoklariDoldur();
            return View("Form", model);
        }

        var (basarili, hata) = await _stokHareketService.KaydetAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            await StoklariDoldur();
            return View("Form", model);
        }

        TempData["Basarili"] = model.Id == 0 ? "Hareket kaydedildi." : "Hareket güncellendi.";
        return RedirectToAction(nameof(Liste));
    }

    private async Task StoklariDoldur()
    {
        ViewBag.Stoklar = await _stokService.SecimListesiAsync();
    }
}
