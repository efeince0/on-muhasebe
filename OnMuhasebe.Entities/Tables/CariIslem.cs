using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class CariIslem : BaseEntity
{
    public string     IslemNo    { get; set; } = null!;
    public int        CariId     { get; set; }
    public IslemTipi  IslemTipi  { get; set; }
    public DateTime   Tarih      { get; set; }
    public decimal    Tutar      { get; set; }
    public OdemeSekli OdemeSekli { get; set; }
    public string?    Aciklama   { get; set; }

    public Cari Cari { get; set; } = null!;
}
