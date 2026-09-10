using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Tahsilat/odeme kayit formu. Id = 0 ise yeni islem.</summary>
public class CariIslemFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "İşlem no zorunludur.")]
    [StringLength(20, ErrorMessage = "İşlem no en fazla 20 karakter olabilir.")]
    [Display(Name = "İşlem No")]
    public string IslemNo { get; set; } = null!;

    // Acilir listede "Seçiniz" secilirse 0 gelir; Range ile zorunlu kiliniyor.
    [Range(1, int.MaxValue, ErrorMessage = "Cari seçilmelidir.")]
    [Display(Name = "Cari")]
    public int CariId { get; set; }

    [Required(ErrorMessage = "İşlem tipi seçilmelidir.")]
    [Display(Name = "İşlem Tipi")]
    public IslemTipi IslemTipi { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Range(0.01, 999999999, ErrorMessage = "Tutar sıfırdan büyük olmalıdır.")]
    [Display(Name = "Tutar")]
    public decimal Tutar { get; set; }

    [Required(ErrorMessage = "Ödeme şekli seçilmelidir.")]
    [Display(Name = "Ödeme Şekli")]
    public OdemeSekli OdemeSekli { get; set; }

    [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;
}
