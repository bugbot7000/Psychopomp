using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines.Primitives;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Letter : MonoBehaviour
{
    public TextMeshProUGUI contents;
    public string Content;
    public Transform Stamps;
    // Start is called before the first frame update

    private void Start()
    {
        AssignStampAndText();
    }

    void AssignStampAndText()
    {
        for (int i = 0; i < Stamps.childCount; i++)
        {
            Stamps.GetChild(i).gameObject.SetActive(false);
        }
        Stamps.GetChild(Random.Range(0,Stamps.transform.childCount)).gameObject.SetActive(true);
        contents.text = Content;
    }
}
