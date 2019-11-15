using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kursunKontrol : MonoBehaviour {

    KarakterKonsol karakter;

	void Start () {

        karakter = GameObject.FindGameObjectWithTag("Player").GetComponent<KarakterKonsol>(); // Karakterin karakter konsol scriptine ulaştık.
        GetComponent<Rigidbody2D>().AddForce(karakter.getKursunPos() * 1000); // Karakter konsoldaki getKursunPos metodundan aldığımız yönde kurşun hareket eder.
	}

    private void OnTriggerEnter2D(Collider2D collision) // Kurşun bir collidera değdiğinde
    {
        if (collision.tag != "FareTag" && collision.tag != "Player" && collision.gameObject.name != "AtesAlanı" && collision.tag != "makinaliKursunTag") 
        {
            Destroy(gameObject);
        }
    }


}
