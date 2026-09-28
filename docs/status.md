# Durum ve devam notu

## 28 Eylül 2026 GitHub arşivi

Kaynak kod v0.3 durumunda: güç barı kaldırıldı, güç/yükseliş mantığı korundu, kayma ile rakibi düşürme ve kalkma eklendi. Kullanıcı 12 Eylül'de bu davranışları oynayarak onayladı.

## Doğrulama sınırları

- v0.2 Windows EXE derlemesi tamamlandı, otomatik fizik/yeniden başlama/gol/eleme/şut testinde `ARENA_SMOKE_PASS` alındı; kamera görüntüleri kontrol edildi.
- v0.3 kaynak kodunun Unity kütüphanelerine karşı bağımsız derleme kontrolü geçti. Düşme testleri kaynakta mevcut; son v0.3 EXE otomatik testinin geçtiğine dair kayıt yoktur.
- Taşıma katmanı bir host + üç istemci ile aynı bilgisayarda bağlantı, girişler, yayın ve host kapanması testini geçti. Bu gerçek internet oynanış testi değildir.
- Arşivlenen Windows EXE ve görüntüler v0.2'ye aittir. Kaynak dosyasının son değişikliği 12 Eylül 13:41, mevcut EXE içindeki oyun derlemesi 13:35'tir.

## Sonraki adım

Güncel kaynakla Unity'de Windows derlemesi alınacak, `-arenaSmoke` ile düşme testleri çalıştırılacak ve v0.3 Windows paketi yayınlanacak. Sonrasında arkadaşlarla dört bilgisayarda bağlantı ve oynanış testi yapılabilir.

## Yerel otomasyon kısıtı

Codex tarafından başlatılan Unity işlemi lisans servisine bağlanamadığı için önceki Windows derlemeleri kullanıcının açık Unity editöründeki Build Windows Test menüsü üzerinden alındı. Bu, projenin taşınabilirliğini etkilemez; başka makinede uygun Unity lisansı ve Windows build desteği gerekir.

Ham günlüklerde yerel makine yolları/lisans tanılama verileri bulunduğu için bunun yerine bu özet saklanmıştır.
