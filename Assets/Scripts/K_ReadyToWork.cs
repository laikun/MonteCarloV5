using System;
using UnityEngine;
using System.Collections;

public class K_ReadyToWork : MonoBehaviour
{
    IEnumerator work;

    public void AddWork(IEnumerator work)
    {
        this.work = work;
    }

    public void DoWork(){
        StartCoroutine(work);
    }

    public void StopWork(){
        StopCoroutine("work");
    }
}

