# xUnit ve Modern Test Dünyasına Giriş Rehberi 🚀

Bu doküman, .NET dünyasındaki test araçlarını, kavramlarını ve xUnit'in neden sektör standardı haline geldiğini anlaman için hazırlanmıştır.

---

## 1. Test Framework'lerin Tarihçesi: Nereden Geliyoruz?

Yazılım dünyasında test araçları bir evrim sürecinden geçmiştir. Bu süreci bilmek, bugünkü araçların neden böyle tasarlandığını anlamanı sağlar.

### 🕰️ Dönem 1: MSTest (Microsoft'un İlk Adımı)
Microsoft tarafından Visual Studio ile entegre olarak sunuldu. Çok eski ve hantaldır. Testleri çalıştırmak için Visual Studio'ya çok bağımlıdır ve modern yazılım prensiplerine (Dependency Injection gibi) uyum sağlamakta zorlanır.

### 🕰️ Dönem 2: NUnit (Topluluğun Yanıtı)
Java'daki JUnit'ten esinlenerek geliştirildi. MSTest'ten çok daha esnektir. Uzun süre sektör standardı oldu. Ancak "Test Life Cycle" (testin yaşam döngüsü) konusunda bazı karmaşıklıklara sahiptir.

### 🕰️ Dönem 3: xUnit (Modern Standart) 👑
NUnit'in orijinal yaratıcıları tarafından, geçmişteki hatalardan ders çıkarılarak **en baştan** yazıldı. 
- **Felsefesi:** "Az ama öz". 
- **En büyük farkı:** Her test metodu için sınıfın yeni bir örneğini (instance) oluşturur. Bu, testlerin birbirini etkilemesini (state pollution) %100 önler.

---

## 2. Temel Kavramlar ve Yapılar

### 🎯 [Fact] (Gerçek)
En temel test etiketidir. Bir metodun üzerinde `[Fact]` görüyorsan, bu şu demektir:
> "Bu test parametre almaz ve her koşulda doğruluğu ispatlanması gereken bir gerçektir."
*Örn: "Giriş butonu boş basıldığında hata vermelidir."*

### 🧪 [Theory] (Teori)
Aynı test senaryosunu farklı verilerle denemek istediğinde kullanılır. Yanına `[InlineData]` eklenir.
> "Bu bir senaryodur ama farklı verilerle (A, B, C) sonucun hep başarılı olmasını bekliyorum."

### ⚖️ Assert (Doğrulama/İddia)
Testin en önemli kısmıdır. Kodun çalıştıktan sonra elde ettiğin sonucun, beklediğin sonuçla aynı olup olmadığını kontrol eder.
- `Assert.Equal(beklenen, gelen)` -> Eşit mi?
- `Assert.True(koşul)` -> Doğru mu?
- `Assert.Contains("metin", gelen)` -> İçinde var mı?

---

## 3. Neden xUnit? (NUnit veya MSTest Değil?)

| Özellik | MSTest | NUnit | xUnit |
| :--- | :--- | :--- | :--- |
| **İzolasyon** | Orta | Düşük | **Çok Yüksek** (Her testte yeni class) |
| **Performans** | Yavaş | Orta | **Çok Hızlı** (Paralel çalışma) |
| **Modernlik** | Eski | Olgun | **Modern / Standart** |
| **Kullanım** | Azalıyor | Yaygın | **Zirvede** |

**xUnit'in "Gizli Silahı":**
Diğer framework'lerde `[SetUp]` ve `[TearDown]` gibi karmaşık metodlar vardır. xUnit'te ise standart C# **Constructor** (Yapıcı Metod) ve **IDisposable** kullanılır. Yani test yazarken aslında sadece standart C# yazarsın!

---

## 4. Test Yazarken "Altın Kural": AAA Modeli

İyi bir test her zaman 3 bölümden oluşur:

1.  **Arrange (Hazırlık):** Nesneleri oluştur, verileri hazırla.
2.  **Act (Eylem):** Test etmek istediğin metodu çalıştır.
3.  **Assert (Doğrulama):** Sonucu kontrol et.

*Bizim UI testlerimizde de bunu yaptık: Sayfaya gittik (Arrange), Butona bastık (Act), Hata mesajına baktık (Assert).*

---

### 📝 Son Söz
Test yazmak zaman kaybı değil, **gelecekteki hatalara karşı bir sigortadır.** xUnit öğrenmek, seni .NET ekosisteminde aranan bir geliştirici yapar. 

**Mutlu Testler!** 😊💻
