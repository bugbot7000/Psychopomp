using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReorderSelf : MonoBehaviour
{
    // Start is called before the first frame update
    public void SendToBack()
    {
        transform.SetSiblingIndex(0);
    }

    public void BringToFront()
    {
        Debug.Log(transform.parent.childCount-1+"   fffff");
        transform.SetSiblingIndex(transform.parent.childCount-1);
    }
}
