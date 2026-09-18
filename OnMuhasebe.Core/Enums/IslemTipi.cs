using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Core.Enums;

public enum IslemTipi
{
    [Display(Name = "Tahsilat")]
    Tahsilat = 1,

    [Display(Name = "Ödeme")]
    Odeme = 2
}
