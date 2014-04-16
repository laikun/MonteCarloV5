using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_ReadyToWork : MonoBehaviour
{
    public static Dictionary<string, K_ReadyToWork> All = new Dictionary<string, K_ReadyToWork>();

    static bool noIntercept;
    public bool NoIntercept { get { return noIntercept; } set { noIntercept = value; } }
    public bool IsWorking { get; private set; }
    public bool IsWorkDone { get {return !this.IsWorking && this.Works.Count == 0;} }

    delegate IEnumerator Work();
    Queue<Work> Works = new Queue<Work>();
    
    public void MoreWork(IEnumerator act) {
        this.Works.Enqueue(() => act);
    }    
    
    void Awake() {
        All.Add(this.name, this);
    }

    public void GoWork() {
        if (noIntercept && !IsWorking)
            StartCoroutine("Waiting");
        if (!IsWorking)
            StartCoroutine("WorkDo");
    }

    public void ForceWork() {
        StartCoroutine("WorkDo");
    }

    IEnumerator Waiting() {
        while (noIntercept) {
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine("WorkDo");
    }

    public void Stop() {
        if (IsWorking)
            StopCoroutine("WorkDo");
    }

    public void ForceStop(){
        StopCoroutine("WorkDo");
        this.Works.Clear();
        this.IsWorking = false;
    }

    public void WorkGone(){
        StopCoroutine("WorkDo");
        this.Works.Clear();
    }

    IEnumerator WorkDo() {
        this.IsWorking = true;
        this.enabled = false;
        while (this.Works.Count > 0) {
            Work work = this.Works.Dequeue();
            yield return StartCoroutine(work());
        }
        this.IsWorking = false;
        noIntercept = false;
        this.enabled = true;
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
    
    public void MoreWork(Action<K_ReadyToWork> work) {
        this.MoreWork(monoWork(work));
    }

    public void DelayWork(Func<K_ReadyToWork, bool> delayFlag, Action<K_ReadyToWork> work) {
        this.MoreLoopWork(x => {}, delayFlag);
        this.MoreWork(work);
    }

    public void MoreLoopWork(Action<K_ReadyToWork> work, Func<K_ReadyToWork, bool> flag) {
        this.MoreWork(loopWork(work, flag));
    }

    ////////////////////////////////////////////////////////////////////////////////////
    /*                                                                                */
    ////////////////////////////////////////////////////////////////////////////////////
    IEnumerator delay(float delay) {
        yield return new WaitForSeconds(delay);
    }

    public void Delay(float d) {
        this.MoreWork(delay(d));
    }

    public void Rest(){
        this.MoreWork(x => StopCoroutine("WorkDo"));
    }

    public static void TweenAnimate<T>(K_ReadyToWork rtw, Func<T> getTarget, Action<T, T, float> loop, Func<T> to, Func<T> from = null, K_TimeCurve timecurve = null, float duration = 1f) {
        timecurve = timecurve ?? K_TimeCurve.Linear(duration);
        from = from != null ? from : getTarget;
        T f = default(T);
        T t = default(T);
        rtw.MoreWork(x => {
            f = from();
            t = to();
        });
        rtw.MoreLoopWork(
            x => {
            timecurve.Progress();
            loop(f, t, timecurve.Eval);},
        x => !EqualityComparer<T>.Default.Equals(t, getTarget()));
    }
    
    void tweenAnimate<T>(Func<T> getTarget, Action<T, T, float> loop, T to, T? from, K_TimeCurve timecurve = null, float duration = 1f) where T : struct {
        Func<T> t = () => to;
        Func<T> f;
        if (from.HasValue)
            f = () => (T)from;
        else 
            f = getTarget;
        TweenAnimate<T>(this, getTarget, loop, t, f, timecurve, duration);
    }

    public void LerpTransform(Transform target, Func<TransformData> to, Func<TransformData> from = null, K_TimeCurve timecurve = null, float duration = 1f) {
        timecurve = timecurve ?? K_TimeCurve.Linear(duration);

        if (from == null)
            from = () => new TransformData(target);
        TransformData f = new TransformData();
        TransformData t = new TransformData();
        this.MoreWork(x => {
            f = from();
            t = to();
        });
        this.MoreLoopWork(
            x => {  
            timecurve.Progress();
            target.position = Vector3.Lerp(f.position, t.position, timecurve.Eval);
            target.localScale = Vector3.Lerp(f.scale, t.scale, timecurve.Eval);
            target.localRotation = Quaternion.Lerp(f.rotation, t.rotation, timecurve.Eval);
            },
        x => timecurve.Eval != 1);
    }

    void lerpPosition(Transform target, Vector3 to, Vector3? from = null, K_TimeCurve timecurve = null, float duration = 1f) {
        this.tweenAnimate<Vector3>(() => target.position, (f, t, p) => target.position = Vector3.Lerp(f, t, p), to, from, timecurve, duration);
    }

    void lerpColor(Material target, Color to, Color? from = null, K_TimeCurve timecurve = null, float duration = 1f) {
        this.tweenAnimate<Color>(() => target.color, (f, t, p) => target.color = Color.Lerp(f, t, p), to, from, timecurve, duration);
    }
    
    IEnumerator lerpScale(Func<Vector3> from, Vector3 to, K_TimeCurve timecurve) {
        Vector3 f = from();
        while (to != this.transform.localScale) {
            timecurve.Progress();
            this.transform.localScale = Vector3.Lerp(f, to, timecurve.Eval);
            yield return null;
        }
    }
    
    void lerpAlpha(Material target, float to, float? from = null, K_TimeCurve timecurve = null, float duration = 1f) {
        Color f = target.color;
        Color t = target.color;
        f.a = from.HasValue ? (float)from : f.a;
        target.color = f;
        t.a = to;
        this.lerpColor(target, t, f, timecurve, duration);
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

    public void LerpScale(Vector3 to, float duration) {
        this.MoreWork(lerpScale(() => this.transform.localScale, to, K_TimeCurve.Linear(duration)));
    }

    public void LerpScale(Vector3 to, K_TimeCurve timecurve) {
        this.MoreWork(lerpScale(() => this.transform.localScale, to, timecurve));
    }

    public void LerpScale(Vector3 from, Vector3 to, K_TimeCurve timecurve) {
        this.MoreWork(lerpScale(() => from, to, timecurve));
    }

    public void LerpColor(Material target, Color to, K_TimeCurve timecurve = null, float duration = 1f) {
        lerpColor(target, to, timecurve: timecurve, duration: duration);
    }

    public void LerpColor(Material target, Color to, float duration = 1f) { 
        lerpColor(target, to, duration: duration);
    }
    
    public void LerpColor(Color from, Color to, float duration = 1f) {
        lerpColor(this.renderer.material, to, from, duration: duration);
    }
    
    public void LerpColor(Color to, float duration = 1f) {
        lerpColor(this.renderer.material, to, duration: duration);
    }

    public void LerpAlpha(Material target, float from, float to, K_TimeCurve timecurve = null, float duration = 1f) {
        this.lerpAlpha(target, to, from, timecurve, duration);
    }
    
    public void LerpAlpha(float from, float to, K_TimeCurve timecurve = null, float duration = 1f) {
        this.lerpAlpha(this.renderer.material, to, from, timecurve, duration);
    }

    public void LerpAlpha(float to, K_TimeCurve timecurve = null, float duration = 1f) {
        this.lerpAlpha(this.renderer.material, to, timecurve: timecurve, duration: duration);
    }
}
