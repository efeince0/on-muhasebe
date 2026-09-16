using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Yoneticinin baska bir kullanicinin sifresini sifirlamasi. Eski sifre sorulmaz.</summary>
public class SifreSifirlaViewModel
{
    public int    Id           { get; set; }
    public string KullaniciAdi { get; set; } = null!;
    public string AdSoyad      { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [DataType(DataType.Password)]
    [RegularExpression(SifreKurali.Desen, ErrorMessage = SifreKurali.Mesaj)]
    [Display(Name = "Yeni Şifre")]
    public string YeniSifre { get; set; } = null!;

    [DataType(DataType.Password)]
    [Compare(nameof(YeniSifre), ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Yeni Şifre (tekrar)")]
    public string YeniSifreTekrar { get; set; } = null!;
}

/// <summary>Kullanicinin kendi sifresini degistirmesi. Mevcut sifre dogrulanir.</summary>
public class SifreDegistirViewModel
{
    [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mevcut Şifre")]
    public string MevcutSifre { get; set; } = null!;

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [DataType(DataType.Password)]
    [RegularExpression(SifreKurali.Desen, ErrorMessage = SifreKurali.Mesaj)]
    [Display(Name = "Yeni Şifre")]
    public string YeniSifre { get; set; } = null!;

    [DataType(DataType.Password)]
    [Compare(nameof(YeniSifre), ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Yeni Şifre (tekrar)")]
    public string YeniSifreTekrar { get; set; } = null!;
}
