using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines.Primitives;
using UnityEngine;
using NaughtyAttributes;
public class ToggleWaterState : MonoBehaviour{
    public Material waterMat;

    [Button("Pause Water")]
    public void PauseWater()
    {
        waterMat.SetInt("_IsPaused",1);
    }
    
    [Button("Resume Water")]
    public void ResumeWater()
    {
        waterMat.SetInt("_IsPaused",0);
    }
}
