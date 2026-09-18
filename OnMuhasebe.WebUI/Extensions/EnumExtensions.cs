using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OnMuhasebe.WebUI.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Enum degerinin [Display(Name=...)] etiketini dondurur; yoksa enum adinin
    /// kendisi doner. View'larda dogrudan ToString() yerine bunu kullanmak,
    /// ekranda "Musteri" / "Ikisi" gibi Turkce karakter icermeyen ham enum
    /// adlarinin gorunmesini onler; asp-items="Html.GetEnumSelectList&lt;T&gt;()"
    /// zaten [Display] etiketini otomatik okuyor, bu sadece dogrudan yazdirilan
    /// yerler icindir.
    /// </summary>
    public static string Etiket(this Enum deger)
    {
        var alan = deger.GetType().GetField(deger.ToString());
        var display = alan?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? deger.ToString();
    }
}
