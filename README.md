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

## Veri modeli

Boyut tabloları: `Cariler`, `Stoklar`, `Kullanicilar`, `Roller`, `RolIzinleri`
Hareket tabloları: `CariIslemler`, `StokHareketleri`, `Faturalar`, `FaturaSatirlari`

Tüm ana tablolar `BaseEntity`'den altı denetim alanı miras alır (`Id`, oluşturma/güncelleme tarihi ve kullanıcısı, `Aktif`). Bu alanlar `SaveChanges` içinde otomatik doldurulur.

---


## Kurulum

Gereksinimler: **.NET 10 SDK**, **SQL Server 2019+**

```bash
git clone https://github.com/efeince0/on-muhasebe.git
cd on-muhasebe
```

`OnMuhasebe.WebUI/appsettings.json` içindeki bağlantı cümlesini kendi ortamınıza göre düzenleyin, sonra:

```bash
dotnet ef database update --project OnMuhasebe.DataAccess --startup-project OnMuhasebe.WebUI
dotnet run --project OnMuhasebe.WebUI
```

Uygulama ilk açılışta rolleri, izin matrisini ve bir yönetici hesabı oluşturur:

```
admin / Admin!2345
```



---



