using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>
/// Kullanici ekleme ve guncelleme formu. Id = 0 ise yeni kayit.
/// Sifre yalnizca yeni kayitta istenir; mevcut kaydin sifresi
/// bu formdan degil, ayri sifirlama ekranindan degistirilir.
/// </summary>
public class KullaniciFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
    [Display(Name = "Kullanıcı Adı")]
    public string KullaniciAdi { get; set; } = null!;

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = null!;

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string? Eposta { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Rol seçilmelidir.")]
    [Display(Name = "Rol")]
    public int RolId { get; set; }

    /// <summary>Yalnizca yeni kayitta doldurulur; guncellemede bos gelir ve yok sayilir.</summary>
    [DataType(DataType.Password)]
    [RegularExpression(SifreKurali.Desen, ErrorMessage = SifreKurali.Mesaj)]
    [Display(Name = "Şifre")]
    public string? Sifre { get; set; }
}
