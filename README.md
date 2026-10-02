# Kaos Arena

Dört oyunculu, dört köşe kaleli Unity futbol prototipi. Üç gol yiyen oyuncu elenir; son kalan kazanır. Bir oyuncunun bilgisayarı host olarak çalışır.

## Güncel kaynak: v0.3

- Mahalle sahası, geniş ve derin fileli dört kale, renkli temel karakterler.
- Hareket, zıplama/kafa vuruşu, kayma ve kayarak rakibi düşürme.
- Sol tık basılı tutuldukça artan şut gücü; şut hazırlanırken fareyi aşağı çekerek yükseliş ayarı. Görsel güç barı kaldırıldı.
- Normal golde herkes köşesine döner, top gol yiyenden başlar. Başlangıçta ve elemede top merkezden bırakılır.
- Host otoriteli maç, üç istemci bağlantısı, hazır olma ekranı, botlu antrenman ve izleyici modu.

## Açma ve Windows derlemesi

Unity **2022.3.21f1** ile repo kökünü açın. Menüden **Kaos Arena → Build Windows Test** seçin. Bu işlem sahneyi oluşturur ve `Build/KaosArena.exe` üretir. Saha ve karakterler oyun çalışırken kodla oluşturulur. Grafik altyapısı Built-in Render Pipeline'dır.

Dağıtım için yalnızca EXE'yi değil, `Build` klasörünün tamamını gönderin.

## Hazır indirmeler

[GitHub Releases](https://github.com/berkayozdmr21/kaos_arena/releases) bölümündeki **Windows-v02** paketi önceki test derlemesidir: bar kaldırma ve rakibi düşürme değişikliklerini içermez. En güncel v0.3 kaynak burada korunmaktadır; v0.3 Windows paketi yeniden derlenmelidir. Eski kaynak arşivi de geçmiş kayıt olarak ayrıca saklanmıştır.

## Bağlantı

Bir oyuncu lobi kurar, diğer üçü hostun IP adresine bağlanır. TCP portu **7777**. Farklı evlerden bağlantı için port yönlendirme veya ortak sanal ağ gerekir. Steam/relay ve internet üzerinden dört kişiyle kapsamlı test henüz tamamlanmadı. Host çıkınca maç iptal olur.

## Kontroller

WASD hareket; fare kamera/yön; sol tık basılı tutup bırakma şut; şut hazırlanırken fare aşağı hareketi yükseliş; sağ tık kayma; Space zıplama; Tab izleyici kamerası; Esc menü.

## Belgeler ve testler

- [Tasarım belgesi](docs/game-design.md)
- [Geliştirme geçmişi ve kullanım notları](docs/development-notes.md)
- [Doğrulama ve kaldığımız yer](docs/status.md)
- [v0.2 saha ve animasyon görüntüleri](docs/previews/)

Ağ katmanı testi: .NET 6 SDK ile `dotnet run --project tests/NetworkTest.csproj`.

Unity EXE testi: `Build/KaosArena.exe -batchmode -nographics -arenaSmoke -logFile smoke.log`.

Unity önbellekleri, makineye özel ayarlar, ham günlükler ve tekrar üretilebilir geçici dosyalar repoya dahil edilmez. Karakterler henüz nihai modeller değildir; diğer sahalar, özelleştirme ve Steam yayını sonraki geliştirme kapsamındadır.

## Karakter model dosyaları

Dört karakterin FBX modelleri ve düzenlenebilir Blender kaynakları eklendi. [Karakter assetleri ve kullanım](docs/character-assets.md).


## 0.4 — On farklı futbolcu

10 FBX model, Blender kaynakları, lobi karakter seçimi ve çakışmayan forma renkleri eklendi. [Güncel değişiklikler ve test durumu](docs/roster-v04.md). Yeni Windows EXE derlemesi ve oyun içi doğrulama bekliyor; mevcut v0.2 Windows paketi bu karakterleri içermez.

