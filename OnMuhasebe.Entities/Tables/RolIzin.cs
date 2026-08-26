using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class RolIzin : BaseEntity
{
    public int   RolId   { get; set; }
    public Modul Modul   { get; set; }
    public Islem Islem   { get; set; }
    public bool  IzinVar { get; set; }

    public Rol Rol { get; set; } = null!;
}
