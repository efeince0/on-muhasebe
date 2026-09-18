using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Core.Enums;

public enum OdemeSekli
{
    [Display(Name = "Nakit")]
    Nakit = 1,

    [Display(Name = "Havale/EFT")]
    Havale = 2,

    [Display(Name = "Kredi Kartı")]
    KrediKarti = 3,

    [Display(Name = "Çek/Senet")]
    CekSenet = 4
}
