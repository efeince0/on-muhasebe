using OnMuhasebe.Core.Enums;
using OnMuhasebe.Entities.Common;

namespace OnMuhasebe.Entities.Tables;

public class Cari : BaseEntity
{
    public string   CariKodu     { get; set; } = null!;
    public string   Unvan        { get; set; } = null!;
    public CariTipi CariTipi     { get; set; }
    public string?  VergiDairesi { get; set; }
    public string?  VergiNo      { get; set; }
    public string?  Telefon      { get; set; }
    public string?  Eposta       { get; set; }
    public string?  Adres        { get; set; }
    public decimal  AcilisBakiye { get; set; }

    public ICollection<CariIslem> CariIslemler { get; set; } = new List<CariIslem>();
    public ICollection<Fatura>    Faturalar    { get; set; } = new List<Fatura>();
}