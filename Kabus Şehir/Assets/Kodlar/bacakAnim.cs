using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bacakAnim : MonoBehaviour {

    SpriteRenderer spriteRenderer;
    public Sprite[] ileriGeriAnim;
    public Sprite[] sağSolAnim;


    void Awake () {

        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
    public SpriteRenderer getbacakSR()
    {
        return spriteRenderer;
    }
	
}
