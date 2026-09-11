using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Fatura ust bilgisi + satirlari. Id = 0 ise yeni fatura.</summary>
public class FaturaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Fatura no zorunludur.")]
    [StringLength(20, ErrorMessage = "Fatura no en fazla 20 karakter olabilir.")]
    [Display(Name = "Fatura No")]
    public string FaturaNo { get; set; } = null!;

    [Required(ErrorMessage = "Fatura tipi seçilmelidir.")]
    [Display(Name = "Fatura Tipi")]
    public FaturaTipi FaturaTipi { get; set; }

    // Acilir listede "Seçiniz" secilirse 0 gelir; Range ile zorunlu kiliniyor.
    [Range(1, int.MaxValue, ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int CariId { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;

    /// <summary>
    /// Satirlar formdan Satirlar[0].StokId gibi adlarla gelir.
    /// Bos satirlar serviste elenir; en az bir gecerli satir zorunludur.
    /// </summary>
    public List<FaturaSatirFormViewModel> Satirlar { get; set; } = [];
}

/// <summary>
/// Izgaradaki bir satir. Dogrulama nitelikleri yok: satir kurallari
/// (miktar, fiyat, KDV araligi) serviste topluca kontrol ediliyor, cunku
/// bir satirin gecerliligi digerlerine ve stok durumuna bagli.
/// </summary>
public class FaturaSatirFormViewModel
{
    public int     Id         { get; set; }
    public int     StokId     { get; set; }
    public decimal Miktar     { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani   { get; set; }
}
