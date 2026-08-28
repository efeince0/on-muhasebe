using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Abstract;
using System.Security.Claims;
using OnMuhasebe.Entities.ViewModels;

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
            ModelState.AddModelError("KullaniciAdi", "Kullanıcı adı veya şifre hatalı");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier , kullanici.Id.ToString()),
            new(ClaimTypes.Name, kullanici.KullaniciAdi),
            new("AdSoyad", kullanici.AdSoyad),
            new(ClaimTypes.Role, kullanici.Rol.RolAdi)

        };

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


}