# Kaos Arena — İlk saha prototipi

Unity **2022.3.21f1**, Windows x64. Bu kaynak proje ilk test sürümüdür; tamamlanmış Steam oyunu değildir.

## Derleme

1. Unity Hub → Add project from disk ile bu klasörü ekleyin.
2. Unity 2022.3.21f1 ile açın; içe aktarma ve kod derlemesini bekleyin.
3. Üst menüde **Kaos Arena → Build Windows Test** seçin.
4. Sahne otomatik oluşturulur ve `Build/KaosArena.exe` üretilir.
5. Arkadaşlarınıza yalnızca EXE'yi değil **Build klasörünün tamamını ZIP olarak** gönderin. Herkes ZIP'i çıkardıktan sonra EXE'yi çalıştırsın.

Saha, karakterler, ışıklar ve kameralar oyun başlarken kodla oluşturulur. Editör sahnesinde başlangıçta yalnızca oyun yöneticisinin görünmesi normaldir. Bu sürüm kurulumu sade tutmak için Built-in Render Pipeline kullanır; URP geçişi henüz yapılmamıştır.

## Oynama

- Tek başına denemek için **Antrenman • 3 Bot** seçin, **Hazırım**, ardından **Maçı Başlat** düğmelerine basın.
- Dört kişi için bir oyuncu **Lobi Oluştur** seçer. Diğer üç kişi host IP adresini yazıp **Lobiye Katıl** seçer.
- Dört oyuncu da hazır olduğunda host maçı başlatır.
- WASD: hareket. Fare: kamera/yön. Sol tık: şut; basılı tutmak gücü artırır. Sağ tık: kayma. Space: zıplama/kafa teması. Esc: maç menüsü.
- Elenen oyuncu Tab ile takip/tribün kameraları arasında geçer.

## İnternet bağlantısı

Bu prototip doğrudan **TCP 7777** bağlantısı kullanır. Steam daveti, herkese açık lobi listesi, kısa davet kodu ve otomatik relay bu sürümde yoktur.

- Aynı yerel ağda hostun yerel IPv4 adresini kullanın.
- Farklı evlerden test için ortak bir sanal ağ veya hostun modeminde TCP 7777 port yönlendirmesi gerekir. Port yönlendirmesinde dış IP adresi kullanılır; CGNAT altında bu yöntem çalışmayabilir.
- Windows güvenlik duvarı bağlantıyı engelliyorsa oyuna kullanılan ağ profilinde izin verilmelidir.
- Aynı bilgisayarda dört ayrı uygulama ile testte istemciler `127.0.0.1` adresine bağlanabilir.
- Bu bağlantı sürümü yalnızca tanıdığınız arkadaşlarla prototip testi içindir; kimlik doğrulama ve internet yayınına uygun korumalar henüz tamamlanmamıştır.

## Bu sürümün kapsamı

- Tek mahalle sahası; merkeze bakan dört köşe kalesi ve fiziksel sekme bariyerleri.
- Dört renkli, geometrik karakter; prosedürel koşu, şut ve kayma pozları. Bunlar nihai rigli karakterler değildir.
- Üç can, kale sahibine gol yazılması, eleme, kapalı kale, son kalan oyuncunun kazanması.
- Host üzerinde top fiziği ve maç kuralları; istemcilere durum aktarımı.
- Host ayrılınca istemcinin menüye dönmesi; maç iptali.
- Botlu antrenman ve temel izleyici modu.

## İlk test için geçici kararlar

- Top dışarı çıktığında en yakın **elenmemiş** oyuncunun kale önünde yeniden başlatılır; o oyuncu da kale önüne alınır. Eşit uzaklıkta daha düşük kale numarası seçilir. Bu ayrıntılar kullanıcı tarafından kesinleştirilmiş kurallar değildir.
- Gol ve dışarı çıkma sonrası top 1,5 saniye bekletilir.
- Host dışındaki bir oyuncu maç sırasında ayrılırsa elenir; kalanlarla maç sürer.
- Goller için son dokunan oyuncu istatistiği bu sürümde tutulmaz; hangi kaleye girdiği can kaybını belirler.
- TCP taşıma ve görsel yumuşatma ilk prototip içindir. İnternet gecikmesinde kontrol hissi henüz ölçülmedi; istemci tahmini ve uzlaştırma uygulanmadı.
- Karakter özelleştirme, diğer dört saha, Steam entegrasyonu ve nihai animasyonlar sonraki geliştirme kapsamındadır.

## Doğrulama durumu

