<div align="center">

<img src="Assets/assassinbullet-icon.png" alt="AssassinBullet Logo" width="120" />

# AssassinBullet

**Bloklarla oluştur. İstekleri çalıştır. Sonuçları takip et.**

C# ve Avalonia ile geliştirilen, yapılandırılabilir HTTP iş akışları için masaüstü uygulaması.

![C#](https://img.shields.io/badge/C%23-512BD4?style=for-the-badge)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge)
![Avalonia](https://img.shields.io/badge/Avalonia-12.1.1-8B5CF6?style=for-the-badge)
![MVVM](https://img.shields.io/badge/Architecture-MVVM-0EA5E9?style=for-the-badge)

[Özellikler](#özellikler) · [Kurulum](#kurulum) · [Kullanım](#kullanım) · [Proje Yapısı](#proje-yapısı)

</div>

---

## Genel Bakış

AssassinBullet; HTTP istekleri, yanıt kontrolleri, veri ayrıştırma ve dosyaya yazma adımlarını tek bir yapılandırmada birleştirir.

Görsel config editöründe oluşturulan bloklar sırayla çalıştırılır. Her iş kendi değişken bağlamını kullanır; bir adımda elde edilen değerler sonraki adımlara aktarılabilir. Yapılandırmalar JSON olarak saklanır.

## Özellikler

- **Görsel config editörü** — Farklı blok türlerini bir araya getirerek iş akışları oluşturma.
- **HTTP istekleri** — GET ve POST istekleri, özel başlıklar ve istek gövdesi tanımlama.
- **Yanıt önizlemesi** — Editörden istek göndererek yanıt gövdesini ve başlıklarını inceleme.
- **Değişken kullanımı** — Sabit ve ayrıştırılmış değerleri iş akışı içinde paylaşma.
- **Metin ayrıştırma** — İki ayraç arasındaki içeriği alıp değişkene kaydetme.
- **Yanıt kontrolleri** — Gövde veya başlık içeriğine göre başarısızlık ve retry sınıflandırması.
- **Eşzamanlı çalışma** — Ayarlanabilir worker sayısıyla birden fazla işi işleme.
- **Proxy desteği** — Dosyadan proxy listesi yükleme ve isteklerde dönüşümlü kullanma.
- **Canlı takip** — İşlem sayaçlarını ve dosyaya yazılan sonuçları arayüzde görüntüleme.
- **Dosya çıktısı** — Değişkenlerle oluşturulan metinleri seçilen klasöre kaydetme.
- **Durdurma kontrolü** — Çalışan işlemler için iptal desteği.

## Bloklar

| Blok | Görevi |
| :--- | :--- |
| `HttpRequest` | HTTP isteği gönderir ve yanıtı işlem bağlamında saklar. |
| `KeyCheck` | Son yanıtın gövdesinde veya başlıklarında tanımlanan metinleri arar. |
| `Variable` | İşlem bağlamına isimlendirilmiş bir değer ekler. |
| `Parse` | Yanıt gövdesinden veya bir değişkenden iki ayraç arasındaki metni çıkarır. |
| `FileWrite` | Metin şablonundaki değişkenleri yerleştirerek sonucu dosyaya yazar. |

### Örnek İş Akışı

```text
HTTP Request
     │
     ▼
  Key Check
     │
     ▼
    Parse
     │
     ▼
 File Write
```

Bloklar config içindeki sıralarına göre yürütülür. Örneğin bir `Parse` bloğuyla elde edilen değer, daha sonra bir `FileWrite` bloğunda kullanılabilir.

### Değişken Şablonları

Dosya çıktılarında değişkenler `<degiskenAdi>` biçiminde kullanılır:

```text
Ürün: <productName>
Durum: <status>
```

İşlem bağlamında bu değerler bulunuyorsa çıktı şu şekilde oluşturulur:

```text
Ürün: Demo
Durum: Hazır
```

## Kurulum

### Gereksinimler

- .NET 10 SDK
- Projenin kaynak kodu

### Çalıştırma

Proje klasöründe aşağıdaki komutları çalıştır:

```bash
dotnet restore
dotnet build
dotnet run --project AssassinBullet.csproj
```

### Release Çıktısı

```bash
dotnet publish AssassinBullet.csproj -c Release -o publish
```

Oluşturulan uygulama dosyaları `publish` klasörüne yazılır.

## Kullanım

1. **Config editörünü aç.** İş akışında kullanacağın blokları oluştur ve alanlarını doldur.
2. **Config’i kaydet.** Oluşturulan JSON dosyaları uygulamanın `Configs` klasöründe saklanır.
3. **Ana ekranda config seç.** Çalıştırmak istediğin yapılandırmayı yükle.
4. **Girdi listesini yükle.** Arayüzdeki `Combolist` alanından iki alanlı metin dosyanı seç.
5. **Hedef adresi ayarla.** Ana ekran URL’sini kullanan HTTP blokları için adresi gir.
6. **Çalışma seçeneklerini belirle.** Worker sayısını ve çıktı klasörünü seç; gerekiyorsa proxy listesi ekle.
7. **İşlemi başlat.** Sayaçları ve sonuçları arayüzden takip et.

### Girdi Biçimi

Girdi dosyasında her satır `:` ile ayrılmış iki alan içermelidir:

```text
item01:tr
item02:en
```

Ana ekran URL’sindeki ilk iki yer tutucu, sırasıyla bu alanlarla eşleştirilir:

```text
http://localhost:5000/items/<itemId>?lang=<language>
```

URL’de yer tutucu bulunmadığında alanlar `Param1` ve `Param2` adlarıyla aktarılır.

## Çalışma Yapısı

AssassinBullet, işleri sınırlı kapasiteli bir `System.Threading.Channels` kuyruğundan geçirir. Ayarlanan sayıdaki asenkron worker bu kuyruktan iş alır.

Her iş için ayrı bir `Context` oluşturulur. Bu bağlam, değişkenleri ve son HTTP yanıtını tutar. Config blokları aynı iş içinde sıralı çalışırken farklı işler eşzamanlı ilerleyebilir.

Dosyaya yazma işlemleri `SemaphoreSlim` ile sıraya alınır.

## Teknolojiler

| Teknoloji | Kullanım |
| :--- | :--- |
| C# / .NET 10 | Uygulama ve iş akışı altyapısı |
| Avalonia UI | Masaüstü arayüzü |
| CommunityToolkit.Mvvm | ViewModel, özellik bildirimleri ve komutlar |
| Newtonsoft.Json | Config serileştirme ve yükleme |
| System.Threading.Channels | İş kuyruğu ve asenkron tüketiciler |
| HttpClient | HTTP iletişimi |

## Proje Yapısı

```text
AssassinBullet/
├── Assets/                  # Logo ve uygulama ikonları
├── Channel/                 # İş kuyruğu, üretici ve tüketiciler
├── Models/                  # Bloklar, config ve işlem bağlamı
├── Services/                # İstek, ayrıştırma, config ve dosya servisleri
├── ViewModels/              # Arayüz durumu ve komutlar
├── Views/                   # Ana ekran ve config editörü
├── App.axaml                # Uygulama kaynakları ve tema
├── Program.cs               # Başlangıç noktası
└── AssassinBullet.csproj    # Proje ayarları ve bağımlılıklar
```

## Mevcut Davranışlar

- HTTP yürütücüsü `GET` ve `POST` metotlarını destekler.
- `Parse`, sol ve sağ ayraçlar arasında metin çıkarır.
- `Retry` eşleşmesi ilgili sayacı artırır ve mevcut işi sonlandırır; otomatik yeniden kuyruğa alma uygulanmaz.
- Başarı sayacı, başarıyla tamamlanan dosyaya yazma işlemlerinde artar.
- Config dosyaları JSON biçiminde işlenir.

## Katkıda Bulunma

Hata bildirimlerini ve geliştirme önerilerini issue olarak paylaşabilirsin. Hata bildirirken yeniden üretme adımlarını, beklenen davranışı ve gerçekleşen sonucu ekle.

Katkı göndermek için projeyi fork’la, değişikliğini ayrı bir branch üzerinde hazırla ve pull request aç.
