using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Core.Enums;

public enum StokHareketTipi
{
    [Display(Name = "Giriş")]
    Giris = 1,

    [Display(Name = "Çıkış")]
    Cikis = 2,

    [Display(Name = "Sayım Düzeltme")]
    Sayim = 3
}
