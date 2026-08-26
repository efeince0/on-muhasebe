using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class FaturaSatir : BaseEntity
{
    public int     FaturaId    { get; set; }
    public int     StokId      { get; set; }
    public decimal Miktar      { get; set; }
    public decimal BirimFiyat  { get; set; }
    public decimal KdvOrani    { get; set; }
    public decimal SatirTutari { get; set; }

    public Fatura Fatura { get; set; } = null!;
    public Stok   Stok   { get; set; } = null!;
}
