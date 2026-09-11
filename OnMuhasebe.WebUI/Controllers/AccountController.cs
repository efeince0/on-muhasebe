using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using OnMuhasebe.Entities.ViewModels;
using System.Security.Claims;

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
    public IActionResult Login(string? returnUrl = null)
    {
        // Oturumu dusen kullanici hangi sayfadan geldiyse oraya donsun.
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

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
            new(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new(ClaimTypes.Name, kullanici.KullaniciAdi),
            new("AdSoyad", kullanici.AdSoyad),
            new(ClaimTypes.Role, kullanici.Rol.RolAdi)
        };

        // Izinler giriste bir kez cozulup cookie'ye yazilir; her istekte sorgu yapilmaz.
        foreach (var izin in kullanici.Rol.Izinler.Where(i => i.IzinVar && i.Aktif))
        {
            claims.Add(new Claim("Izin", $"{izin.Modul}.{izin.Islem}"));
        }

        var kimlik = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(kimlik),
            new AuthenticationProperties { IsPersistent = model.BeniHatirla });

        // returnUrl adres cubugundan geliyor; disariya yonlendirme yapilmasin diye
        // yalnizca kendi sitemizin goreli adresleri kabul ediliyor.
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // Cikis oturumu sonlandiriyor, yani durum degistiriyor: GET olamaz.
    // Link olsaydi sayfaya gomulu bir <img src="/Account/Logout"> kullaniciyi atardi.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult Yetkisiz()
    {
        // Sayfa gorunse de istek basarili degil; loglar ve tarayici bunu bilmeli.
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
