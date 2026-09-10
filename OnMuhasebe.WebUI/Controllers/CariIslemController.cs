using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;
using OnMuhasebe.WebUI.Filters;

namespace OnMuhasebe.WebUI.Controllers;

public class CariIslemController : Controller
{
    private readonly ICariIslemService _cariIslemService;
    private readonly ICariService      _cariService;

    public CariIslemController(ICariIslemService cariIslemService, ICariService cariService)
    {
        _cariIslemService = cariIslemService;
        _cariService      = cariService;
    }

    [Yetki(Modul.CariIslem, Islem.Goruntule)]
    public async Task<IActionResult> Liste(
        int? cariId, DateTime? baslangic, DateTime? bitis, IslemTipi? islemTipi,
        bool sadeceAktif = true, int sayfa = 1,
        string sirala = "tarih", string yon = "desc")
    {
        var liste = await _cariIslemService.ListeleAsync(
            cariId, baslangic, bitis, islemTipi, sadeceAktif, sayfa, 20, sirala, yon);

        await SuzgecleriDoldur(cariId, baslangic, bitis, islemTipi, sadeceAktif, sirala, yon);

        return View(liste);
    }

    [HttpGet]
    [Yetki(Modul.CariIslem, Islem.Ekle)]
    public async Task<IActionResult> Ekle(IslemTipi islemTipi = IslemTipi.Tahsilat, int? cariId = null)
    {
        // Makbuz numarasi tipe gore uretilir; bu yuzden tip formdan once secilir.
        var model = new CariIslemFormViewModel
        {
            IslemTipi = islemTipi,
            CariId    = cariId ?? 0,
            IslemNo   = await _cariIslemService.SonrakiIslemNoOnerAsync(islemTipi)
        };

        await CarileriDoldur();
        return View("Form", model);
    }

    /// <summary>
    /// Form uzerinde islem tipi degistiginde makbuz numarasini tazelemek icin.
    /// JSON dondurse de yetki kontrolu diger action'lardan farksiz uygulanir.
    /// </summary>
    [HttpGet]
    [Yetki(Modul.CariIslem, Islem.Ekle)]
    public async Task<IActionResult> SonrakiNo(IslemTipi islemTipi)
    {
        var numara = await _cariIslemService.SonrakiIslemNoOnerAsync(islemTipi);
        return Json(new { islemNo = numara });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.CariIslem, Islem.Ekle)]
    public async Task<IActionResult> Ekle(CariIslemFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.CariIslem, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(int id)
    {
        var model = await _cariIslemService.FormGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        await CarileriDoldur();
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.CariIslem, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(CariIslemFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.CariIslem, Islem.Sil)]
    public async Task<IActionResult> PasifeAl(int id)
    {
        var (basarili, hata) = await _cariIslemService.PasifeAlAsync(id);

        if (basarili) TempData["Basarili"] = "İşlem pasife alındı, cari bakiyesi güncellendi.";
        else          TempData["Hata"]     = hata;

        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.CariIslem, Islem.Guncelle)]
    public async Task<IActionResult> AktifYap(int id)
    {
        var (basarili, hata) = await _cariIslemService.AktifYapAsync(id);

        if (basarili) TempData["Basarili"] = "İşlem yeniden aktif edildi.";
        else          TempData["Hata"]     = hata;

        return RedirectToAction(nameof(Liste), new { sadeceAktif = false });
    }

    [Yetki(Modul.CariIslem, Islem.Goruntule)]
    public async Task<IActionResult> Ekstre(int cariId, DateTime? baslangic, DateTime? bitis)
    {
        var model = await _cariIslemService.EkstreGetirAsync(cariId, baslangic, bitis);
        if (model == null)
        {
            TempData["Hata"] = "Cari bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        return View(model);
    }

    private async Task<IActionResult> KaydetVeYonlendir(CariIslemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Dogrulama hatasinda form yeniden cizilir; acilir liste bos kalmasin.
            await CarileriDoldur();
            return View("Form", model);
        }

        var (basarili, hata) = await _cariIslemService.KaydetAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            await CarileriDoldur();
            return View("Form", model);
        }

        TempData["Basarili"] = model.Id == 0 ? "İşlem kaydedildi." : "İşlem güncellendi.";
        return RedirectToAction(nameof(Liste));
    }

    private async Task CarileriDoldur()
    {
        ViewBag.Cariler = await _cariService.SecimListesiAsync();
    }

    private async Task SuzgecleriDoldur(
        int? cariId, DateTime? baslangic, DateTime? bitis, IslemTipi? islemTipi,
        bool sadeceAktif, string sirala, string yon)
    {
        // Sayfa ve siralama baglantilarinin suzgeci koruyabilmesi icin geri gonderiliyor.
        ViewBag.CariId      = cariId;
        ViewBag.Baslangic   = baslangic;
        ViewBag.Bitis       = bitis;
        ViewBag.IslemTipi   = islemTipi;
        ViewBag.SadeceAktif = sadeceAktif;
        ViewBag.Sirala      = sirala;
        ViewBag.Yon         = yon;

        await CarileriDoldur();
    }
}
