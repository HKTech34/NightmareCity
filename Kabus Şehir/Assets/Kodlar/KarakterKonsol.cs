using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KarakterKonsol : MonoBehaviour {

    //Karakter hareket
    private float yatay;
    private float dikey;
    private Rigidbody2D fizik;
    private Vector2 hareketVec;
    [SerializeField] float karakterHızı;

    // Karakteri mouse yönüne döndürme
    private Vector3 fareHareketPos;
    Camera kamera;
    Vector3 karakter_mouse_Mesafe;
    float karakterYönAcısı;
    Quaternion rotasyon;

    // Fare imleci oluşturma
    GameObject fare;

    //Kamera hareketi
    Vector3 kameraPos;

    //Ray Cizdirme
    RaycastHit2D karakterRay;
    [SerializeField] LayerMask layerMask;
    GameObject nisangah;
    RaycastHit2D nisangahRay;

    //Ates etme
    [SerializeField] GameObject karakterKursun;
    private bool atesKontrol = false;
    float atesAraligi=0.1f;

    //Animasyon
    private Animator animator;
    int karakterCephane=120;
    bool sarjörAnim=false;
    float sarjörZaman;

    //Bacak Animasyonları
    public bacakAnim bacakAnimasyon;
    int ileriGeribacakSpriteSayac;
    int sagSolbacakSpriteSayac;

    //Şarjör ayarları
    int şarjör = 40;
    [SerializeField] TextMeshProUGUI cephane_sarjörTMP;

    void Start () {

        fizik = GetComponent<Rigidbody2D>();
        hareketVec = new Vector2();

        kamera = Camera.main; // Ana kamerayı kamera objemize attık.

        fare = GameObject.FindGameObjectWithTag("FareTag"); // Fare imlecini koyduğumuz objeyi bulduk.

        kameraPos = new Vector3(); // Kamera hareketi için vektör atadık

        nisangah = GameObject.FindGameObjectWithTag("NisangahTag"); // Nişangah objemizi bulduk.

        animator = GetComponent<Animator>();

        fizik.angularVelocity = 0; // Nesnelere çarpıp açısal hız almasını engelledik.
    }
	void Update ()
    {

        if(karakterCephane>0 || şarjör > 0) // Eğer karakterin mermisi varsa
        {

            if (Input.GetMouseButtonDown(0)) // Eğer farenin sol tuşuna basılı ise
            {
                atesKontrol = true; // Ateş kısmını tetikleyen bool doğru oluyor.
            }
            else if (Input.GetMouseButtonUp(0)) // Parmağımızı kaldırdığımızda
            {
                atesKontrol = false;
                atesAraligi = 0.1f; // Parmağımızı kaldırdığımızda ates aralığı 0.1 den küçük kalmış olabilir tekrar ateş edebilmek için 0.1 e eşitledik.
            }

            if (Input.GetKeyDown(KeyCode.R) && karakterCephane>0) // Eğer cephane varsa R tuşu ile mermi ekleme
            {
                sarjörAnim = true;
                SarjKontrol(); // Şarj ile alakalı kontroller yapılır
            }
        }
        else
        {
            atesKontrol = false; // Eğer mermi yok ise ateş etme
        }

        cephaneTextGöster();
    }
    void FixedUpdate()
    {
        KarakterKontrol();
        FareYönAl();
        FareGöstergesi();
        KameraHareket();
        RayCizdir();
        KarakterAtesEtme();
        KarakterAnimasyon();
    }
    private void KarakterKontrol()
    {
        // W S A D Değerlerini aldık ve karakteri hareket ettirdik.
        yatay = Input.GetAxisRaw("Horizontal");
        dikey = Input.GetAxisRaw("Vertical");
        hareketVec.Set(yatay, dikey);
        hareketVec.Normalize(); //Aynı anda W ile D ye bastığımızda oluşan değerin hipotenüsü alındığı için karakter hızlanır,Bunu önlemek amacıyla normalize metodunu kullandık.
        fizik.velocity = hareketVec*karakterHızı;
    }
    private void FareYönAl()
    {
        fareHareketPos = kamera.ScreenToWorldPoint(Input.mousePosition); // Fare pozisyonunu oyun ekranı pozisyonuna çevirdik.
        fareHareketPos.Set(fareHareketPos.x, fareHareketPos.y, transform.position.z); // Z sini sabit tutmak için değiştirdik.
        karakter_mouse_Mesafe = fareHareketPos - transform.position; // Fare ile karakter arası vektörü aldık.
        karakterYönAcısı = Mathf.Atan2(karakter_mouse_Mesafe.y, karakter_mouse_Mesafe.x) * Mathf.Rad2Deg; // Fare ile karakter arasında oluşan vektörün arasındaki tanjant radyanını bulup bunu açıya çevirdik.
        rotasyon = Quaternion.Euler(0, 0, karakterYönAcısı); // Karakteri z yönünde döndürmek için dönüş vektörü oluşturduk
        rotasyon*= Quaternion.Euler(0, 0, 270); // Başlangıcımız 0 derecesinde olduğu için mouse yönüne döndürdük.
        fizik.transform.rotation = rotasyon; // Karakteri döndürdük.
    }
    private void FareGöstergesi()
    {
        fare.transform.position = fareHareketPos; // Fare göstergesini farenin olduğu yere eşitledik.
    }
    private void KameraHareket()
    {
        kameraPos.Set(transform.position.x, transform.position.y, -10); // Kamera pozisyonunu X Y ekseninde karakterin pozisyonuna eşitledik.
        kamera.transform.position =Vector3.Lerp(kamera.transform.position, kameraPos ,0.1f); // Kameranın sonradan gelmesi için Lerp metodunu kullandık.

    }
    private void RayCizdir()
    {
        karakterRay = Physics2D.Raycast(transform.position, karakter_mouse_Mesafe,1000,layerMask); //Karakterin olduğu yerden farenin olduğu yerdeki imlecimizin colliderına ray çizdiriyoruz.Karakterin colliderı engel olmasın diye layerMask ekledik.
        Debug.DrawLine(transform.position, karakterRay.point,Color.red); // Ray i görmek için çizgi çizdik.

        // Mesafe yakınken silahı çeviremediğimiz için silah yamuk atışlar yapacak bunu önlemek için mesafe kısa iken ileri çizdirmesini sağladık.
        if (Vector3.Distance(transform.position,karakterRay.point) > 5)  // Mesafe uzak ise
        {
            nisangahRay = Physics2D.Raycast(nisangah.transform.position, (fareHareketPos - nisangah.transform.position), 1000, layerMask); //Fare imlecine çizdir.
            Debug.DrawLine(nisangah.transform.position, nisangahRay.point, Color.blue);
        }
        else //Yakın ise
        {
            nisangahRay = Physics2D.Raycast(nisangah.transform.position, nisangah.transform.up , 1000, layerMask); //İleri çizdir.
            Debug.DrawLine(nisangah.transform.position, nisangahRay.point, Color.blue);
        }
       
    }
    private void KarakterAtesEtme()
    {
        atesAraligi += Time.deltaTime; // Ateş aralığı yapmak için time.deltatime a eşitledik.

        if (atesKontrol && !sarjörAnim) // Sarjör animasyonu çalışmıyorsa ve mouse basılı ise
        {
            if (atesAraligi > 0.1f) // Her 0.1 snde bir 
            {
                Instantiate(karakterKursun, nisangah.transform.position, rotasyon); // Kurşun prefabını oluştur
                atesAraligi=0; // Sıfırla 
                şarjör--;
                if (şarjör == 0) // Eğer şarjör 0 ise
                {
                    if (karakterCephane != 0) // Cephanede mermi varsa
                    {
                        SarjKontrol(); // Sarjörü doldur
                        sarjörAnim = true; // Sarjör animasyonunu çalıştır(atış animasyonu durur)
                    }
                    
                }
            }
          
        }

    }
    public Vector2 getKursunPos()
    {
        return (nisangahRay.point - (Vector2)nisangah.transform.position).normalized; // Ve kurşunu nişangahtan çıkan rayin olduğu noktaya gönder
    }
    private void KarakterAnimasyon()
    {
        if (!sarjörAnim) // Eğer şarjör animasyonu çalışmıyorsa ateş animasyonunu kontrol et
        {
            if (!atesKontrol) //  Eğer ateş animasyonu çalışmıyorsa yürüme animasyonunu kontrol et
            {
                animator.SetBool("atesTrigger", false); // Ateş animasyonu false olunca durağan animasyonu çalışır
                if (yatay != 0 || dikey != 0) // Eğer hareket varsa 
                {
                    animator.SetBool("yürümeTrigger", true); // Yürüme animasyonunu çalıştır
                }
                else
                {
                    animator.SetBool("yürümeTrigger", false); // Yürüme animasyonunu durdur (durağan animasyon çalışır)
                }
            }
            else // Eğer ateş kontrol true ise (Mouse button down)
            {
                animator.SetBool("atesTrigger", true); // Ateş animasyonunu oynat.
            }
        }
        else // Şarjör deişiyorsa (Keydown R)
        {
            animator.SetBool("sarjörTrigger", true); // Şarjör animasyonu çalışır
            sarjörZaman += Time.deltaTime; // animasyonun bitmesini bekle
            if (sarjörZaman > 1)
            {
                animator.SetBool("sarjörTrigger", false); // Şarjör animasyonunu durdur(Diger animasyonlara geçer)
                sarjörAnim = false; // Şarjör animasyonunu durdur(Diger animasyonlara geçer)
                sarjörZaman = 0;
            }
        }

        // ---------------------------------------------------------------------------Bacak Animasyonları---------------------------------------------------------------------//

        if(dikey==0 && yatay == 0) // Eğer hareket yoksa
        {
            bacakAnimasyon.getbacakSR().sprite = bacakAnimasyon.ileriGeriAnim[5]; // Durağan bacak sprite ı göster.
        }
        else if (dikey > 0) // Eğer ileri gidiyorsa
        {
            bacakAnimasyon.gameObject.transform.rotation = Quaternion.Euler(0, 0, 90); // Bacaklar öne bakar
            bacakAnimasyon.getbacakSR().sprite = bacakAnimasyon.ileriGeriAnim[ileriGeribacakSpriteSayac++]; // Yürüme animasyonunu oynat
            if(ileriGeribacakSpriteSayac>= bacakAnimasyon.ileriGeriAnim.Length) // Eğer sprite array i geçerse
            {
                ileriGeribacakSpriteSayac = 0; // Başa döndür
            }
        }
        else if (dikey < 0) // Eğer geri gidiyorsa
        {
            bacakAnimasyon.gameObject.transform.rotation = Quaternion.Euler(0, 0, 270); // Bacaklar aşağı bakar
            bacakAnimasyon.getbacakSR().sprite = bacakAnimasyon.ileriGeriAnim[ileriGeribacakSpriteSayac++];// Yürüme animasyonunu oynat
            if (ileriGeribacakSpriteSayac >= bacakAnimasyon.ileriGeriAnim.Length) // Loop
            {
                ileriGeribacakSpriteSayac = 0;
            }
        }
        else if (yatay > 0) //Eğer sağa gidiyorsa
        {
            bacakAnimasyon.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0); // Bacaklar sağa bakar
            bacakAnimasyon.getbacakSR().sprite = bacakAnimasyon.sağSolAnim[sagSolbacakSpriteSayac++]; //Yürüme animasyonu oynar
            if (sagSolbacakSpriteSayac >= bacakAnimasyon.sağSolAnim.Length) //Loop
            {
                sagSolbacakSpriteSayac = 0;
            }
        }
        else if (yatay < 0) // Eğer sola gidiyorsa
        {
            bacakAnimasyon.gameObject.transform.rotation = Quaternion.Euler(0, 0, 180); // Bacaklar sola bakar
            bacakAnimasyon.getbacakSR().sprite = bacakAnimasyon.sağSolAnim[sagSolbacakSpriteSayac++];//Yürüme animasyonu oynar
            if (sagSolbacakSpriteSayac >= bacakAnimasyon.sağSolAnim.Length) //Loop
            {
                sagSolbacakSpriteSayac = 0;
            }
        }

    }
    private void SarjKontrol()
    {
        if (karakterCephane > 40) // Eğer karakterin cephanesi 40 tan büyükse şarjörü fulle ve cephaneden 40-sarjördeki mermi kadar çıkar(Örnek: Cephane 80 şarjör 5 -> Cephane 45 sarjör 40)
        {
            karakterCephane -= 40 - şarjör;
            şarjör = 40;
        }
        else // Eğer cephane 40 tan küçük ise
        {
            if (karakterCephane + şarjör >= 40) // Toplam mermi 40 tan büyük ise şarjörü fulle, Cephaneye toplam mermi-40 kadar mermi bırak(Örnek: Cephane 20 şarşör 25 -> cephane 5 şarjör 40)
            {
                karakterCephane = karakterCephane + şarjör - 40; 
                şarjör = 40;
            }
            else // Eğer toplam mermi 40 tan küçük ise Tüm mermiyi şarjör e ver, Cephaneyi sıfırla.
            {
                şarjör += karakterCephane;
                karakterCephane = 0;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if (collision.gameObject.tag == "CephaneTag" && karakterCephane!=120) // Eğer cephane kutusu bulursa karakter.
        {
            karakterCephane += collision.GetComponent<Cephane>().cephaneDeger; // Cephaneye atadığımız değer kadar karakter cephanesine ekler.
            if (şarjör == 0) // Sarj boş ise otomatik şarj depiştirmesi için sarj kontrol çalışır.
            {
                sarjörAnim = true;
                SarjKontrol();
            }
            Destroy(collision.gameObject); // Cephane kutusunu yok et
        }
        else if (collision.gameObject.tag == "Bombba")
        {
            collision.GetComponent<BombbaKonsol>().BombbaIntihar();
        }
        if (karakterCephane>120) // Karakterin cephanesini sınırlama
        {
            karakterCephane = 120;
        }
    }
    private void cephaneTextGöster() // Cephane / Şarjör texti gösterir.
    {
        cephane_sarjörTMP.text = karakterCephane + "/" + şarjör;
    }
}
