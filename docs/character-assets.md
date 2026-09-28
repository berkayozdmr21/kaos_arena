# Karakter model assetleri

Dört karakter artık gerçek model dosyaları olarak da mevcuttur:

- `Assets/Characters/Models/Player_Turquoise.fbx`
- `Assets/Characters/Models/Player_Coral.fbx`
- `Assets/Characters/Models/Player_Gold.fbx`
- `Assets/Characters/Models/Player_Purple.fbx`

FBX dosyalarını Unity Project panelinden sahneye sürükleyebilirsiniz. Materyal renkleri dosyalara dahildir. Her karakterin ayrı kol/bacak pivotları ve mesh parçaları vardır; sırt numarası da mesh geometrisidir. Oyuncu adı etiketi kullanıcı verisi olduğu için modele gömülmemiştir.

## Düzenleme

`SourceArt/Characters/` altında her karakterin düzenlenebilir `.blend` dosyası bulunur. Unity'nin Blender kurulumuna bağımlı olmaması için bu kaynaklar `Assets` klasörü dışında tutulmuştur.

## Prefab ve .mat oluşturma

Unity'de import tamamlandıktan sonra **Kaos Arena → Create Character Prefabs** seçin. Menü dört `.prefab`, CharacterController bileşenleri ve ayrı `.mat` dosyaları oluşturur. Çıktılar `Assets/Characters/Prefabs` ve `Assets/Characters/Materials` altına kaydedilir.

Hazır FBX modelleri repoya eklenmiştir. `.prefab` ve `.mat` çıktıları Unity menüsü çalıştırıldığında oluşur; bu otomatik üretim burada Unity lisans hatası nedeniyle çalıştırılarak doğrulanamamıştır.

## Kapsam

Bunlar mevcut prosedürel karakterlerin aynı ölçü, renk ve parça düzenine dayanan Blender mesh karşılıklarıdır. Unity'nin yerleşik primitive meshleriyle verteks düzeyinde aynı değildir. Kemikli Humanoid rig, skinning veya gömülü animasyon klipleri içermezler. Koşma, şut, kayma ve düşme kodu `ArenaGame.cs` içinde korunur. Oyun henüz bu FBX dosyalarını kullanacak şekilde değiştirilmemiştir; prefablar bağımsız kullanım ve sonraki entegrasyon içindir.

## Yeniden üretim ve doğrulama

Blender 4.4 ile repo kökünde:

```
blender --background --factory-startup --python tools/export_characters.py
blender --background --factory-startup --python tools/validate_character_assets.py
```

Doğrulama scripti dört FBX'i yeniden içe aktarır; parça sayısı, boy ve materyallerini kontrol edip önizleme üretir. Sonuçlar `docs/character-asset-validation.json`, görsel `docs/previews/character-assets.png` içindedir.

FBX dışa aktarma seçenekleri: https://docs.blender.org/api/main/bpy.ops.export_scene.html
