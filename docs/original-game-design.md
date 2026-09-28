# KAOS ARENA FUTBOL — Teknik Tasarım ve Geliştirme Dokümanı
*(4 Kişilik Kare Sahada Eleme Usulü Co-op/Rekabetçi Online Futbol Oyunu)*

> Bu doküman Codex (veya başka bir AI kodlama ajanı) için yazılmıştır. Amaç: Unity 3D içinde açıklanan oyunu geliştirmek. Doküman aşağıda **fazlara (phase)** ayrılmıştır — tavsiye: tüm dokümanı tek seferde vermek yerine, her fazı ayrı bir görev olarak sırayla vermek çok daha sağlıklı sonuç verir (bkz. Bölüm 12).

---

## 1. OYUN ÖZETİ

**Tür:** 3D Online Multiplayer, 4 oyunculu, eleme usulü, serbest-format futbol
**Motor:** Unity (2022 LTS veya üzeri, URP)
**Perspektif:** Üçüncü şahıs (karakterin arkasından, hafif yukarıdan)
**Ağ modeli:** Host (lobiyi kuran oyuncunun bilgisayarı) = sunucu. Diğer 3 oyuncu client olarak bağlanır (listen-server / peer-hosted mimari).

### Temel Konsept
Kare şeklinde bir sahanın her köşesinde bir kale vardır. 4 oyuncu aynı anda sahaya girer, her biri kendi kalesini savunur ama istediği kaleye gol atmaya çalışabilir (kendi kalesi hariç herkes potansiyel rakip/hedef). Round başında top sahanın ortasına düşer. 3 gol yiyen oyuncu elenir, kalesi kapanır. Elenen oyuncu, kalan oyunculardan birinin gözünden (spectator/first-person-follow) veya tribün kamerasından izlemeye devam eder. Son ayakta kalan oyuncu turnuvayı/raundu kazanır.

---

## 2. TEMEL OYNANIŞ KURALLARI (Game Rules)

