using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_SandBox : MonoBehaviour
{
    K_ReadyToWork mp;
    // Use this for initialization
    void Start()
    {
        mp = transform.GetOrAddComponent<K_ReadyToWork>();
        mp.Delay(1f);
        mp.LerpAlpha(this.renderer.material, 0f, 1f);
        mp.GoWork();
        
    }



}
