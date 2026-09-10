using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class Fatura : BaseEntity
{
    public string     FaturaNo    { get; set; } = null!;
    public FaturaTipi FaturaTipi  { get; set; }
    public int        CariId      { get; set; }
    public DateTime   Tarih       { get; set; }
    public decimal    AraToplam   { get; set; }
    public decimal    ToplamKdv   { get; set; }
    public decimal    GenelToplam { get; set; }
    public string?    Aciklama    { get; set; }

    public Cari Cari { get; set; } = null!;

    public ICollection<FaturaSatir> Satirlar { get; set; } = new List<FaturaSatir>();
}
