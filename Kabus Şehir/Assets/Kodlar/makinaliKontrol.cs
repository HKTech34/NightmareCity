using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class makinaliKontrol : MonoBehaviour {

    [HideInInspector] public bool atesKontrol=false; // Ateş alanı içine girdiğinde true olur. Ates alanı kontrolden çağrılır.

    //Makinalı düsman rotasyon tanımları:
    GameObject anaKarakter;
    private Vector3 makinali_karakter_mesafe;
    float dönmeAcisi;
    Quaternion rotasyon;

    //Makinalı düsman ateş etme tanımları:
    private GameObject nisangah;
    private GameObject nisangahYon;
    [SerializeField] GameObject makinaliKursun;
    float atisAraligi;

    // Animasyon tanımlamaları
    SpriteRenderer spriteRenderer;
    public Sprite[] makinaliAtesAnim;
    private int atesAnimSayac;
    private float animZaman;
    public Sprite[] makinaliSarjorAnim;
    private int sarjorAnimSayac;
    private bool sarjorKontrol=false;

    //Can barı
    public Image canBar;
    [SerializeField] [Range(0,100)] int canUzunlugu;
    private float can;

    //Barikat ve boşkova tanımlamaları
    [SerializeField] private GameObject barikat;
    [SerializeField] private GameObject bosKovan;
    [SerializeField] private GameObject bosKovanPos;

    //Makinalı düşman patlama efekt tanımlamaları
    [SerializeField] GameObject kan;
    [SerializeField] GameObject patlamaEfekt;
    [SerializeField] GameObject[] makinaliCeset;

    void Start () {

        anaKarakter = GameObject.FindGameObjectWithTag("Player");
        nisangah = transform.Find("MakinaliNisangah").gameObject;
        nisangahYon = nisangah.transform.Find("MakinaliNisangahYon").gameObject;
        spriteRenderer = GetComponent<SpriteRenderer>();

        can = canUzunlugu;

        barikat.transform.parent = transform.parent;
	}
	
	void FixedUpdate () {

        MakinaliDönme();
        MakinaliAtes();
        MakinaliAnimasyon();

        canBar.fillAmount = can / canUzunlugu;  // Canı yüz verirsek en başta can uzunlugu 100 oluyor ve can azaldıkça 99/100 98/100.. diye azalıp oranlıyor.
    }
    private void MakinaliDönme()
    {
        if (atesKontrol) // Karakter ateş alanı içine girince
        {
            makinali_karakter_mesafe = (transform.position - anaKarakter.transform.position).normalized; // Karakter ile makinaşı arasındaki mesafeyi al
            dönmeAcisi = Mathf.Atan2(makinali_karakter_mesafe.y, makinali_karakter_mesafe.x)*Mathf.Rad2Deg; // Mesafe vektörünün tanjant açısını al
            rotasyon =Quaternion.Euler(0, 0, dönmeAcisi); // Rotasyona ekle
            rotasyon *= Quaternion.Euler(0, 0, 180); // Düzgün dönüş almak için 180 ile çarptık
            transform.rotation = Quaternion.Lerp(transform.rotation, rotasyon, 0.1f); // 0.1 sn farkla o noktaya dön
        }
    }
    private void MakinaliAtes() 
    {
        atisAraligi += Time.fixedDeltaTime;
        if (atesKontrol && atisAraligi>0.1f && !sarjorKontrol) // Sarjör animasyonu çalışmıyor ve oyuncu atış alanı içine girdiyse
        {
           Instantiate(makinaliKursun, nisangah.transform.position, transform.rotation * Quaternion.Euler(0, 0, 90)); // Kurşun at
           var bosKovanlar=Instantiate(bosKovan, bosKovanPos.transform.position, transform.rotation * Quaternion.Euler(0, 0, 90)); // Boş kovan oluştur
           bosKovanlar.GetComponent<Rigidbody2D>().AddForce(-GetKursunPos()*1000); // kurşunun tam tersinden kuvvet uygula
           Destroy(bosKovanlar, Random.Range(2, 5)); // kovanı yok et
           atisAraligi = 0;
        }
    }
    public Vector2 GetKursunPos() // Makinalı kurşun scriptinden çağırdığımız yön metodu.
    {
        return (nisangahYon.transform.position - nisangah.transform.position).normalized;
    }
    private void MakinaliAnimasyon()
    {
        if (!sarjorKontrol) // Sarjör kontrol çalışmıyorsa
        {
            animZaman += Time.fixedDeltaTime;
            if (atesKontrol && animZaman >0.05f) // animasyonu zamanladık ve karakter atış alanında ise girmesini sağladık
            {
                spriteRenderer.sprite = makinaliAtesAnim[atesAnimSayac++]; // Atış animasyonunu çalıştır
                if (atesAnimSayac == makinaliAtesAnim.Length)
                {
                    atesAnimSayac = 0;
                    animZaman = 0;
                    sarjorKontrol = true; // Mermi atış sprite ı son a geldiğinde sarjör kontrol true olur ve sarjör animasyonu tetiklenir.
                }

                animZaman = 0;
            }
        }
        else // Sarjör kontrol çalışıyorsa a
        {
            animZaman += Time.fixedDeltaTime;
            if (atesKontrol && animZaman > 0.05f)
            {
                spriteRenderer.sprite = makinaliSarjorAnim[sarjorAnimSayac++]; // Sarjör animasyonunu çalıştır
                if (sarjorAnimSayac == makinaliSarjorAnim.Length) // Sarjör animasyonu bitince
                {
                    sarjorAnimSayac = 0;
                    animZaman = 0;
                    sarjorKontrol = false; // Atış animasyonuna geçmek için sarjör kontrol false olur
                }
                animZaman = 0;
            }
           
        }
      
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "karakterKursunTag") // Eğer makinalı karakter kurşununa değerse
        {
            can--; // Can azalır ve böylelikle image.fill amount azalır ve resim yavaşça kaybolur.
            if (can <= 0)
            {
                var patlama=Instantiate(patlamaEfekt, transform.position, Quaternion.identity);
                DontDestroyOnLoad(patlama);
                Destroy(patlama, 5);

                var cesetIndex = Random.Range(0, makinaliCeset.Length);  // Random bir ceset sprite ı oluştur
                var cesetler=Instantiate(makinaliCeset[cesetIndex], transform.position, Quaternion.identity);
                DontDestroyOnLoad(cesetler);
                Destroy(cesetler, 10);

                for (int i = 0; i < (Random.Range(1, 3)); i++) // Kan spriteı oluşturur.
                {
                    var kanEfekt = Instantiate(kan, transform.position, Quaternion.Euler(0, 0, (Random.Range(0, 360))));
                    DontDestroyOnLoad(kanEfekt); // Karakter yok olduğunda kan kalsın diye kullandık
                    Destroy(kanEfekt, 15); // 15 sn sonra yok olur.
                }

                Destroy(barikat);
                Destroy(gameObject);
            }
        }
    }
}
