namespace OnMuhasebe.Entities.Common;

public abstract class BaseEntity
{
    public int Id                     { get; set; }
    public DateTime OlusturmaTarihi        { get; set; }
    public int? OlusturanKullaniciId   { get; set; }
    public DateTime? GuncellemeTarihi       { get; set; }
    public int? GuncelleyenKullaniciId { get; set; }
    public bool Aktif                  { get; set; } = true;
}