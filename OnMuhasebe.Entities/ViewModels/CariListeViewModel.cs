using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Liste ekranindaki bir tablo satiri.</summary>
public class CariListeViewModel
{
    public int      Id       { get; set; }
    public string   CariKodu { get; set; } = null!;
    public string   Unvan    { get; set; } = null!;
    public CariTipi CariTipi { get; set; }
    public string?  Telefon  { get; set; }

    /// <summary>Tabloda saklanmaz; hareketlerden hesaplanir.</summary>
    public decimal GuncelBakiye { get; set; }

    public bool Aktif { get; set; }
}
