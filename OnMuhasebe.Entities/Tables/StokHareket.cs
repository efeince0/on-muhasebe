using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class StokHareket : BaseEntity
{
    public string          HareketNo   { get; set; } = null!;
    public int             StokId      { get; set; }
    public StokHareketTipi HareketTipi { get; set; }
    public decimal         Miktar      { get; set; }
    public DateTime        Tarih       { get; set; }
    public string?         Aciklama    { get; set; }
    public int?            FaturaId    { get; set; }

    public Stok    Stok   { get; set; } = null!;
    public Fatura? Fatura { get; set; }
}
