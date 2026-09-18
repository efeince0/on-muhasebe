using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Core.Enums;

public enum FaturaTipi
{
    [Display(Name = "Alış Faturası")]
    Alis = 1,

    [Display(Name = "Satış Faturası")]
    Satis = 2
}
