using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Ekleme ve guncelleme formu. Id = 0 ise yeni kayit.</summary>
public class StokFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Stok kodu zorunludur.")]
    [StringLength(20, ErrorMessage = "Stok kodu en fazla 20 karakter olabilir.")]
    [Display(Name = "Stok Kodu")]
    public string StokKodu { get; set; } = null!;

    [Required(ErrorMessage = "Stok adı zorunludur.")]
    [StringLength(150, ErrorMessage = "Stok adı en fazla 150 karakter olabilir.")]
    [Display(Name = "Stok Adı")]
    public string StokAdi { get; set; } = null!;

    [StringLength(50)]
    [Display(Name = "Kategori")]
    public string? Kategori { get; set; }

    [Required(ErrorMessage = "Birim zorunludur.")]
    [StringLength(15, ErrorMessage = "Birim en fazla 15 karakter olabilir.")]
    [Display(Name = "Birim")]
    public string Birim { get; set; } = "Adet";

    [Range(0, 99999999, ErrorMessage = "Alış fiyatı negatif olamaz.")]
    [Display(Name = "Alış Fiyatı")]
    public decimal AlisFiyati { get; set; }

    [Range(0, 99999999, ErrorMessage = "Satış fiyatı negatif olamaz.")]
    [Display(Name = "Satış Fiyatı")]
    public decimal SatisFiyati { get; set; }

    [Range(0, 100, ErrorMessage = "KDV oranı 0 ile 100 arasında olmalıdır.")]
    [Display(Name = "KDV Oranı (%)")]
    public decimal KdvOrani { get; set; } = 20;

    [Range(0, 99999999, ErrorMessage = "Kritik stok negatif olamaz.")]
    [Display(Name = "Kritik Stok Seviyesi")]
    public decimal? KritikStok { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;
}
