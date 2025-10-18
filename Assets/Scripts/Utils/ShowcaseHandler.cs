using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowcaseHandler : MonoBehaviour
{
    public int IdleTimeSetting = 5;
    public float IdleTime;
    public float idleInSeconds;
    public GameObject IdleAnimator;
    
    private bool isRestarting = false;
    
    void Awake()
    {
        idleInSeconds = IdleTimeSetting * 60;
    }
        
    private void Update(){
        if(isRestarting && Input.anyKey){
            isRestarting = false;
            IdleAnimator.gameObject.SetActive(false);
        }

        if(Input.anyKey) {
            IdleTime = 0;
        }

        if (IdleTime >= idleInSeconds-4) {
            isRestarting = true;
            IdleAnimator.gameObject.SetActive(true);  
        } else {
            IdleCheck();
        }
        
        if(Input.GetKeyUp(KeyCode.Backslash)){
            IdleAnimator.gameObject.SetActive(true);
            isRestarting = true;
        }
        
    }
        
    public void IdleCheck(){
        IdleTime += Time.deltaTime;
    }
}
