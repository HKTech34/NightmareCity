using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makinaKursunKontrol : MonoBehaviour {

    makinaliKontrol makinaliDüsman;
    void Start()
    {

        makinaliDüsman = GameObject.FindGameObjectWithTag("makinaliTag").GetComponent<makinaliKontrol>();
        GetComponent<Rigidbody2D>().AddForce(makinaliDüsman.GetKursunPos() * 1000); // Makinalı düsmandan pozisyonu aldık ve o pozisyona doğru bir kuvvet uyguladık.
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "FareTag" && collision.gameObject.name != "AtesAlanı" && collision.tag != "makinaliTag" && collision.tag !="karakterKursunTag") // Kurşunun hangi durumlarda yok olmamasını ayarladık.
        {
            Destroy(gameObject);
        }
    }

}
