namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Sayfalanmis liste sonucu. Kayitlar + sayfa bilgisi bir arada tasinir.</summary>
public class SayfaliListe<T>
{
    public List<T> Kayitlar    { get; set; } = [];
    public int     ToplamKayit { get; set; }
    public int     SayfaNo     { get; set; } = 1;
    public int     SayfaBoyutu { get; set; } = 20;

    public int  ToplamSayfa => ToplamKayit == 0 ? 1 : (int)Math.Ceiling(ToplamKayit / (double)SayfaBoyutu);
    public bool OncekiVar   => SayfaNo > 1;
    public bool SonrakiVar  => SayfaNo < ToplamSayfa;
}
