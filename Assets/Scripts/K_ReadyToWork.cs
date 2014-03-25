using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class K_ReadyToWork : MonoBehaviour
{
    public static Dictionary<string, K_ReadyToWork> All = new Dictionary<string, K_ReadyToWork>();

    void Awake() {
        All.Add(this.name, this);
    }

    delegate IEnumerator Work();

    Queue<Work> Works = new Queue<Work>();

    public bool IsPlaying { get; private set; }

    public bool IsWorkDone {
        get { 
            if (!this.IsPlaying && this.Works.Count == 0)
                Debug.Log("WORK DONE");
            return !this.IsPlaying && this.Works.Count == 0;}
    }

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

    IEnumerator monoWork(Action<K_ReadyToWork> work) {
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

    ////////////////////////////////////////////////////////////////////////////////////
    /*                                                                                */
    ////////////////////////////////////////////////////////////////////////////////////
    IEnumerator delay(float delay) {
        yield return new WaitForSeconds(delay);
    }

    public void Delay(float d) {
        this.AddWork(delay(d));
    }

    void tweenAnimate<T>(Func<T> getTarget, Action<T, T, float> loop, T to, Func<T> from, K_TimeCurve timecurve = null, float duration = 1f) {
        timecurve = timecurve ?? K_TimeCurve.Linear(duration);
        T f = default(T);
        this.AddWork(x => f = from());
        this.AddLoopWork(
            x => {
            timecurve.Progress();
            loop(f, to, timecurve.Eval);},
            x => !EqualityComparer<T>.Default.Equals(to, getTarget()));
    }
    void tweenAnimate<T>(Func<T> getTarget, Action<T, T, float> loop, T to, T? from, K_TimeCurve timecurve = null, float duration = 1f) where T : struct {
        Func<T> f;
        if (from.HasValue)
            f = () => (T)from;
        else 
            f = () => getTarget();
        this.tweenAnimate<T>(getTarget, loop, to, f, timecurve, duration);
    }

    void lerpPosition(Transform target, Vector3 to, Vector3? from = null, K_TimeCurve timecurve = null, float duration = 1f) {
        this.tweenAnimate<Vector3>(() => target.position, (f, t, p) => target.position = Vector3.Lerp(f, t, p), to, from, timecurve, duration);
    }

    public void LerpPosition(Transform target, Vector3 from, Vector3 to, K_TimeCurve timecurve = null, float duration = 1f) {
        lerpPosition(target, to, from: from, timecurve: timecurve, duration: duration);
    }

    public void LerpPosition(Vector3 to, K_TimeCurve timecurve = null, float duration = 1f) {
        lerpPosition(this.transform, to, timecurve: timecurve, duration: duration);
    }

    public void LerpPosition(Vector3 from, Vector3 to, K_TimeCurve timecurve = null, float duration = 1f) {
        lerpPosition(this.transform, to, from, timecurve: timecurve, duration: duration);
    }

    IEnumerator lerpScale(Func<Vector3> from, Vector3 to, K_TimeCurve timecurve) {
        Vector3 f = from();
        while (to != this.transform.localScale) {
            timecurve.Progress();
            this.transform.localScale = Vector3.Lerp(f, to, timecurve.Eval);
            yield return null;
        }
    }

    public void LerpScale(Vector3 to, float duration) {
        this.AddWork(lerpScale(() => this.transform.localScale, to, K_TimeCurve.Linear(duration)));
    }

    public void LerpScale(Vector3 to, K_TimeCurve timecurve) {
        this.AddWork(lerpScale(() => this.transform.localScale, to, timecurve));
    }

    public void LerpScale(Vector3 from, Vector3 to, K_TimeCurve timecurve) {
        this.AddWork(lerpScale(() => from, to, timecurve));
    }

    void lerpColor(Material target, Color to, Color? from = null, K_TimeCurve timecurve = null, float duration = 1f) {
        this.tweenAnimate<Color>(() => target.color, (f, t, p) => target.color = Color.Lerp(f, t, p), to, from, timecurve, duration);
    }

    public void LerpColor(Color to, K_TimeCurve timecurve = null, float duration = 1f) {
        lerpColor(this.renderer.material, to, timecurve: timecurve, duration: duration);
    }

    public void LerpColor(Material target, Color to, K_TimeCurve timecurve = null, float duration = 1f) {
        lerpColor(target, to, timecurve: timecurve, duration: duration);
    }

    public void LerpColor(Material target, Color to, float duration = 1f) {
        lerpColor(target, to, duration: duration);
    }
    
}
