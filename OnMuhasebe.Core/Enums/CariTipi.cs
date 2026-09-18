using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Core.Enums;

public enum CariTipi
{
    [Display(Name = "Müşteri")]
    Musteri = 1,

    [Display(Name = "Tedarikçi")]
    Tedarikci = 2,

    [Display(Name = "Her İkisi")]
    Ikisi = 3
}
