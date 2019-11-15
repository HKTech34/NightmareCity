using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADKursunKontrol : MonoBehaviour {

    AnaDüsmanKonsol anaDüsman;
    Rigidbody2D fizik;
    [SerializeField] float kursunHizi=1000;

	void Start () {

        anaDüsman = GameObject.FindGameObjectWithTag("AnaDüsmanTag").GetComponent<AnaDüsmanKonsol>();
        fizik = GetComponent<Rigidbody2D>();
        fizik.AddForce(anaDüsman.KursunPosAl()*kursunHizi); // Kurşun çıktığında ileri hareket eder
	}
}
