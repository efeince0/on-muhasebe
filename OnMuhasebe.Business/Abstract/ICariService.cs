using OnMuhasebe.Entities.ViewModels;

namespace OnMuhasebe.Business.Abstract;

public interface ICariService
{
    Task<List<CariListeViewModel>> ListeleAsync(string? arama = null, bool sadeceAktif = true);

    //Guncelleme formunu doldurmak icin. Bulunamazsa null
    Task<CariFormViewModel?> FormGetirAsync(int id);

    Task<(bool Basarili, string? Hata)> KaydetAsync(CariFormViewModel model);

    //Kalici silmez; Aktif = false 
    Task<(bool Basarili, string? Hata)> PasifeAlAsync(int id);

    //Pasif kaydi geri acar
    Task<(bool Basarili, string? Hata)> AktifYapAsync(int id);
}
