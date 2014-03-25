using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class K_OnStage : Singleton<K_OnStage>
{
    static int backStage;
    static int stage;

    static void OnStage(GameObject go, bool on) {
        Array.ForEach(go.GetComponentsInChildren<Transform>(), x => x.gameObject.layer = on ? stage : backStage);
    }
    
    public static void In(MonoBehaviour go) {
        In(go.gameObject);
    }
    public static void In(GameObject go) {
        OnStage(go, true);
    }

    public static void Out(MonoBehaviour go) {
        Out(go.gameObject);
    }
    public static void Out(GameObject go) {
        OnStage(go, false);
    }
    
    public static void Init() {
        backStage = LayerMask.NameToLayer("BackStage");
        stage = LayerMask.NameToLayer("Stage");
    }
}

