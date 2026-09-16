namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Acilir listelerde kullanilan hafif rol kaydi.</summary>
public class RolSecimViewModel
{
    public int    Id       { get; set; }
    public string RolAdi   { get; set; } = null!;
    public string? Aciklama { get; set; }
}