- **Oyuncu sayısı:** Sabit 4 (lobi 4 kişi dolmadan round başlamaz — bkz. Bölüm 3 için host'un "botla doldurma" veya "bekleme" opsiyonu tasarım kararı olarak eklenebilir, ama v1 kapsamı dışında tutulabilir).
- **Saha:** Kare, 4 köşede 4 kale. Her kale, standart bir kaleden **küçük** (minyatür kale) — tasarım hedefi: gol atmak zor değil ama hassasiyet gerektirsin.
- **Kaleci yok, eller yasak:** Hiçbir oyuncu elleriyle topa dokunamaz (kaleci rolü yok). Sadece **ayak** ve **kafa** ile topa müdahale edilir.
- **Round başlangıcı:** Top sahanın tam ortasına düşer (hafif yükseklikten serbest düşüş, fizik motoruyla zıplayarak yere oturur). Oyuncular kendi kale önlerinde başlar.
- **Serbestlik:** Bir oyuncu isterse hemen topa koşabilir, isterse kendi kalesi önünde bekleyip savunma yapabilir. Oyuncular birbiriyle sözlü/harici anlaşıp ittifak kurabilir (oyun mekaniği olarak bunu engelleyen bir şey yok — bu bilinçli bir tasarım kararı, "resmi takım" sistemi YOK).
- **Gol kuralı:** Herhangi bir oyuncu, herhangi bir kaleye (kendi kalesi hariç) gol atabilir.
- **Can sistemi:** Her oyuncunun 3 canı vardır (yediği gol sayısı). 3. golü yediğinde **elenir**.
- **Eleme sonrası:** Elenen oyuncunun kalesi kapanır (görsel olarak bir bariyer/kapı iner, artık o kaleye gol atılamaz — o alan oyun dışı kabul edilir). Diğer oyuncuların skorları/canları **sıfırlanmaz**, kaldıkları yerden devam eder.
- **Oyun sonu:** Son ayakta kalan tek oyuncu round'u kazanır.
- **İzleyici modu:** Elenen oyuncu oyun bitene kadar sahadan çıkmaz, spectator moduna geçer:
  - Kalan oyunculardan birini seçip onun 3. şahıs kamerasından izleyebilir ("X'in gözünden izle").
  - Ya da sabit "tribün" kameralarından (sahaya konumlandırılmış birkaç sabit/serbest kamera noktası) izleyebilir.
  - Spectator UI: izlenen oyuncu ismi, canlı skor tablosu (4 oyuncunun kalan can durumu).

---

## 3. LOBİ VE MENÜ AKIŞI

### 3.1 Ana Sayfa
İki buton:
- **Lobi Oluştur**
- **Lobiye Katıl**

### 3.2 Lobi Oluştur
- Host, lobiyi **Açık (Public)** veya **Gizli (Private)** olarak kurar.
- **Açık lobi:** "Lobiye Katıl" ekranındaki listede herkese görünür (lobi adı/host adı, dolu/boş oyuncu sayısı, seçili saha gösterilir).
- **Gizli lobi:** Listede görünmez. Host'a otomatik üretilmiş bir **davet kodu** (örn. 6 haneli alfanümerik, "A3F9K2" gibi) verilir. Bu kod arkadaşlarla paylaşılır.
- Lobi içinde host: saha seçimi yapabilir (5 sahadan biri, bkz. Bölüm 6), oyuncuların hazır (ready) durumunu görür, 4 kişi dolup herkes hazır olunca round'u başlatabilir.

### 3.3 Lobiye Katıl
- Ekranda güncel **açık lobiler listesi** (host adı, doluluk [x/4], harita önizlemesi).
- Ayrıca bir **"Kod ile Katıl"** alanı: kullanıcı davet kodunu girip Enter/Katıl'a basınca doğrudan o gizli lobiye bağlanır.

### 3.4 Lobi İçi Bekleme Ekranı
- Katılan her oyuncu burada **karakter seçimi ve özelleştirme** yapar (Bölüm 4).
- Oyuncular "Hazır" (Ready) durumuna geçer; host tüm oyuncular hazır olduğunda "Başlat"a basar.

---

## 4. KARAKTER SİSTEMİ

### 4.1 Karakter Seçimi
- **10 farklı temel karakter modeli**, birbirinden belirgin şekilde farklı (vücut yapısı, yüz, saç stili gibi temel farklar).
- Her karakterin şu özellikleri **oyuncu tarafından özelleştirilebilir** olmalı:
  - **Ten rengi** (birkaç seçenekten biri)
  - **Saç stili / rengi**
  - **Forma (kıyafet):** üst (forma — kısa/uzun kol seçeneği, renk), alt (şort — renk)
  - **Dövme gibi kozmetik detaylar** (opsiyonel katman, örn. sol kol dövmesi açık/kapalı)
- **Forma sırtı:**
  - **İsim:** oyuncu serbest metin girer, maksimum **10 karakter**, formanın **üst kısmında** gösterilir.
  - **Numara:** oyuncu **1–99 arası** bir sayı girer/seçer, formanın **alt kısmında** (isim altında) gösterilir.
- Karakter seçim ekranında canlı 3D önizleme (oyuncu döndürüp inceleyebilmeli) olmalı.

### 4.2 Teknik Not (Codex için)
- Karakterler tek bir **rig/iskelet** üzerine kurulmalı ki animasyonlar (koşu, şut, kayma, zıplama) tüm karakterlerde ortak çalışsın (Mecanim Humanoid rig önerilir).
- Kıyafet/saç/ten rengi değişimleri **materyal/texture swap** veya **blend shape + material color** ile yapılmalı; her kombinasyon için ayrı model gerekmemeli.
- İsim + numara, forma üzerine **runtime texture** (RenderTexture veya TextMeshPro 3D dünya nesnesi olarak forma üstüne yerleştirme) ile basılmalı.

---

## 5. KONTROLLER VE KAMERA

| Girdi | Aksiyon |
|---|---|
| **W A S D** | Karakter hareketi (koşma) |
| **Sol Tık** | Şut çekme (topa yakınken güç uygulayarak vurma; basılı tutma = şut gücü ayarı önerilir) |
| **Sağ Tık** | Kayma (slide tackle) — topa/rakibe müdahale hareketi |
| **Space** | Zıplama (kafa vuruşu için de kullanılır — havadayken topa temas kafa vuruşu sayılır) |
| **Fare hareketi** | Kamera yönü / karakter dönüşü |

- **Kamera:** Üçüncü şahıs, karakterin **arkasından ve hafif üstünden**, hareketleri (koşu animasyonu, şut, kayma) net görülecek şekilde. Kamera karakterin baktığı yöne göre yumuşak (smoothed) takip yapmalı, duvar/kale gibi engellerde çarpışma kontrolü (camera collision) olmalı.

---

## 6. SAHA TASARIMLARI (5 Farklı Harita)

Tüm sahalar **kare formatlı**, 4 köşede kale mantığı sabit; sadece **tema ve görsel çevre** değişiyor:

1. **Sahil / Kumsal Sahası** — kum zemin, dalga sesi, palmiyeler, arka planda deniz.
2. **Orman Sahası** — ağaçlarla çevrili, toprak/çim karışık zemin, doğal ışık (gün ışığı ağaçlardan süzülüyor).
3. **Gökyüzü Sahası (Balon Temalı)** — saha büyük balonlarla havaya kaldırılmış gibi, etrafta bulutlar, uçurumsu boşluk hissi (düşme riski olmasa da görsel olarak yükseklik hissi — güvenlik ağı/görünmez duvar sınırları olmalı).
4. **Mahalle Sahası (Rio/Brezilya Sokak Sahası Esintili)** — beton/asfalt zemin, etrafta grafiti duvarlar, tel örgü çit, sokak lambaları, mahalle evleri.
5. **Çatı Sahası** — bir gökdelenin çatısında, etrafta şehir manzarası ve yüksek binalar, gece şehir ışıkları.

- Saha seçimi **lobi kurulurken host tarafından** yapılır.
- Her saha aynı **oyun mantığını** (kare saha + 4 kale + fizik kuralları) paylaşır, sadece prefab/ortam/ışıklandırma/skybox farklıdır — yani teknik olarak tüm haritalar aynı temel "arena şablonu" üzerine kurulmalı (modülerlik için önemli).

### 6.1 Saha Sınırları ve Bariyer Sistemi
- Kareyi oluşturan dört kenarın **kaleler dışında kalan tüm bölümlerinde** bariyer (duvar) bulunur.
- Top sahadan sadece iki şekilde çıkabilir: **(1)** bir kaleye gol olarak girerse, **(2)** bariyerlerin **üstünden** havadan (yüksek vuruş/şut ile) çıkarsa. Bunların dışında top asla saha dışına çıkmamalı, her zaman bariyerden geri sekmeli.
- Bariyere çarpan topun sekme yönü **sabit/scriptlenmiş bir yön DEĞİL**, topun geliş açısına bağlı gerçekçi bir fizik sekmesi olmalı (angle of incidence = angle of reflection mantığı). Teknik olarak: bariyerler **BoxCollider + uygun bir Physic Material** (bounciness/friction ayarlı) ile oluşturulmalı; Unity'nin fizik motoru sekme yönünü kendisi hesaplasın, elle kod ile yön zorlanmamalı.
- Bariyer yüksekliği: kale yüksekliğinin **yarısı** kadar olmalı (bkz. Bölüm 8.1 — kale ölçüleri).

---

## 7. FİZİK VE TOP SİSTEMİ

- Top: Unity **Rigidbody + SphereCollider**, gerçekçi zıplama/yuvarlanma (physic material: uygun sürtünme ve sekme/bounciness değerleri ayarlanmalı).
- Şut mekaniği: Sol tık ile ayakla vuruş; vuruş gücü ve yönü karakterin bakış açısı + (opsiyonel) basılı tutma süresine göre hesaplanmalı.
- Kafa vuruşu: Karakter zıplama (space) sırasında topla temas ederse farklı bir vuruş animasyonu/kuvvet vektörü uygulanmalı.
- Kayma (sağ tık): Kısa mesafeli hızlı hareket + topa dokunursa topu kontrolden çıkarma/kapma şansı; rakip oyuncuya değerse (opsiyonel) hafif itme efekti.
- Round başı top spawn: Sahanın tam merkezinde, belirli bir yükseklikten serbest bırakılıp yere düşmesi.

---

## 8. KALE VE GOL SİSTEMİ

### 8.1 Kale Ölçüleri
- **Kale yüksekliği:** Karakter boyundan **biraz daha uzun** olmalı.
- **Kale eni (genişliği):** Karakter yan çevrilip yan yana **iki tanesi** konulduğunda kapladığı genişlik kadar olmalı (yani kale eni ≈ karakterin yandan görünen eni × 2).
- **Bariyer yüksekliği:** Kalenin **dikey uzunluğunun (yüksekliğinin) yarısı** kadar olmalı (bkz. Bölüm 6.1).
- Bu üç ölçü birbirine bağlı olduğundan, Codex'e görev verirken önce karakterin nihai boy/en değerlerini (capsule collider boyutları) sabitlemesini, kale ve bariyer ölçülerini bu değerlerden **oransal olarak** türetmesini istemen önerilir (sabit sayı yerine formül/oran kullanımı, ileride karakter modeli değişirse ölçülerin otomatik uyum sağlaması için önemlidir).

- Her kale: standart kaleden **küçük ölçekli** bir prefab, içinde **trigger collider** (gol çizgisi algılama).
- Top bu trigger'a girince: golü atan oyuncu tespit edilir (son dokunan oyuncu / son vuran oyuncu takip edilmeli — "last touch" sistemi), kalenin sahibi bir can kaybeder.
- Kale sahibi 3. golü yediğinde:
  - Kale görsel olarak **kapanır** (bariyer/kapak animasyonu).
  - O oyuncu **elenir**, spectator moduna geçirilir (Bölüm 2).
- Top, elenen bir oyuncunun kapalı kalesine çarparsa normal duvar gibi sekmeli (gol sayılmamalı).

---

## 9. UI EKRANLARI (Gerekli Ekranlar Listesi)

1. Ana Sayfa (Lobi Oluştur / Lobiye Katıl)
2. Lobi Oluştur ekranı (Açık/Gizli seçimi, saha seçimi)
3. Lobiye Katıl ekranı (açık lobi listesi + kod ile katıl kutusu)
4. Lobi bekleme odası (oyuncu listesi, hazır durumları, karakter özelleştirme paneli)
5. Karakter özelleştirme ekranı (ten/saç/kıyafet/isim/numara + 3D önizleme)
6. Oyun içi HUD (kendi canın, diğer 3 oyuncunun canı/skoru, mini harita opsiyonel)
7. Eleme ekranı → Spectator moduna geçiş arayüzü (izlenecek oyuncu seçimi / tribün kamerası butonu)
8. Round/Oyun sonu ekranı (kazanan oyuncu, skor özeti, "Lobiye Dön" butonu)

---

## 10. AĞ MİMARİSİ (Networking) — Codex İçin Teknik Karar Önerisi

> Bu bölüm bir **öneri**dir; Codex'e görev verirken bu kısmı netleştirip vermen gerekir çünkü mimari seçimi tüm kod yapısını etkiler.

- Açıklanan model ("lobiyi kuranın bilgisayarı sunucu olacak") klasik bir **listen-server (host + client)** mimarisidir.
- Önerilen teknoloji: **Unity Netcode for GameObjects (NGO)** + **Unity Relay & Lobby servisleri** (NAT sorunlarını aşmak ve açık lobi listeleme/kod ile katılma özelliğini hazır servisle çözmek için idealdir — "lobi listesi" ve "kod ile katılma" zaten Unity Lobby servisinin desteklediği özellikler).
- Alternatif: **Mirror Networking** (ücretsiz, açık kaynak) + kendi yazılacak bir lobi/relay çözümü — Relay servisiniz olmadan host'un port yönlendirmesi (port forwarding) gerekebilir, bu da son kullanıcı için ek karmaşıklık demektir. Bu riski Codex'e görev verirken belirtmen önerilir.
- Senkronize edilmesi gereken veriler: oyuncu pozisyon/rotasyon/animasyon state'i, top'un Rigidbody durumu (NetworkTransform + physics authority host'ta olmalı), can/skor durumu, kale açık/kapalı durumu, karakter özelleştirme verileri (tüm client'lara yayılmalı).

---

## 11. KAPSAM UYARISI (Gerçekçi Beklenti Notu)

Bu, **tam teşekküllü bir 3D online multiplayer oyun** — profesyonel stüdyolarda aylar süren bir kapsam. Bir AI kodlama ajanına (Codex dahil) bunu tek seferde "yap" demek gerçekçi sonuç vermez. Önerilen yaklaşım, aşağıdaki fazları **ayrı ayrı, sırayla** görev olarak vermek:

## 12. AŞAMALI GÖREV LİSTESİ (Codex'e Sırayla Verilecek Fazlar)

**FAZ 1 — Temel Prototip (Tek Oyunculu, Ağsız)**
Kare saha + 4 kale prefabı, WASD hareket, şut/kayma/zıplama kontrolleri, top fiziği, tek kale için gol/can/eleme mantığı, basit 3. şahıs kamera. Ağ yok, sadece local test.

**FAZ 2 — 4 Oyunculu Lokal Mantık**
Faz 1'i 4 kaleye/4 can sistemine genişletme, "last touch" gol atfetme, eleme sonrası kale kapanma, round kazanma koşulu. Hâlâ tek makinede (4 oyuncu AI/klavye simülasyonuyla test edilebilir).

**FAZ 3 — Networking Entegrasyonu**
NGO (veya seçilen çözüm) ile host/client mimarisi, oyuncu senkronizasyonu, top fizik otoritesi, temel lobi (oda kurma/katılma, henüz UI'siz, kod tabanlı test).

**FAZ 4 — Lobi Sistemi ve UI**
Ana sayfa, lobi oluştur (açık/gizli + kod üretimi), lobiye katıl (liste + kod girişi), bekleme odası.

**FAZ 5 — Karakter Sistemi**
10 karakter modeli entegrasyonu, özelleştirme (ten/saç/kıyafet), isim/numara sistemi (runtime texture/TMP), karakter seçim ekranı.

**FAZ 6 — Spectator Sistemi**
Eleme sonrası oyuncu takip kamerası + tribün kamerası, spectator HUD.

**FAZ 7 — Saha Çeşitliliği**
Modüler arena şablonunu 5 farklı temaya (sahil, orman, gökyüzü, mahalle, çatı) uyarlama, saha seçim sistemi.

**FAZ 8 — Cilalama (Polish)**
Animasyon geçişleri, ses efektleri, VFX (gol kutlaması, şut efekti), UI/UX iyileştirme, bug fixing.

---

### Codex'e Görev Verirken Kullanabileceğin Örnek Açılış Cümlesi:
*"Aşağıda bir futbol oyununun tam tasarım dokümanı var. Şu anda sadece [FAZ X] üzerinde çalışmanı istiyorum — diğer fazları görmezden gel, sadece bağlam için oku. [FAZ X'in içeriğini buraya yapıştır]"*

Bu şekilde Codex'e dokümanın tamamını referans olarak verip, her seferinde hangi fazda olduğunu net belirtirsen çok daha sağlıklı ve yönetilebilir kod üretimi alırsın.
