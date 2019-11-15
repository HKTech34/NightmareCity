using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class atesAlaniKontrol : MonoBehaviour {

    makinaliKontrol atesKontrolDurum;  // Bool u aldık.

	void Start () {

        atesKontrolDurum = transform.parent.GetComponent<makinaliKontrol>();
        transform.SetParent(transform.parent.parent); // Alan kontrol normalde makinalı kontrolun child ı biz rotasyonu almasın diye oyun esnasında çıkardık childden.
        
	}

    void OnTriggerStay2D(Collider2D collision) 
    {
        if (collision.tag == "Player") // Eğer oyuncu ateş alanı içine girdiyse
        {
            atesKontrolDurum.atesKontrol = true; // Bool true
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player") // Çıktıysa
        {
            atesKontrolDurum.atesKontrol = false; // false.
        }
    }


}
