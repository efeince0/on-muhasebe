# Ön Muhasebe Yönetim Sistemi

Küçük ve orta ölçekli işletmeler için cari, stok ve fatura takibi yapan web uygulaması.

**ASP.NET Core MVC (.NET 10)** · **EF Core 10 — Code First** · **SQL Server 2025** · **Bootstrap 5**

---

## Mimari

Beş katman, tek yönlü bağımlılık:

```
WebUI  →  Business  →  DataAccess  →  Entities  →  Core
```

| Proje | İçinde |
|---|---|
| `Core` | Enum'lar, sabitler |
| `Entities` | `BaseEntity`, 9 varlık sınıfı, ViewModel'ler |
| `DataAccess` | `DbContext`, Fluent API konfigürasyonları, migration'lar |
| `Business` | Servisler, iş kuralları, DI kayıtları, seed |
| `WebUI` | Controller'lar, Razor view'lar, yetki filtresi |

WebUI projesi DataAccess'e referans vermez; veri erişimi Business servisleri üzerinden yapılır.

---

## Modüller

| Modül | Ekranlar | Durum |
|---|---|---|
| Cari | Liste, form, detay, hesap ekstresi | Tamam |
| Cari İşlemleri | Tahsilat / ödeme listesi ve formu | Tamam |
| Stok | Liste, form, detay | Tamam |
| Stok İşlemleri | Giriş / çıkış / sayım listesi ve formu | Tamam |
| Fatura | Liste, satır ızgaralı form, detay ve yazdırma | Tamam |
| Kullanıcı Yönetimi | Liste, form, şifre sıfırlama, şifre değiştirme | Tamam |
| Rol ve İzin | Rol listesi, form, yetki matrisi | Tamam |

---

## Yetkilendirme

Dört rol (Yönetici, Muhasebe, Depo, Görüntüleyici) ve yedi modül × dört işlem izni.

- İzinler girişte bir kez claim'lere çözülür; her istekte veritabanına sorulmaz.
- Kontrol iki yerde: menü görünürlüğü (görgü kuralı) ve `[Yetki]` filtresi (asıl koruma).
- Varsayılan politika **giriş şartı**; istisnalar `[AllowAnonymous]` taşır.
- Yetki matrisi ekranından değiştirilen izinler, kullanıcı **yeniden giriş yaptığında** etkili olur.
- Kullanıcı yönetebilen en az bir aktif hesap her zaman korunur: son yönetici pasife
  alınamaz, rolü değiştirilemez, rolünün yetkisi kaldırılamaz.

---

## Veri modeli

Boyut tabloları: `Cariler`, `Stoklar`, `Kullanicilar`, `Roller`, `RolIzinleri`
Hareket tabloları: `CariIslemler`, `StokHareketleri`, `Faturalar`, `FaturaSatirlari`

Tüm ana tablolar `BaseEntity`'den altı denetim alanı miras alır (`Id`, oluşturma/güncelleme
tarihi ve kullanıcısı, `Aktif`). Bu alanlar `SaveChanges` içinde otomatik doldurulur.

**Bakiye ve stok miktarı kolonda tutulmaz, hesaplanır:**

```
Cari bakiyesi = Açılış + Satış faturaları + Ödemeler − Alış faturaları − Tahsilatlar
Stok miktarı  = Girişler − Çıkışlar + Sayım farkları + Alış faturaları − Satış faturaları
```

Her iki formül de yalnızca `Aktif` kayıtları sayar. Bunun pratik sonucu: bir fatura iptal
edildiğinde stok ve cari hareketlerini geri almak için ayrı kod gerekmez, kayıt hesaba
girmeyi bıraktığı an her iki değer kendiliğinden düzelir.

---

## Kurulum

Gereksinimler: **.NET 10 SDK**, **SQL Server 2019+**

```bash
git clone https://github.com/efeince0/on-muhasebe.git
cd on-muhasebe
```

`OnMuhasebe.WebUI/appsettings.json` içindeki bağlantı cümlesini kendi ortamınıza göre
düzenleyin, sonra:

```bash
dotnet run --project OnMuhasebe.WebUI
```

Uygulama ilk açılışta bekleyen migration'ları uygular, rolleri, izin matrisini ve bir
yönetici hesabı oluşturur. `DemoVerisi:Ekle` ayarı açıksa örnek cari, stok, fatura,
hareket ve kullanıcı kayıtlarını da ekler.

### Hesaplar

| Kullanıcı | Şifre | Rol |
|---|---|---|
| `admin` | `Admin!2345` | Yönetici |
| `muhasebe` | `Muhasebe123` | Muhasebe |
| `depo` | `Depo12345` | Depo |
| `bakis` | `Bakis12345` | Görüntüleyici |

Yönetici dışındaki hesaplar yalnızca demo verisiyle birlikte oluşur.
Gerçek kurulumda `DemoVerisi:Ekle` değerini `false` yapın ve ilk girişten sonra
yönetici şifresini değiştirin.

### Veritabanı betiği

Şemanın tamamını üreten SQL betiği:

```bash
dotnet ef migrations script --idempotent --project OnMuhasebe.DataAccess --startup-project OnMuhasebe.WebUI --output veritabani.sql
```

`--idempotent` betiği tekrar tekrar çalıştırılabilir yapar: her migration'ın uygulanıp
uygulanmadığını kontrol edip yalnızca eksikleri işler.

---

## Şartname dokümanından bilinçli sapmalar

| Konu | Dokümanda | Bu projede | Gerekçe |
|---|---|---|---|
| Sayım miktarı | `Miktar > 0` | Sayım satırlarında negatif olabilir | Sayım düzeltmesi hem artırır hem azaltır; kullanıcı sayılan miktarı girer, fark hesaplanır. Check constraint sayımı istisna tutar. |
| Satır tutarı | "Miktar × birim fiyat (+KDV)" | KDV hariç | Dokümanın kendi `AraToplam` tanımı ("KDV hariç satır toplamları") ancak böyle tutarlı olur. |
| Vergi no | Tabloda kısıt yok | Filtreli benzersiz indeks | İş kuralları bölümü "vergi no benzersiz olmalıdır" diyor. SQL Server unique index'te NULL'ları eşit saydığı için boş olanlar filtre dışı bırakıldı. |
| Pasife alma | "Pasife alınmalıdır" | Bakiye/stok engel değil, uyarı | Doküman pasife almayı çıkış yolu olarak tanımlıyor; engellemek o yolu kapatırdı. |
| Son yönetici | "Son yönetici silinememelidir" | Rol değiştirme ve rol yetkisi de korunur | Rolü değiştirmek de o hesabı yönetici olmaktan çıkarır; kural rol adına değil `Kullanici.Guncelle` yetkisine bağlandı. |
