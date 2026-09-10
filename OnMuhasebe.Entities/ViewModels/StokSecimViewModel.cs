namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Acilir listelerde kullanilan hafif stok karti.</summary>
public class StokSecimViewModel
{
    public int    Id       { get; set; }
    public string StokKodu { get; set; } = null!;
    public string StokAdi  { get; set; } = null!;
    public string Birim    { get; set; } = null!;

    public string Etiket => $"{StokKodu} - {StokAdi}";
}
