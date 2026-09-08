namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Stok liste ekranindaki bir tablo satiri.</summary>
public class StokListeViewModel
{
    public int Id { get; set; }
    public string StokKodu { get; set; } = null!;
    public string StokAdi { get; set; } = null!;
    public string? Kategori { get; set; }
    public string Birim { get; set; } = null!;
    public decimal SatisFiyati { get; set; }
    public decimal? KritikStok { get; set; }
    public bool Aktif { get; set; }
    public decimal AlisFiyati { get; set; }

    /// <summary>Tabloda saklanmaz; hareketlerden hesaplanir.</summary>
    public decimal MevcutMiktar { get; set; }

    /// <summary>Kritik seviye tanimliysa ve miktar onun altina dustuyse true.</summary>
    public bool KritikSeviyede => KritikStok.HasValue && MevcutMiktar <= KritikStok.Value;
}
