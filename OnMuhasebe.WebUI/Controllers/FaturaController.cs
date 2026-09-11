using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.ViewModels;
using OnMuhasebe.WebUI.Filters;

namespace OnMuhasebe.WebUI.Controllers;

public class FaturaController : Controller
{
    private readonly IFaturaService _faturaService;
    private readonly ICariService   _cariService;
    private readonly IStokService   _stokService;

    public FaturaController(
        IFaturaService faturaService,
        ICariService cariService,
        IStokService stokService)
    {
        _faturaService = faturaService;
        _cariService   = cariService;
        _stokService   = stokService;
    }

    [Yetki(Modul.Fatura, Islem.Goruntule)]
    public async Task<IActionResult> Liste(
        FaturaTipi? faturaTipi, int? cariId, DateTime? baslangic, DateTime? bitis,
        decimal? minTutar, decimal? maxTutar,
        bool sadeceAktif = true, int sayfa = 1,
        string sirala = "tarih", string yon = "desc")
    {
        var liste = await _faturaService.ListeleAsync(
            faturaTipi, cariId, baslangic, bitis, minTutar, maxTutar,
            sadeceAktif, sayfa, 20, sirala, yon);

        // Sayfa ve siralama baglantilarinin suzgeci koruyabilmesi icin geri gonderiliyor.
        ViewBag.FaturaTipi  = faturaTipi;
        ViewBag.CariId      = cariId;
        ViewBag.Baslangic   = baslangic;
        ViewBag.Bitis       = bitis;
        ViewBag.MinTutar    = minTutar;
        ViewBag.MaxTutar    = maxTutar;
        ViewBag.SadeceAktif = sadeceAktif;
        ViewBag.Sirala      = sirala;
        ViewBag.Yon         = yon;

        ViewBag.Cariler = await _cariService.SecimListesiAsync();
        return View(liste);
    }

    [HttpGet]
    [Yetki(Modul.Fatura, Islem.Ekle)]
    public async Task<IActionResult> Ekle(FaturaTipi faturaTipi = FaturaTipi.Satis, int? cariId = null)
    {
        var model = new FaturaFormViewModel
        {
            FaturaTipi = faturaTipi,
            CariId     = cariId ?? 0,
            FaturaNo   = await _faturaService.SonrakiFaturaNoOnerAsync(faturaTipi),

            // Form bos bir satirla acilsin. Degerler "Satir Ekle" sablonuyla ayni;
            // ikisi farkli olursa ilk satir digerlerinden farkli davraniyor gorunur.
            Satirlar   = [new FaturaSatirFormViewModel { Miktar = 1, KdvOrani = 20 }]
        };

        await ListeleriDoldur();
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Fatura, Islem.Ekle)]
    public async Task<IActionResult> Ekle(FaturaFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpGet]
    [Yetki(Modul.Fatura, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(int id)
    {
        var model = await _faturaService.FormGetirAsync(id);
        if (model == null)
        {
            TempData["Hata"] = "Kayıt bulunamadı.";
            return RedirectToAction(nameof(Liste));
        }

        await ListeleriDoldur();
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Fatura, Islem.Guncelle)]
    public async Task<IActionResult> Guncelle(FaturaFormViewModel model)
    {
        return await KaydetVeYonlendir(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Fatura, Islem.Sil)]
    public async Task<IActionResult> PasifeAl(int id)
    {
        var (basarili, hata) = await _faturaService.PasifeAlAsync(id);

        if (basarili) TempData["Basarili"] = "Fatura pasife alındı; stok ve cari hareketleri geri alındı.";
        else          TempData["Hata"]     = hata;

        return RedirectToAction(nameof(Liste));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Yetki(Modul.Fatura, Islem.Guncelle)]
    public async Task<IActionResult> AktifYap(int id)
    {
        var (basarili, mesaj) = await _faturaService.AktifYapAsync(id);

        if (basarili) TempData["Basarili"] = mesaj ?? "Fatura yeniden aktif edildi.";
        else          TempData["Hata"]     = mesaj;

        return RedirectToAction(nameof(Liste), new { sadeceAktif = false });
    }

    /// <summary>
    /// Form uzerinde fatura tipi degistiginde numarayi tazelemek icin.
    /// JSON dondurse de yetki kontrolu diger action'lardan farksiz uygulanir.
    /// </summary>
    [HttpGet]
    [Yetki(Modul.Fatura, Islem.Ekle)]
    public async Task<IActionResult> SonrakiNo(FaturaTipi faturaTipi)
    {
        var numara = await _faturaService.SonrakiFaturaNoOnerAsync(faturaTipi);
        return Json(new { faturaNo = numara });
    }

    private async Task<IActionResult> KaydetVeYonlendir(FaturaFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Dogrulama hatasinda form yeniden cizilir; acilir listeler bos kalmasin.
            await ListeleriDoldur();
            return View("Form", model);
        }

        var (basarili, hata) = await _faturaService.KaydetAsync(model);

        if (!basarili)
        {
            ModelState.AddModelError("", hata!);
            await ListeleriDoldur();
            return View("Form", model);
        }

        TempData["Basarili"] = model.Id == 0 ? "Fatura kaydedildi." : "Fatura güncellendi.";
        return RedirectToAction(nameof(Liste));
    }

    private async Task ListeleriDoldur()
    {
        ViewBag.Cariler = await _cariService.SecimListesiAsync();
        ViewBag.Stoklar = await _stokService.SecimListesiAsync();
    }
}
