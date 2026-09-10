using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>
/// Stok hareketi kayit formu. Id = 0 ise yeni hareket.
/// Sayim'da kullanici SAYILAN miktari girer; farki servis hesaplar.
/// </summary>
public class StokHareketFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Hareket no zorunludur.")]
    [StringLength(20, ErrorMessage = "Hareket no en fazla 20 karakter olabilir.")]
    [Display(Name = "Hareket No")]
    public string HareketNo { get; set; } = null!;

    // Acilir listede "Seçiniz" secilirse 0 gelir; Range ile zorunlu kiliniyor.
    [Range(1, int.MaxValue, ErrorMessage = "Ürün seçilmelidir.")]
    [Display(Name = "Ürün")]
    public int StokId { get; set; }

    [Required(ErrorMessage = "Hareket tipi seçilmelidir.")]
    [Display(Name = "Hareket Tipi")]
    public StokHareketTipi HareketTipi { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    /// <summary>
    /// Giris/Cikis'ta hareket miktari, Sayim'da sayilan miktar.
    /// Sayim'da sifir gecerlidir: depoda hic mal kalmadigi da bir sayim sonucudur.
    /// </summary>
    [Range(0, 999999999, ErrorMessage = "Miktar negatif olamaz.")]
    [Display(Name = "Miktar")]
    public decimal Miktar { get; set; }

    [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;
}
