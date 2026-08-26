using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class Rol : BaseEntity
{
    public string  RolAdi   { get; set; } = null!;
    public string? Aciklama { get; set; }

    public ICollection<Kullanici> Kullanicilar { get; set; } = new List<Kullanici>();
    public ICollection<RolIzin>   Izinler      { get; set; } = new List<RolIzin>();
}