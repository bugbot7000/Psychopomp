using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterDeliveredText : MonoBehaviour
{
    private Animator anim;
    private void OnEnable()
    {
        anim = GetComponent<Animator>();
        anim.Play("Hooray");
    }

    public void IamGoingToFuckingSlIIIP()
    {
        gameObject.SetActive(false);
    }
    
}
