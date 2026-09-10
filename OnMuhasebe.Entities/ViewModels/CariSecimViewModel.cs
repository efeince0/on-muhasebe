namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Acilir listelerde kullanilan hafif cari kaydi.</summary>
public class CariSecimViewModel
{
    public int    Id       { get; set; }
    public string CariKodu { get; set; } = null!;
    public string Unvan    { get; set; } = null!;

    public string Etiket => $"{CariKodu} - {Unvan}";
}
