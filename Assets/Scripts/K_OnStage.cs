using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class K_OnStage : MonoBehaviour
{
    int backStage;
    int stage;

    void OnStage(bool on)
    {
        Array.ForEach(this.gameObject.GetComponentsInChildren<Transform>(), x => x.gameObject.layer = on ? stage : backStage);
    }   
    
    public void InStage()
    {
        this.OnStage(true);
    }
    
    public void OutStage()
    {
        this.OnStage(false);
    }

    void Awake(){
        backStage = LayerMask.NameToLayer("BackStage");
        stage = LayerMask.NameToLayer("Stage");
    }
}

