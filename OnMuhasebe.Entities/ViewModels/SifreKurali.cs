namespace OnMuhasebe.Entities.ViewModels;

/// <summary>
/// Dokuman [353]: yeni sifreler icin asgari karmasiklik kurali uygulanmali.
/// Kural tek yerde tanimli; hem form niteliklerinde hem serviste buradan okunur.
/// </summary>
public static class SifreKurali
{
    /// <summary>En az 8 karakter, icinde en az bir harf ve bir rakam.</summary>
    public const string Desen = @"^(?=.*[A-Za-zçğıöşüÇĞİÖŞÜ])(?=.*\d).{8,}$";

    public const string Mesaj = "Şifre en az 8 karakter olmalı ve en az bir harf ile bir rakam içermelidir.";
}
