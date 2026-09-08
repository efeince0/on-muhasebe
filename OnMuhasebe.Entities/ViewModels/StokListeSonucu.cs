namespace OnMuhasebe.Entities.ViewModels;

/// <summary>
/// Sayfalanmis stok listesi + liste altinda gosterilen ozet bilgiler.
/// Ozetler suzgecin tamamini kapsar, yalnizca gorunen sayfayi degil.
/// </summary>
public class StokListeSonucu : SayfaliListe<StokListeViewModel>
{
    public decimal ToplamStokDegeri { get; set; }
    public int     KritikKartSayisi { get; set; }
    
}
