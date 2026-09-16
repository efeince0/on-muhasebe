using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Core.Enums;

namespace OnMuhasebe.Entities.ViewModels;

/// <summary>Rol listesindeki bir tablo satiri.</summary>
public class RolListeViewModel
{
    public int     Id              { get; set; }
    public string  RolAdi          { get; set; } = null!;
    public string? Aciklama        { get; set; }
    public int     KullaniciSayisi { get; set; }
    public int     IzinSayisi      { get; set; }
    public bool    Aktif           { get; set; }

    /// <summary>Kullanici yonetme yetkisini veren tek kaynak mi.</summary>
    public bool YonetimKaynagi { get; set; }
}

/// <summary>Rol ekleme ve guncelleme formu. Id = 0 ise yeni rol.</summary>
public class RolFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Rol adı zorunludur.")]
    [StringLength(50, ErrorMessage = "Rol adı en fazla 50 karakter olabilir.")]
    [Display(Name = "Rol Adı")]
    public string RolAdi { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;
}

/// <summary>
/// Rol x modul x islem onay kutusu matrisi.
/// Her modul bir satir, her satirda dort islem kutusu bulunur.
/// </summary>
public class YetkiMatrisiViewModel
{
    public int    RolId  { get; set; }
    public string RolAdi { get; set; } = null!;

    public List<MatrisSatirViewModel> Satirlar { get; set; } = [];
}

public class MatrisSatirViewModel
{
    public Modul Modul { get; set; }

    /// <summary>Ekranda gosterilecek okunabilir ad; enum adi degil.</summary>
    public string ModulAdi { get; set; } = null!;

    public bool Goruntule { get; set; }
    public bool Ekle      { get; set; }
    public bool Guncelle  { get; set; }
    public bool Sil       { get; set; }
}
