namespace OnMuhasebe.Entities.ViewModels;

/// <summary>
/// Acilir listelerde kullanilan hafif stok karti.
/// Fiyat ve KDV alanlari fatura satirinda varsayilan doldurmak icin tasiniyor;
/// kullanici bunlari satirda degistirebilir.
/// </summary>
public class StokSecimViewModel
{
    public int     Id          { get; set; }
    public string  StokKodu    { get; set; } = null!;
    public string  StokAdi     { get; set; } = null!;
    public string  Birim       { get; set; } = null!;
    public decimal AlisFiyati  { get; set; }
    public decimal SatisFiyati { get; set; }
    public decimal KdvOrani    { get; set; }

    public string Etiket => $"{StokKodu} - {StokAdi}";
}
