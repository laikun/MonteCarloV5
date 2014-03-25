using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class K_ReadyToWork : MonoBehaviour
{
    public static Dictionary<string, K_ReadyToWork> All = new Dictionary<string, K_ReadyToWork>();

    void Awake(){
        All.Add(this.name, this);
    }

    delegate IEnumerator Work();
    Queue<Work> Works = new Queue<Work>();

    public bool IsPlaying { get; private set; }
    public bool IsWorkDone {get {if (!this.IsPlaying && this.Works.Count == 0) Debug.Log("WORK DONE"); return !this.IsPlaying && this.Works.Count == 0;}}

    public void Play() {
        if (!IsPlaying)
            StartCoroutine("WorkDo");
    }

    public void Stop() {
        if (IsPlaying)
            StopCoroutine("WorkDo");
    }

    IEnumerator WorkDo() {
        this.IsPlaying = true;
        while (Works.Count > 0) {
            Work work = Works.Dequeue();
            yield return StartCoroutine(work());
        }
        this.IsPlaying = false;
    }

    IEnumerator monoWork(Action<K_ReadyToWork> work){
        work(this);
        yield break;
    }

    IEnumerator loopWork(Action<K_ReadyToWork> loopWork, Func<K_ReadyToWork, bool> flag) {
        while (flag(this)) {
            loopWork(this);
            yield return null;
        }
    }
    
    public void AddWork(IEnumerator act) {
        this.Works.Enqueue(() => act);
    }

    public void AddWork(Action<K_ReadyToWork> work) {
        this.AddWork(monoWork(work));
    }

    public void AddDelayWork(Func<K_ReadyToWork, bool> delayFlag, Action<K_ReadyToWork> work) {
        this.AddLoopWork(x => {}, delayFlag);
        this.AddWork(work);
    }

    public void AddLoopWork(Action<K_ReadyToWork> work, Func<K_ReadyToWork, bool> flag) {
        this.AddWork(loopWork(work, flag));
    }
    
    IEnumerator delay(float delay) {
        yield return new WaitForSeconds(delay);
    }

    public void Delay(float d) {
        this.AddWork(delay(d));
    }

    void lerfPosition(Func<Vector3> from, Vector3 to, K_TimeCurve timecurve) {
        Vector3 f = Vector3.zero;
        this.AddWork(x => f = from());
        this.AddLoopWork(
            x => {
                timecurve.Progress();
                x.transform.position = Vector3.Lerp(f, to, timecurve.Eval);},
            x => to != x.transform.position);
    }
    
    public void LerfPosition(Vector3 to, float duration) {
        lerfPosition(() => this.transform.position, to, K_TimeCurve.Linear(duration));
    }

    public void LerfPosition(Vector3 to, K_TimeCurve timecurve) {
        lerfPosition(() => this.transform.position, to, timecurve);
    }

    public void LerfPosition(Vector3 from, Vector3 to, K_TimeCurve timecurve) {
        lerfPosition(() => from, to, timecurve);
    }

    IEnumerator lerfScale(Func<Vector3> from, Vector3 to, K_TimeCurve timecurve) {
        Vector3 f = from();
        while (to != this.transform.localScale) {
            timecurve.Progress();
            this.transform.localScale = Vector3.Lerp(f, to, timecurve.Eval);
            yield return null;
        }
    }

    public void LerfScale(Vector3 to, float duration) {
        this.AddWork(lerfScale(() => this.transform.localScale, to, K_TimeCurve.Linear(duration)));
    }

    public void LerfScale(Vector3 to, K_TimeCurve timecurve) {
        this.AddWork(lerfScale(() => this.transform.localScale, to, timecurve));
    }

    public void LerfScale(Vector3 from, Vector3 to, K_TimeCurve timecurve) {
        this.AddWork(lerfScale(() => from, to, timecurve));
    }

    IEnumerator lerfColor(Func<Color> from, Color to, K_TimeCurve timecurve) {
        Color f = this.transform.renderer.material.color;
        while (to != this.transform.renderer.material.color) {
            timecurve.Progress();
            this.transform.renderer.material.color = Color.Lerp(f, to, timecurve.Eval);
            yield return null;
        }
    }

    public void LerfColor(Color to, float duration) {
        this.AddWork(lerfColor(() => this.renderer.material.color, to, K_TimeCurve.Linear(duration)));
    }

    public void LerfColor(Color to, K_TimeCurve timecurve) {
        this.AddWork(lerfColor(() => this.renderer.material.color, to, timecurve));
    }

    public void LerfColor(Color from, Color to, K_TimeCurve timecurve) {
        this.AddWork(lerfColor(() => from, to, timecurve));
    }
}