- Oyun ve editör C# kaynakları kurulu Unity kütüphanelerine karşı bağımsız derleme kontrolünden geçti: 0 hata / 0 uyarı.
- Ağ taşıma testi tek bilgisayarda bir host ve üç ayrı istemciyle geçti: bağlantı, üç giriş akışı, üç istemciye durum gönderimi ve host kapanışını algılama; 8 kontrol.
- Bu kontroller Unity oynanışının veya farklı evlerden internet bağlantısının doğrulandığı anlamına gelmez.
- Windows EXE derlemesi kullanıcı tarafından açılan Unity editöründe tamamlandı. Yeni EXE üzerinde yeniden başlama, fizik, can/eleme, şut gücü/yükselişi ve kayma kontrolleri geçti. Saha, kale ve kayma görüntüleri oyun kamerasıyla render edilerek kontrol edildi. Farklı bilgisayarlardan dört oyunculu maç testi henüz yapılmadı.
- Derleme sonrasında `KaosArena.exe -batchmode -nographics -arenaSmoke -logFile smoke.log` temel fizik/can/eleme kontrollerini çalıştırır; başarıda kayda `ARENA_SMOKE_PASS` yazar.

## Test geri bildirimi

Hata olduğunda ne yaptığınızı, host mu istemci mi olduğunuzu, kaç kişi bağlı olduğunu ve ekrandaki sonucu yazın. Windows oyuncu kaydı normalde `%USERPROFILE%/AppData/LocalLow/KaosArenaPrototype/Kaos Arena/Player.log` konumundadır. Bu günlük ile kullanılan proje sürümünü birlikte saklayın.

## 0.2 — Oynanış geri bildirimi güncellemesi

- Bina pencerelerinin üst nesnenin ölçeğini alıp sahaya taşması düzeltildi. Çevre aydınlatması yumuşatıldı.
- Kale genişliği 1,7 metreden 2,8 metreye çıkarıldı. File 1,8 metre derinlikte, arka/yan/tavan yüzeyleriyle oluşturuluyor.
- Kaymada tüm gövde yere yakın yatıyor; kayma yönü hareket başında sabitleniyor, hız hareket boyunca azalıyor ve karakterin çarpışma yüksekliği düşüyor. Kayma ve şut pozları ağdan aktarılıyor.
- Başlangıç ve yeniden başlama beklemesinde host oyuncu hareketlerini kilitliyor; topun serbest bırakıldığı fizik adımında hareket açılıyor.
- Her golden sonra oyuncular kendi köşelerine dönüyor. Normal golde top gol yiyen oyuncunun önüne bırakılıyor; elemede merkezden bırakılıyor. Son oyuncu kaldığında maç bitiyor.
- Şut gücü sol tık 1,25 saniye boyunca tutuldukça dolar; bırakınca vurulur. Güç barı oyun ekranında görünür.
- Şut hazırlanırken fareyi aşağı çekmek vuruş açısını artırır. Güç çıkış hızını, fare çekişi çıkış açısını belirler; uçuşu yerçekimi, sürüklenme ve çarpışmalar hesaplar. En yüksek güç ve yükseliş, topu saha dışına gönderebilir.
- Şut hazırlanırken dikey fare hareketi kamerayı oynatmaz; yükselişi ayarlar. Yukarı hareket yükseliş ayarını geri azaltır.
- Yeniden başlama kimliği ağdan gönderilir; önceki oyundan gecikmiş şut/hareket paketleri yeniden başlatmaya taşınmaz.
- Bu sürüm Windows EXE olarak derlendi. ARENA_SMOKE_PASS alındı; saha, kale ve kayma görüntüleri kontrol edildi. Bu bölüm önceki sürümün çelişen açıklamalarının yerine geçer.


## 0.3 — Barsız şut ve kayarak düşürme

Bu bölüm önceki sürümün çelişen açıklamalarının yerine geçer.

- Şut gücü/yükseliş paneli kaldırıldı. Sol tık basılı tutuldukça güç birikir; bırakınca şut çekilir. Basılı tutarken fareyi aşağı çekmek yükselişi artırmaya devam eder.
- Kayma sırasında yerdeki başka bir aktif oyuncuya temas edilirse o oyuncu yaklaşık 1,4 saniyelik yana devrilme, komik kol/bacak hareketleri ve kalkma animasyonuna girer.
- Düşen oyuncu bu kısa süre boyunca koşamaz, şut çekemez veya kayamaz; ayağa kalkınca kontrol geri gelir. Can kaybı yoktur.
- Aynı kayma aynı oyuncuyu yalnızca bir kez düşürür. Ayağa kalkışın ardından 0,65 saniyelik kısa koruma vardır; sürekli yere kilitleme önlenir.
- Yeterince yükseğe zıplamış oyuncuya alçak kayma isabet etmez. Elenmiş oyuncular hedef olmaz.
- Temas kararını host verir; düşme aşaması diğer istemcilere gönderilir. Gol sonrası yeniden başlangıç düşme durumunu temizler.
- Herkes aynı sürümü kullanmalıdır. İnternet üzerinde dört kişiyle gerçek oynanış testi ayrıca yapılmalıdır.
