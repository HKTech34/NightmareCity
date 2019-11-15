using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AnaDüsmanKonsol : MonoBehaviour {

    //Düsman hareket tanımlamaları
    private GameObject gecisBolgesi;
    private GameObject birinciBolge;
    private GameObject ikinciBolge;
    private GameObject ucuncuBolge;
    private GameObject anaDüsman;
    private int randomBolge=0;
    private int randomTur;
    private int turSayac=0;
    private int bolgeIndex;
    private bool noktayaGitti=false;
    BolgeKontrol bolgeKontrol = BolgeKontrol.noktayaGecis;

    //Karakter hareket tanımları
    private Vector3 yeniPos;
    private float mesafe;

    //Ray çizme tanımlamaları
    RaycastHit2D raycastHit2D;
    [SerializeField] LayerMask layerMask;
    GameObject anaKarakter;
    bool rayKaraktereDegdi = false;
    Vector3 rotasyonelPozisyon;

    // Ateş etme tanımlamaları
    [SerializeField] GameObject kursun;
    private GameObject nisangah;
    private GameObject nisangahYon;
    private float atisAraligi;
    bool kursunYonKontrol=true;

    public float can;
    [SerializeField] [Range(0, 100)] int canUzunlugu;
    public Image canBar;
    public GameObject[] anaDüsmanCeset;
    public GameObject anaDüsmanKan;

    private SpriteRenderer spriteRenderer;
    public Sprite[] yürümeAnim;
    private int yürümeIndex;
    private float yürümeAnimAraligi;
    public Sprite[] atesEtmeAnim;
    private int atesIndex;
    private float atesAnimAraligi;


    void Start () {

        anaDüsman = transform.Find("AnaDüsman").gameObject;
        gecisBolgesi = transform.Find("GecisBolgesi").gameObject;
        birinciBolge = transform.Find("BirinciBolge").gameObject;
        ikinciBolge = transform.Find("IkinciBolge").gameObject;
        ucuncuBolge = transform.Find("UcuncuBolge").gameObject;

        anaKarakter = GameObject.FindGameObjectWithTag("Player");
        nisangah = anaDüsman.transform.Find("Nisangah").gameObject;
        nisangahYon = anaDüsman.transform.Find("NisangahAtesYon").gameObject;

        can = canUzunlugu;

        spriteRenderer = anaDüsman.GetComponent<SpriteRenderer>();
    }
	void FixedUpdate () {

        RayCizdir();
        AtesEt();
        Animasyon();
        canBar.fillAmount = can / canUzunlugu; // Canı yüz verirsek en başta can uzunlugu 100 oluyor ve can azaldıkça 99/100 98/100.. diye azalıp oranlıyor.

        if (!noktayaGitti) // Birkere random seçmesi için
        {
            randomBolge = Random.Range(1, 4); // Random bölge belirlenir
            randomTur = 1;//Random.Range(1, 4);
            noktayaGitti = true; // tekrar random almaması için
        }
     
        if (randomBolge == 1) // Eğer random 1 çıkmışsa
        {
            BolgelerArasiGecis(birinciBolge); // İlk bölgeye git
        }
        else if (randomBolge == 2)  // Eğer random 1 çıkmışsa
        {
            BolgelerArasiGecis(ikinciBolge);  // İkinci bölgeye git
        }
        else if (randomBolge == 3) // Eğer random 1 çıkmışsa
        {
            BolgelerArasiGecis(ucuncuBolge);  // Üçüncü bölgeye git
        }
        
    }
    private void BolgelerArasiGecis(GameObject bolge) // Update metonda Random olarak oluşan sayıya göre bir bölge belirlenir ve o bölgeye geçiş için bu metod kullanılır.
    {
        if (bolgeKontrol == BolgeKontrol.noktayaGecis) // Eğer noktaya hareket ediyorsa
        {
            MesPosRot(anaDüsman.transform.position, bolge.transform.GetChild(bolgeIndex).position, out yeniPos, out mesafe,out rotasyonelPozisyon);
            anaDüsman.transform.position += yeniPos; // (bolge.transform.GetChild(bolgeIndex).position- anaDüsman.transform.position).normalized*Time.fixedDeltaTime*10
            if (mesafe < 0.2f) // Eğer noktaya geldiyse
            {
                bolgeIndex++; // Diğer noktaya geçmesi için bölgenin indexini arttırdık
                if (bolgeIndex >= bolge.transform.childCount) // Eğer son noktaya geldiyse (tur tamamlanmışsa)
                {
                    bolgeIndex = 0; // index sıfırlanır
                    turSayac++; // Her turu tamamladığında tur sayacı artar
                    if (turSayac == randomTur) // Random olarak alınan tur numarasına eşit olursa
                    {
                        bolgeKontrol = BolgeKontrol.ilkNoktayaGecis; // ilk noktaya geri döner
                    }
                    
                }
            }
        }
        else if (bolgeKontrol == BolgeKontrol.ilkNoktayaGecis)
        {
            MesPosRot(anaDüsman.transform.position, bolge.transform.GetChild(0).position, out yeniPos, out mesafe, out rotasyonelPozisyon); 
            anaDüsman.transform.position += yeniPos; // İlk noktaya geri dön (bolge.transform.GetChild(0).position)
            if (mesafe < 0.2f) // Yaklaştığında
            {
                bolgeKontrol = BolgeKontrol.anaNoktayaGecis; // Ana noktaya geri dön
            }
        }
        else if(bolgeKontrol == BolgeKontrol.anaNoktayaGecis)
        {
            MesPosRot(anaDüsman.transform.position, gecisBolgesi.transform.position, out yeniPos, out mesafe, out rotasyonelPozisyon);
            anaDüsman.transform.position += yeniPos; // Ana noktaya geri dön ( gecisBolgesi.transform.position,)
            if (mesafe < 0.2f) // yaklaştığında
            {
                noktayaGitti = false; // Yeniden random bölge belirlemek için update kısmının if ini açar
                bolgeKontrol = BolgeKontrol.noktayaGecis; // Yeni belirlenen bölgeye geçiş için ilk if e geri dön
                turSayac = 0; // Sayaçları sıfırla
                bolgeIndex =0;
            }
        }
        Rotasyon(rotasyonelPozisyon);
    }
    private void MesPosRot(Vector3 ilkPos, Vector3 sonPos, out Vector3 yeniPos, out float mesafe, out Vector3 rotPos) // Ana düsmanın mesafe pozisyon ve rotasyonunun hesaplandığı metod.
    {
        yeniPos = (sonPos - ilkPos).normalized*Time.fixedDeltaTime*2; //Gidiş noktasını aldık ve bunu geçiş metodunda pozisyona ekledik.
        mesafe = Vector3.Distance(ilkPos, sonPos); // Noktaya geldiğimizi anlamak için mesafe aldık

        Vector3 anaDüsmanPos = anaDüsman.transform.position;
        if (rayKaraktereDegdi) // Eğer ana düsman karakteri görüyorsa Ana karaktere dönmesi için
        {
            Vector3 karakterPos = anaKarakter.transform.position;
            rotPos = (anaDüsmanPos - karakterPos).normalized; // Rotasyon pozisyonunu out olarak alırız
        }
        else // Eğer görmüyorsa noktaya dönmesi için
        {
            Vector3 noktaPos = sonPos;
            rotPos = (anaDüsmanPos - sonPos).normalized;// Rotasyon pozisyonunu out olarak alırız
        }
    }
    private void RayCizdir() // Ana karaktere ray çizimi
    {
        raycastHit2D = Physics2D.Raycast(anaDüsman.transform.position, anaKarakter.transform.position - anaDüsman.transform.position, 1000, layerMask);
        Debug.DrawLine(anaDüsman.transform.position, raycastHit2D.point, Color.magenta);
      
        rayKaraktereDegdi = (raycastHit2D.collider.tag == "Player") ? true : false; // Eğer ray karaktere değiyorsa kontrol true olur değmiyorsa false.

    }
    private void Rotasyon(Vector3 rotasyonYonu) // Out olarak alınan rotasyon pozisyonu  MESPOSROT da  duruma göre belirlenir ve dönüş metounda kullanılarak düşman karakterin dönüşü sağlanır
    {
        float dönmeAcisi = Mathf.Atan2(rotasyonYonu.y, rotasyonYonu.x) * Mathf.Rad2Deg; // Mesafe vektörünün tanjant açısını al
        Quaternion rotasyon = Quaternion.Euler(0, 0, dönmeAcisi); // Rotasyona ekle
        rotasyon *= Quaternion.Euler(0, 0, 90); // Düzgün dönüş almak için 180 ile çarptık
        anaDüsman.transform.rotation = Quaternion.Lerp(anaDüsman.transform.rotation, rotasyon, 0.1f); // 0.1 sn farkla o noktaya dön
    }
    private void AtesEt()
    {
        if (rayKaraktereDegdi) // Eğer düsman, karakteri gördüyse
        {
            if (raycastHit2D.distance < 4 ) // Eğer karakter düşmana yakınsa
            {
                kursunYonKontrol = false; // Kurşun ileri doğru gitsin diye kontrol false olur
            }
            else // Karakter uzak ise
            {
                kursunYonKontrol = true; // Kurşun ona doğru olması için kontrol true olur
            }
            atisAraligi += Time.fixedDeltaTime;
            if (atisAraligi > 0.2f)
            {
                var atilanKursun=Instantiate(kursun, nisangah.transform.position, nisangah.transform.rotation*Quaternion.Euler(0,0,180)); // Kurşun prefabını oluştur
                atisAraligi = 0;
                Destroy(atilanKursun, 2);
            }
          
        }
    }
    public Vector3 KursunPosAl() // Kurşun gidiş yönünü aldığımız mesafe metodu
    {
        if (kursunYonKontrol) // Eğer mesafe uzaksa
        {
            return (raycastHit2D.point - (Vector2)nisangah.transform.position).normalized; // Karaktere doğru atış yap
        }
        else
        {
            return (nisangahYon.transform.position - nisangah.transform.position).normalized; // ileri doğru atış yap
        }
       
    }
    private void Animasyon()
    {
        if (rayKaraktereDegdi)
        {
            atesAnimAraligi += Time.fixedDeltaTime;
            if (atesAnimAraligi > 0.05f)
            {
                spriteRenderer.sprite = atesEtmeAnim[atesIndex++];
                if (atesIndex >= atesEtmeAnim.Length)
                {
                    atesIndex = 0;
                }

                atesAnimAraligi = 0;
            }
            
        }
        else
        {

            yürümeAnimAraligi += Time.fixedDeltaTime;
            if (yürümeAnimAraligi > 0.05f)
            {
                spriteRenderer.sprite = yürümeAnim[yürümeIndex++];
                if (yürümeIndex >= yürümeAnim.Length)
                {
                    yürümeIndex = 0;
                }

                yürümeAnimAraligi = 0;
            }
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos() // Her bölge oluşturacağımız gidiş noktalarına gizmoz çizdirdik.
    {
        List<GameObject> objelerimBir = new List<GameObject>(); // Noktaları liste halinde aldık
        List<GameObject> objelerimIki = new List<GameObject>();
        List<GameObject> objelerimUc = new List<GameObject>();
        GameObject BirinciBolge = transform.Find("BirinciBolge").gameObject; // Bölgeleri aldık
        GameObject IkinciBolge = transform.Find("IkinciBolge").gameObject;
        GameObject UcuncuBolge = transform.Find("UcuncuBolge").gameObject;
        GameObject GecisBolgesi = transform.Find("GecisBolgesi").gameObject; // Diğer bölgelere geçmeden önceki geçiş bölgesini aldık
        GameObject AnaDüsman = transform.Find("AnaDüsman").gameObject; // Ana düsman sprite ına ve colliderına sahip alt nesnemizi aldık

        if (!Application.isPlaying)
        {
            AnaDüsman.transform.position = GecisBolgesi.transform.position;
        }

        //------------------------------------------Geçiş Bölgesi gizmos çizimi------------------------------------------------------------------//
        Gizmos.DrawWireSphere(GecisBolgesi.transform.position, 1);
        Gizmos.color = Color.white;
        //------------------------------------------Birinci Bölge gizmos çizimi------------------------------------------------------------------//
        for(int i=0; i<BirinciBolge.transform.childCount; i++) // Bölgenin kaçtane childı varsa
        {
            objelerimBir.Add(BirinciBolge.transform.GetChild(i).gameObject); // o Kadar nokta ekle
        }
        for(int i=0; i < objelerimBir.Count; i++) // Ne kadar nokta varsa
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(objelerimBir[i].transform.position, 1); // Orda o kadar gizmos çizdir
        }
        for(int i=0; i< objelerimBir.Count-1; i++) // Ne kadar nokta var ise aralarında çizgi çizdir
        {
            Gizmos.DrawLine(objelerimBir[i].transform.position, objelerimBir[i + 1].transform.position);
        }
        if (objelerimBir.Count != 0) // Eğer nokta var ise
        {
            Gizmos.DrawLine(objelerimBir[objelerimBir.Count-1].transform.position, objelerimBir[0].transform.position); // Son nokta ile ilk nokta rası çizgi çizdir
            Gizmos.DrawLine(GecisBolgesi.transform.position, objelerimBir[0].transform.position); // İlk nokta ve geçiş noktası arasında çizgi çizdir.
        }
        //------------------------------------------İkinci Bölge gizmos çizimi------------------------------------------------------------------//
        for (int i = 0; i < IkinciBolge.transform.childCount; i++)
        {
            objelerimIki.Add(IkinciBolge.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < objelerimIki.Count; i++)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(objelerimIki[i].transform.position, 1);
        }
        for (int i = 0; i < objelerimIki.Count - 1; i++)
        {
            Gizmos.DrawLine(objelerimIki[i].transform.position, objelerimIki[i + 1].transform.position);
        }
        if (objelerimIki.Count != 0)
        {
            Gizmos.DrawLine(objelerimIki[objelerimIki.Count - 1].transform.position, objelerimIki[0].transform.position);
            Gizmos.DrawLine(GecisBolgesi.transform.position, objelerimIki[0].transform.position);
        }
        //-------------------------------------------Uçüncü Bölge gizmos çizimi------------------------------------------------------------------//
        for (int i = 0; i <UcuncuBolge.transform.childCount; i++)
        {
            objelerimUc.Add(UcuncuBolge.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < objelerimUc.Count; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(objelerimUc[i].transform.position, 1);
        }
        for (int i = 0; i < objelerimUc.Count - 1; i++)
        {
            Gizmos.DrawLine(objelerimUc[i].transform.position, objelerimUc[i + 1].transform.position);
        }
        if (objelerimUc.Count != 0)
        {
            Gizmos.DrawLine(objelerimUc[objelerimUc.Count - 1].transform.position, objelerimUc[0].transform.position);
            Gizmos.DrawLine(GecisBolgesi.transform.position, objelerimUc[0].transform.position);
        }


    }
#endif
}

enum BolgeKontrol // Durumları kontrol etmek için enum oluşturduk
{
    noktayaGecis=1,
    ilkNoktayaGecis =2,
    anaNoktayaGecis =3
}

#if UNITY_EDITOR

[CustomEditor(typeof(AnaDüsmanKonsol))]
[System.Serializable]

class anadüsmanEditor : Editor // Editör kodu yazarak Inspector kısmından her zaman istediğimiz bölgeye nokta oluşturduk.
{
    public override void OnInspectorGUI()
    {
        AnaDüsmanKonsol script = (AnaDüsmanKonsol)target; // Anadüsman konsol a eşit olan bir script adlı ana düsman konsol oluşturduk.
        if(GUILayout.Button("Nokta:Birinci Bolge",GUILayout.Width(200))) // Genişliği 200 olan Nokta Birinci Bölge adlı butonu oluştur.
        {
            GameObject yeniObje = new GameObject(); // Her buton click te bir game objesi oluştur
            yeniObje.transform.position = script.transform.position; // Bunu anaKonsolun ana objesinin pozisyonuna koy
            yeniObje.transform.parent = script.transform.Find("BirinciBolge").transform; // Ana düsmanın altında olan birinci bölgenin altına oluşturmak için objenin parentı olarak atadık.
            yeniObje.name = script.transform.Find("BirinciBolge").transform.childCount.ToString(); // İsmini birincibölge adlı parentın child count u olarak atadık 1,2,3,4 ...
        }
        else if (GUILayout.Button("Nokta:Ikinci Bolge", GUILayout.Width(200)))// Genişliği 200 olan Nokta İkinci Bölge adlı butonu oluştur.
        {
            GameObject yeniObje = new GameObject();// Her buton click te bir game objesi oluştur
            yeniObje.transform.position = script.transform.position;// Bunu anaKonsolun ana objesinin pozisyonuna koy
            yeniObje.transform.parent = script.transform.Find("IkinciBolge").transform;// Ana düsmanın altında olan ikinci bölgenin altına oluşturmak için objenin parentı olarak atadık. 
            yeniObje.name = script.transform.Find("IkinciBolge").transform.childCount.ToString();// İsmini Üçüncü bölge adlı parentın child count u olarak atadık 1,2,3,4 ...
        }
        else if (GUILayout.Button("Nokta:Ucuncu Bolge", GUILayout.Width(200)))// Genişliği 200 olan Nokta Üçüncü Bölge adlı butonu oluştur.
        {
            GameObject yeniObje = new GameObject(); ;// Her buton click te bir game objesi oluştur
            yeniObje.transform.position = script.transform.position; ;// Bunu anaKonsolun ana objesinin pozisyonuna koy
            yeniObje.transform.parent = script.transform.Find("UcuncuBolge").transform;// Ana düsmanın altında olan ikinci bölgenin altına oluşturmak için objenin parentı olarak atadık. 
            yeniObje.name = script.transform.Find("UcuncuBolge").transform.childCount.ToString();// İsmini Üçüncü bölge adlı parentın child count u olarak atadık 1,2,3,4 ...
        }

        EditorGUILayout.Space(); // Inspector panelinde boşluk oluşturur
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("layerMask")); // Inspector panelinde gözükmesi için yazdık
        EditorGUILayout.PropertyField(serializedObject.FindProperty("kursun")); // Inspector panelinde gözükmesi için yazdık
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canUzunlugu")); // Inspector panelinde gözükmesi için yazdık
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canBar")); // Inspector panelinde gözükmesi için yazdık
        EditorGUILayout.PropertyField(serializedObject.FindProperty("anaDüsmanCeset"),true); // Inspector panelinde gözükmesi için yazdık
        EditorGUILayout.PropertyField(serializedObject.FindProperty("anaDüsmanKan")); // Inspector panelinde gözükmesi için yazdık
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yürümeAnim"),true); // Inspector panelinde gözükmesi için yazdık (true yazarak alt sekmeleri gördük)
        EditorGUILayout.PropertyField(serializedObject.FindProperty("atesEtmeAnim"),true); // Inspector panelinde gözükmesi için yazdık
        serializedObject.ApplyModifiedProperties(); // Değişiklik yapabilmek için
        serializedObject.Update(); // Güncelleyebilmek için
    }

}

#endif