using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombbaKonsol : MonoBehaviour {

    Vector3 gidilecekNoktaPOS; // Gidilecek nokta pozisyonları
    Vector3 bombbaPos; // Bombacının bulunduğu pozisyon
    float randomX; // Gideceği noktanın random verilen X değeri
    float randomY; // Gideceği noktanın random verilen Y değeri
    GameObject gidilecekNoktaObje; // Ray çizdirebilmek için oluşturduğumuz noktalarda bulunan obje
    BombbaDurum durum = BombbaDurum.noktaBulmadi; // Durum almak için enum.

    RaycastHit2D noktayaRay; // Noktaya çizilen Ray
    RaycastHit2D karaktereRay; // Karaktere çizilen ray
    Vector3 noktayaMesafe; // Bombacı Karakter ile nokta arası mesafe
    Vector3 karaktereMesafe; // Bombacı karakter ile oyuncu arası mesafe
    GameObject karakter; // Oyuncu
    [SerializeField] LayerMask layerMask; // Ray dan bombacının colliderını çıkarmak için layermask

    Rigidbody2D fizik; // Fizik
    float hareketMesafe; // Noktaya yaklaştığımızı görmek için mesafe
    float randomBeklemeSüresi; // Noktada bekleme random süresi
    float beklemeSüresi; // Noktada bekleme süresi
    bool  oyuncuGörüldüKontrol=false; // Oyuncu görülmediyse nokta aramak için kontrol

    Vector3 hedefPos; // Duruma göre değişen dönüş yönü pozisyonu
    Vector3 hedefMesafe; // Dönüş için alınanm mesafe
    float dönüsAcisi; // Dönüş için alınan tan açısı
    Quaternion rotasyon; // Dönüşü atadığımız quaternion.

    [SerializeField] Sprite[] bombbaSprite;
    SpriteRenderer spriteRenderer;
    int spriteSayac;
    float animZaman;

    GameObject bombaEfekti;
    [SerializeField] GameObject dumanEfekti; // Flare i içine atadık.
    [SerializeField] GameObject patlamaEfekti;
    [SerializeField] GameObject[] uzuvlar;
    [SerializeField] GameObject kan;
    int uzuvSayac;
    

    void Start () {

        gidilecekNoktaPOS = new Vector3();

        gidilecekNoktaObje = new GameObject("BombbaGidilecekNokta"); // Gidilecek nokta objesi oluşturduk 
        gidilecekNoktaObje.AddComponent<CircleCollider2D>(); // Collider atadık
        gidilecekNoktaObje.GetComponent<CircleCollider2D>().isTrigger = true; // Is trigger yaptık
        gidilecekNoktaObje.GetComponent<CircleCollider2D>().radius = 0.15f; // Yarım açısını değiştirdik.

        fizik = GetComponent<Rigidbody2D>();

        karakter = GameObject.FindGameObjectWithTag("Player"); // Oyuncuyu bulduk.

        spriteRenderer = GetComponent<SpriteRenderer>();

        bombaEfekti = transform.Find("BombaEfekt").gameObject;

        fizik.angularVelocity = 0; // Nesnelere çarpıp açısal hız almasını engelledik.
    }
	
	
	void FixedUpdate () {

        if(durum != BombbaDurum.oyuncuGörüldü) // Eğer oyuncu görülmediyse nokta bul
        {
            NoktaBul();
            
        }
        BombbaHareket();
        KaraktereRayCiz();
        HedefeDönüs();
        Animasyon();

        dumanEfekti.transform.position = bombaEfekti.transform.position; // Duman efekti rotasyonu almasın diye farklı bir yerde oluşturduk ve takip ettirdik.
    }
    private void OnDrawGizmos() // Gideceğimiz noktalara bir gizmos çizdirdik görebilmek amacıyla.
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gidilecekNoktaPOS, 0.5f);
    }

    private void NoktaBul()
    {
        if (durum == BombbaDurum.noktaBulmadi) // Eğer karakter nokta bulamadı ise
        {
            bombbaPos = transform.position;
            randomX = Random.Range(2, 7); // Random X ve Y noktaları bulduk
            randomY = Random.Range(2, 7);

            gidilecekNoktaPOS.Set(bombbaPos.x + Random.Range(-randomX, randomX), bombbaPos.y + Random.Range(-randomY, randomY), 0); // Ve gidilecek pozisyonumuzu belirledik

            gidilecekNoktaObje.transform.position = gidilecekNoktaPOS; // Noktaya ray çizdirebilmek için bir obje oluşturup collider verdik. Bunun pozisyonunuda random atadığımız gidilecek noktalara eşitledik.

            NoktayaRayCiz();
        }
        
    }
    private void NoktayaRayCiz() // Bombacı karakterin durumlarını yapabilmek ve mesafeleri alabilmek için karakterden nokta objesinin colliderına bir ray çizdirdik.
    {
        noktayaMesafe = (gidilecekNoktaPOS-transform.position).normalized;
        noktayaRay = Physics2D.Raycast(transform.position, noktayaMesafe, 1000, layerMask);
        Debug.DrawLine(transform.position, noktayaRay.point);

        if(noktayaRay && noktayaRay.collider.name == gidilecekNoktaObje.name) // Eğer önümüzde hiçbir nesne yok ve rayimiz nokta objesinin colliderına değiyorsa 
        {
            durum = BombbaDurum.noktaBuldu; // Durum nokta buldu olur ve o noktaya gitmeye başlar.
        }
    }
    private void KaraktereRayCiz() // Karakteri gördüğümüzde [if(karaktereRay && karaktereRay.collider.tag == "Player")] ona gidebilmek için karaktere bir ray çizdirdik. Karakteri görmediği sürece ise başka noktalarda gezinecek.
    {
        karaktereMesafe = (karakter.transform.position - transform.position).normalized;
        karaktereRay = Physics2D.Raycast(transform.position, karaktereMesafe,1000,layerMask);
        Debug.DrawLine(transform.position, karaktereRay.point,Color.magenta);

        if(karaktereRay && karaktereRay.collider.tag == "Player") // Eğer karakteri gördüyse
        {
            durum = BombbaDurum.oyuncuGörüldü; // Oyuncuya hareket eden if statementını çalıştırır.
            oyuncuGörüldüKontrol = true; // Metod update de çağrıldığı için playerı görmediğinde durumu bir kere nokta bulmadı yapmak amacıyla bool tanımladık
        }
        else
        {
            if (oyuncuGörüldüKontrol)
            {
                durum = BombbaDurum.noktaBulmadi;
                oyuncuGörüldüKontrol = false;
            }
        

        }
    }
    private void BombbaHareket() // Bombacı karakterin hareketlerinin işlendiği metod.
    {
        if (durum == BombbaDurum.oyuncuGörüldü) // Eğer karaktere çizilen ray karaktere denk geldiyse durum oyuncu görüldü olarak değişir
        {
            fizik.position += (Vector2)(karakter.transform.position- transform.position).normalized * Time.fixedDeltaTime * 5; // bombacının pozisyonunu karakter ile kendisi arasındaki vektöre eşitledik.
        }
        else // Eğer karaktere çizilen ray oyuncuya denk gelmediyse
        {
            if(durum == BombbaDurum.noktaBuldu) // Noktaya çizdirdiğimiz ray ordaki objeyi buldu ise durum nokta buldu olur. [if(noktayaRay && noktayaRay.collider.name == gidilecekNoktaObje.name)]
            {
                fizik.position += (Vector2)(gidilecekNoktaPOS - transform.position).normalized * Time.fixedDeltaTime*10; // bombacının pozisyonunu nokta ile kendisi arasındaki vektöre eşitledik.
                hareketMesafe = Vector2.Distance(transform.position, noktayaRay.point); // Noktaya yaklaştığımızı anlamak için rayın nokta objesinin collidere değdiği nokta ile pozisyonumuz arasındaki mesafeyi bulduk.
                if (hareketMesafe < 1f) // Eğer noktaya yaklaştıysak:
                {
                    durum = BombbaDurum.noktadaBekle; // Noktada bekle durumuna geçer
                    randomBeklemeSüresi = Random.Range(0.5f, 4.0f); // random bekleme süreleri atadık
                }
            }
            else if (durum == BombbaDurum.noktadaBekle) // Eğer noktayı buldu ve yaklaştı ise bombacı:
            {
                beklemeSüresi += Time.fixedDeltaTime; // Bekleme süresi yaptık
                if (beklemeSüresi >= randomBeklemeSüresi) // Bekleme süresini bitirince
                {
                    durum = BombbaDurum.noktaBulmadi; // Yeni bir nokta bulması için tekrar durumu nokta bulmadi yaptık.
                    beklemeSüresi = 0;
                }
            }
          
            
        }
    }
    private void HedefeDönüs() // Hangi yöne gidiyorsa oraya dönmesi için yazılan metod.
    {
        if (durum == BombbaDurum.oyuncuGörüldü) // Eğer oyuncuyu gördü ise gidilecek pozisyonu oyuncunun pozisyonuna eşitlliyoruz
        {
            hedefPos = karakter.transform.position;
        }
        else // Eğer oyuncuyu görmediyse gideceği noktayı random atadığımız nokta objesinin pozisyonuna eşitliyoruz.
        {
            hedefPos = gidilecekNoktaObje.transform.position;
        }
        hedefMesafe = (transform.position - hedefPos).normalized; // Duruma göre değişen pozisyon ile bombacının pozisyonunun farkını alıp mesafeyi bulduk.
        dönüsAcisi = Mathf.Atan2(hedefMesafe.y, hedefMesafe.x) * Mathf.Rad2Deg; // Tanjant açısını bulup bunu dereceye çevirdik.
        rotasyon = Quaternion.Euler(0, 0, dönüsAcisi); // Z eksenine bu rotasyonu verdik.
        rotasyon *= Quaternion.Euler(0, 0, 90); // Ufak bir ayarlama yaptık dönüş doğru olsun diye.
        transform.rotation = Quaternion.Lerp(transform.rotation, rotasyon, 0.1f); // Rotasyonu bombacının rotasyonuna atadık ve biraz yumuşak bir dönüş için Quaternion lerp kullandık.
    }

    private void Animasyon() // Bombacının yürüme animasyonunun olduğu metod.
    {
        if (durum != BombbaDurum.noktadaBekle) // Eğer karakter beklemiyorsa
        {
            animZaman += Time.fixedDeltaTime; // Spriteları belirli zaman aralığında değiştirmek için animasyon zaman atadık.
            if (animZaman > 0.1f) // her 0.1 snde bir içeri gir
            {
                spriteRenderer.sprite = bombbaSprite[spriteSayac++]; // Spriteları oynat
                if (spriteSayac >= bombbaSprite.Length)
                {
                    spriteSayac = 0;
                }
                animZaman = 0;
            }
        }
        else // Eğer karakter bekliyorsa
        {
            spriteRenderer.sprite = bombbaSprite[0]; // İlk sprite ı al
            spriteSayac = 0;
            animZaman = 0;
        }
    }
    public void BombbaIntihar() // Karakterin on trigger metodundan çağrılır ve karakterin colliderına değince patlama efekti verir.
    {
        var patlamaEfekt=Instantiate(patlamaEfekti, transform.position, Quaternion.identity); // Patlama efektini oluştur.
        dumanEfekti.SetActive(false); // Patlamadan önce olan duman efektini kapatır
        DontDestroyOnLoad(patlamaEfekt); // Karakter yok olduğunda patlama efekti çalışsın diye yok etmeme metodunu kullandık
        Destroy(patlamaEfekt, 5); // 5 sn sonra yok olur

        for(int i=0; i<(Random.Range(1,3)); i++) // Kan spriteı oluşturur.
        {
            var kanEfekt = Instantiate(kan, transform.position, Quaternion.Euler(0, 0, (Random.Range(0, 360))));
            DontDestroyOnLoad(kanEfekt); // Karakter yok olduğunda kan kalsın diye kullandık
            Destroy(kanEfekt, 15); // 15 sn sonra yok olur.
        }
       

        for (int i=0; i<uzuvlar.Length; i++) // Patlama sonrası bombacının vucüt parçalarının savrulduğu metod.
        {
            var uzuv=Instantiate(uzuvlar[i], transform.position, Quaternion.Euler(0, 0,(Random.Range(0, 360)))); // Uzuvlar sırayla oluşur ve döner
            Vector2 uzuvlarVec = new Vector2(Random.Range(-1000, 1000), Random.Range(-1000, 1000)); // Savrulması için vektör
            uzuv.GetComponent<Rigidbody2D>().AddForce(uzuvlarVec); // Add force ile savrulur
            Destroy(uzuv, Random.Range(5, 7)); // 5 ile 7 sn arasında yok olur
            if (uzuvlar[i].tag != "kafaTag") // 2 Kol 2 bacak yapmak için kafa dışında tüm uzuvları 2. kere oluşturduk
            {
               var uzuv2= Instantiate(uzuvlar[i], transform.position, Quaternion.Euler(0, 0, (Random.Range(0, 360))));
               uzuv2.GetComponent<Rigidbody2D>().AddForce(uzuvlarVec);
               Destroy(uzuv2, Random.Range(5, 7));
            }
        }

       
        Destroy(gameObject); // Bombacı karakteri yok eder.
    }
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.tag == "karakterKursunTag") // Mermi değdiğinde patlaması için.
        {
            BombbaIntihar();

        }  
    }

}
/* Bombacı karakterin 4 durumu var. Eğer oyuncuyu görmediyse nokta bulup o noktaya gitmesi ve o noktada beklemesi gerek bunun için enum oluşturduk */


public enum BombbaDurum 
{
    noktaBuldu=1,
    noktaBulmadi=2,
    noktadaBekle=3,
    oyuncuGörüldü=4

}
