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
- Rolü pasife alınmış kullanıcı giriş yapamaz.
- Aktiflik durumu **formdan** değiştirilemez; yalnızca listedeki *Pasife Al* (`Sil` izni)
  ve *Aktif Yap* (`Güncelle` izni) düğmeleriyle değişir. Aksi hâlde `Güncelle` izni olan
  biri formdaki kutucukla `Sil` iznini atlayabilirdi.
- `Ekle` ve `Güncelle` POST'ları aynı servis metodunu çağırır; hangisinin çalışacağını
  `Id` belirler. Formdan gelen `Id` ile action'ın izni uyuşmazsa istek reddedilir.

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

## Loglama

Doküman "teknik hatalar loglanmalı" diyor. Harici bir pakete (Serilog vb.) ihtiyaç
duymadan bunu karşılamak için `OnMuhasebe.WebUI/Logging/DosyaLoggerProvider.cs`
adında küçük, bağımlılıksız bir `ILoggerProvider` yazıldı:

- Konsolun yanına, `Warning` ve üzeri seviyedeki log kayıtlarını çalışma dizinindeki
  `Loglar/hata-yyyyMMdd.log` dosyasına da yazar (bu klasör `.gitignore`'da, depoya
  girmez).
- `Program.cs`'de `builder.Logging.AddProvider(...)` ile kayıt edilir; ASP.NET Core'un
  `UseExceptionHandler` middleware'i beklenmeyen her hatayı zaten `Error` seviyesinde
  logladığı için, ek bir kod yazmaya gerek kalmadan bu hatalar artık kalıcı dosyaya da
  düşer. Hata sayfasında gösterilen `RequestId` ile log satırındaki zaman damgası
  eşleştirilerek ilgili hata bulunabilir.
- Dosya yazımı `try/catch` ile korunur: loglama başarısız olsa bile uygulama akışı
  bozulmaz.

---

## Şartname dokümanından bilinçli sapmalar

| Konu | Dokümanda | Bu projede | Gerekçe |
|---|---|---|---|
| Sayım miktarı | `Miktar > 0` | Sayım satırlarında negatif olabilir | Sayım düzeltmesi hem artırır hem azaltır; kullanıcı sayılan miktarı girer, fark hesaplanır. Check constraint sayımı istisna tutar. |
| Satır tutarı | "Miktar × birim fiyat (+KDV)" | KDV hariç | Dokümanın kendi `AraToplam` tanımı ("KDV hariç satır toplamları") ancak böyle tutarlı olur. |
| Vergi no | Tabloda kısıt yok | Filtreli benzersiz indeks | İş kuralları bölümü "vergi no benzersiz olmalıdır" diyor. SQL Server unique index'te NULL'ları eşit saydığı için boş olanlar filtre dışı bırakıldı. |
| Pasife alma | "Pasife alınmalıdır" | Bakiye/stok engel değil, uyarı | Doküman pasife almayı çıkış yolu olarak tanımlıyor; engellemek o yolu kapatırdı. |
| Son yönetici | "Son yönetici silinememelidir" | Rol değiştirme ve rol yetkisi de korunur | Rolü değiştirmek de o hesabı yönetici olmaktan çıkarır; kural rol adına değil `Kullanici.Guncelle` yetkisine bağlandı. |
| Stok kategorisi | Tabloda "Liste" (sabit seçenekler) | Serbest metin + öneri listesi (`datalist`) | Doküman kategori için örnek bir liste vermiyor (sadece "ürün kategorisi/grubu" diyor); hangi kategorilerin var olacağı işletmeye göre değişir. Sabit bir enum yazmak yerine, mevcut kayıtlardan öneri listesi üretildi; serbest yazmak da mümkün. |
| Stok birimi | Tabloda "Liste" (sabit seçenekler) | Seçim kutusu (8 yaygın birim: Adet/Kg/Litre/Metre/M2/Paket/Kutu/Koli) + "Diğer" ile serbest giriş | Doküman örnek değerler veriyor ("Adet / Kg / Litre / Kutu vb."); form artık gerçek bir `<select>` sunuyor, ama "vb." ifadesi listeyi kapalı görmediği için "Diğer" seçeneğiyle listede olmayan bir birim de girilebiliyor. Veritabanında hâlâ serbest metin (`nvarchar`) — enum olsaydı yeni bir birim eklemek yeniden derleme gerektirirdi. |

---

## Bilinen sınırlar

Bu maddeler proje kapsamında bilerek çözülmedi. Gerçek bir kurulumda ele alınmaları gerekir.

| Konu | Durum | Neden şimdilik böyle |
|---|---|---|
| Yetki iptali | İzinler girişte bir kez çözülüp çerezde taşınır. Kullanıcının rolü veya yetkisi değişirse, mevcut oturum 8 saatlik çerez süresi dolana ya da kullanıcı çıkış yapana kadar eski yetkilerle çalışır. | Her istekte veritabanından yetki doğrulamak (`ValidatePrincipal`) çözer ama her sayfa açılışına bir sorgu ekler. Ekranlarda bu davranış yazıyor: yetki kaydedildiğinde "değişiklik yeniden giriş yapıldığında etkili olur" uyarısı çıkar. |
| Eşzamanlılık | Stok yeterliliği ve "son yönetici" kontrolü okuma ile `SaveChanges` arasında başka bir isteğin araya girmesine karşı korunmuyor. İki eşzamanlı çıkış kaydı stoğu negatife düşürebilir. | Doğru çözüm satır sürümü (`rowversion`) veya `SERIALIZABLE` işlem düzeyi. Tek kullanıcılı ön muhasebe senaryosunda pratik bir etkisi yok, kapsam dışı bırakıldı. |
| Otomatik test | Birim/entegrasyon testi yok; doğrulama elle yapıldı. | Zaman kısıtı. Test yazılacak olsa ilk sıra hesaplanan bakiye ve miktar formüllerinde olurdu: girdisi belli, çıktısı tek sayı. |
