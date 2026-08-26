using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class Kullanici : BaseEntity
{
    public string    KullaniciAdi { get; set; } = null!;
    public string    AdSoyad      { get; set; } = null!;
    public string?   Eposta       { get; set; }
    public string    SifreHash    { get; set; } = null!;
    public int       RolId        { get; set; }
    public DateTime? SonGiris     { get; set; }

    public Rol Rol { get; set; } = null!;
}