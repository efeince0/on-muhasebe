namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Kullanici listesindeki bir tablo satiri.</summary>
public class KullaniciListeViewModel
{
    public int       Id           { get; set; }
    public string    KullaniciAdi { get; set; } = null!;
    public string    AdSoyad      { get; set; } = null!;
    public string?   Eposta       { get; set; }
    public string    RolAdi       { get; set; } = null!;
    public DateTime? SonGiris     { get; set; }
    public bool      Aktif        { get; set; }

    /// <summary>
    /// Kullanici yonetebilen tek aktif hesap mi. Dokuman [352] geregi
    /// boyle bir hesap pasife alinamaz ve rolu degistirilemez.
    /// </summary>
    public bool SonYonetici { get; set; }
}
