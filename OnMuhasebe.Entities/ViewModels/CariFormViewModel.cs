using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Ekleme ve guncelleme formu. Id = 0 ise yeni kayit.</summary>
public class CariFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Cari kodu zorunludur.")]
    [StringLength(20, ErrorMessage = "Cari kodu en fazla 20 karakter olabilir.")]
    [Display(Name = "Cari Kodu")]
    public string CariKodu { get; set; } = null!;

    [Required(ErrorMessage = "Ünvan zorunludur.")]
    [StringLength(150, ErrorMessage = "Ünvan en fazla 150 karakter olabilir.")]
    [Display(Name = "Ünvan")]
    public string Unvan { get; set; } = null!;

    [Required(ErrorMessage = "Cari tipi seçilmelidir.")]
    [Display(Name = "Cari Tipi")]
    public CariTipi CariTipi { get; set; }

    [StringLength(80)]
    [Display(Name = "Vergi Dairesi")]
    public string? VergiDairesi { get; set; }

    [StringLength(15)]
    [Display(Name = "Vergi No")]
    public string? VergiNo { get; set; }

    [StringLength(20)]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string? Eposta { get; set; }

    [StringLength(250)]
    [Display(Name = "Adres")]
    public string? Adres { get; set; }

    [Display(Name = "Açılış Bakiyesi")]
    public decimal AcilisBakiye { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;
}
