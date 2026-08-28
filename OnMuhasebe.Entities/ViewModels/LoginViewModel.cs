using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Entities.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [Display(Name = "Kullanıcı Adı")]
    public string KullaniciAdi { get; set; } = null!;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [Display(Name = "Şifre")]
    [DataType(DataType.Password)]
    public string Sifre { get; set; } = null!;

    [Display(Name = "Beni hatırla")]
    public bool BeniHatirla { get; set; }

}