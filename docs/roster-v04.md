# 10 karakterli kadro — 2 Ekim 2026

## Karakterler
Atlas, Kaya, Lina, Bora, Mira, Efe, Zara, Aras, Deniz ve Rüzgar. Referansların stilize futbolcu yaklaşımına göre özgün yüz, saç, ten, forma deseni ve renk varyasyonları oluşturuldu. Silah yoktur.

- On FBX: `Assets/Resources/RosterModels/`
- Düzenlenebilir Blender kaynakları: `SourceArt/Roster/`
- Katalog: `Assets/Resources/Roster.json`
- Asset üretimi: `tools/build_roster.py`
- Unity'de `Kaos Arena > Prepare Ten Character Assets` menüsü görsel prefabları `Assets/Characters/RosterPrefabs/` altına kaydeder. Windows derlemesi bu adımı otomatik çalıştırır.

## Eşit oyun koşulları
Her FBX'in görünen boyu 1,95 metreye eşitlendi. Bütün karakterler aynı 1,85 metre yüksekliğinde, 0,32 metre yarıçapında CharacterController kullanır. FBX'lerde oynanış collider'ı yoktur. Karakter kataloğunda hız, güç veya erişim mesafesi alanı yoktur.

Önceki sürümün FixedUpdate, ShotVelocity, CheckSlideHits, Goal ve ResetBall işlevleri kaynak karşılaştırmasında aynı kaldı. Koşu hızı, sıçrama, kayma süresi, düşme süresi, şut fiziği, gol ve yeniden başlama değişmedi.

## Seçim ve forma rengi
Lobide 10 seçenek ve dönen 3D önizleme bulunur. Seçim değiştirilince hazır durumu kapanır. Maç içinde karakter değiştirilemez. Aynı karakteri ilk seçen slot standart forma rengi alır; diğerleri host tarafından uygun alternatiflere atanır. Aynı lobby durumunda renkler sabit kalır. Karakter ve forma kimlikleri tüm istemcilere gönderilir.

Tüm oyuncular bu sürümü kullanmalıdır; ağ mesaj sürümü eski paketlerle karışmasını önlemek için değiştirildi.

## Animasyon
Mevcut transform animasyonları korunup görsel diz/dirsek bükülmesi, hızla yumuşayan koşu ve hafif gövde hareketi eklendi. Kayma ve düşmede bacak pozları temizlenir. Bunlar henüz mocap veya Mecanim Humanoid animasyon klipleri değildir.

## Yapılan kontroller
- On FBX yeniden Blender'a içe aktarıldı: tamamı 1,95 metre, gerekli 8 eklem/pivot mevcut.
- On karakterin toplu görseli render edilip incelendi.
- 10.000 dört oyunculu seçim kombinasyonu ve 16 eksik lobi durumunda forma benzersizliği doğrulandı.
- Yeniden hesaplamada forma renklerinin sabit kaldığı doğrulandı.
- Oyun ve editör kaynakları Unity kütüphanelerine karşı hatasız derlendi.
- Unity'nin gerçek model importu, lobi önizlemesi, EXE ve dört bilgisayarlı internet testi henüz bekliyor. Kullanıcının Unity editöründe Build Windows Test adımı gerekli.

`-arenaSmoke` testi yeni model boyu, collider eşitliği ve tekrar karakter renk kontrolünü içerir. Yeni EXE üretilmeden eski Windows paketlerini bu sürüm diye dağıtmayın.
