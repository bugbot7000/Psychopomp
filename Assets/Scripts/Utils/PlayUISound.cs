using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayUISound : MonoBehaviour
{
    private WwiseSoundManager wwiseSoundManager;

    // Start is called before the first frame update
    void Start()
    {
        wwiseSoundManager = GameObject.FindObjectOfType<WwiseSoundManager>();

    }

    public void PlayStartSound()
    {
        wwiseSoundManager.UI_Sound_Start();
    }

    public void QuitSound()
    {
        wwiseSoundManager.UI_Sound_Quit();
        Invoke("ExitGam",4);
    }

    void ExitGam()
    {
        Application.Quit();
    }
}
