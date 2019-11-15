using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTriggerKontrol : MonoBehaviour {

    AnaDüsmanKonsol anaDüsmanKonsol;

    private void Start()
    {
        anaDüsmanKonsol = GameObject.FindGameObjectWithTag("AnaDüsmanTag").GetComponent<AnaDüsmanKonsol>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "karakterKursunTag")
        {
            anaDüsmanKonsol.can--;

            if (anaDüsmanKonsol.canBar.fillAmount <= 0.5f)
            {
                var kanEfekt = Instantiate(anaDüsmanKonsol.anaDüsmanKan, transform.position, Quaternion.Euler(0, 0, (Random.Range(0, 360))));
                DontDestroyOnLoad(kanEfekt); // Karakter yok olduğunda kan kalsın diye kullandık
                Destroy(kanEfekt, 15); // 15 sn sonra yok olur.

            }

            if (anaDüsmanKonsol.can <= 0)
            {
                var cesetIndex = Random.Range(0, anaDüsmanKonsol.anaDüsmanCeset.Length);  // Random bir ceset sprite ı oluştur
                var cesetler=Instantiate(anaDüsmanKonsol.anaDüsmanCeset[cesetIndex], transform.position, Quaternion.identity);
                DontDestroyOnLoad(cesetler);
                Destroy(cesetler, 10);

                Destroy(anaDüsmanKonsol.gameObject);
            }
        }
    }
}
