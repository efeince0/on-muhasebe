using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class Stok : BaseEntity
{
    public string   StokKodu    { get; set; } = null!;
    public string   StokAdi     { get; set; } = null!;
    public string?  Kategori    { get; set; }
    public string   Birim       { get; set; } = null!;
    public decimal  AlisFiyati  { get; set; }
    public decimal  SatisFiyati { get; set; }
    public decimal  KdvOrani    { get; set; }
    public decimal? KritikStok  { get; set; }

    public ICollection<StokHareket> Hareketler      { get; set; } = new List<StokHareket>();
    public ICollection<FaturaSatir> FaturaSatirlari { get; set; } = new List<FaturaSatir>();
}