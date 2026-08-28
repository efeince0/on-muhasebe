using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using System.Security.Claims;
using OnMuhasebe.Entities.ViewModels;
using OnMuhasebe.Core.Enums;
using OnMuhasebe.Business.Concrete;
using OnMuhasebe.Entities.Tables;

namespace OnMuhasebe.WebUI.Controllers;

public class AccountController : Controller
{

    private readonly IKimlikService _kimlikService;

    public AccountController(IKimlikService kimlikService)
    {
        _kimlikService = kimlikService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        var kullanici = await _kimlikService.GirisDogrulaAsync(model.KullaniciAdi, model.Sifre);

        if (kullanici == null)
        {
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier , kullanici.Id.ToString()),
            new(ClaimTypes.Name, kullanici.KullaniciAdi),
            new("AdSoyad", kullanici.AdSoyad),
            new(ClaimTypes.Role, kullanici.Rol.RolAdi)

        };

        // Rolun izin satirlarini tek tek claim'e cevir.
        // Ornek uretilen deger: "Cari.Ekle", "Stok.Goruntule"
        // Boylece her istekte veritabanina gitmeden yetki sorgulanabilir.
        foreach (var izin in kullanici.Rol.Izinler.Where(i => i.IzinVar && i.Aktif))
        {
            claims.Add(new Claim("Izin", $"{izin.Modul}.{izin.Islem}"));
        }

        var kimlik = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(kimlik),
            new AuthenticationProperties
            {
                IsPersistent = model.BeniHatirla
            });

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

     
        return RedirectToAction(nameof(Login));
    }

  
    [AllowAnonymous]
    public IActionResult Yetkisiz()
    {
        return View();
    }
}