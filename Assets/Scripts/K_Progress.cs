using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_TimeCurve
{
    private float duration;

    public float Duration { get { return this.duration; } set { this.duration = value != 0f ? value : 0.0001f; } }
    
    public float Point { private set; get; }
    
    public AnimationCurve Curve { get; set; }

    public bool IsZero { get { return this.Point <= 0f; } }

    public bool IsOver { get { return this.Point >= this.Duration; } }

    public static K_TimeCurve Dummy { get { return new K_TimeCurve(null, 0f); } }

    public static K_TimeCurve Normal { get { return new K_TimeCurve(null); } }
    
    public K_TimeCurve(AnimationCurve curve, float duration = 1f) {
        this.Point = 0f;
        this.Duration = duration;
        this.Curve = curve ?? NormalCurve.Linear;
    }
    
    public void Reset() {
        this.Point = 0f;
    }
    
    public void Progress(bool direction = true) {
        this.Point += (direction ? 1 : -1) * Time.deltaTime;
    }
    
    public float Evaluate(float point) {
        return this.Curve.Evaluate(Mathf.Clamp01(1 / this.Duration * point));
    }
    
    public float Eval { get { return this.Evaluate(this.Point); } }

    public K_TimeCurve Clone() {
        K_TimeCurve tc = MemberwiseClone() as K_TimeCurve;
        tc.Reset();
        return tc;
    }

    public override string ToString() {
        return string.Format("[K_TimeCurve: Duration={0}, Point={1}]", duration, Point);
    }
}

public interface K_ITween<T>
{
    T From { get; set; }

    T To { get; set; }

    K_TimeCurve TimeCurve { get; set; }
}

public class K_Tween<T> : K_ITween<T>
{
    public T From { get; set; }

    public T To { get; set; }
    
    public K_TimeCurve TimeCurve { get; set; }

    public K_Tween(T from, T to) {
        this.From = from;
        this.To = to;
    }
}

public delegate IEnumerator Work(GameObject go,K_Trans trans);

public delegate bool Flag(GameObject go,K_Trans trans);

public interface K_Trans
{
    K_TimeCurve TimeCurve { get; set; }
    
    bool Direction { get; set; }

    bool Repeat { get; set; }

    bool RepeatRewind { get; set; }

    bool Pause { get; set; }

    K_Trans Clone { get; }

    IEnumerator Progress(GameObject go);

    event Flag WorkDelay;
    event Work WorkDo;
    event Work WorkDone;
}

public class K_TransTween<T> : K_ITween<T>, K_Trans
{
    public K_TimeCurve TimeCurve { get; set; }

    public T From { get; set; }
    
    public T To { get; set; }

    public bool Direction { get; set; }

    public bool Repeat { get; set; }

    public bool RepeatRewind { get; set; }

    public bool Pause { get; set; }

    public K_Trans Clone { get { return MemberwiseClone() as K_Trans; } }

    public event Work WorkDo;
    public event Work WorkDone;
    public event Flag WorkDelay;
    
    protected virtual IEnumerator OnWorkDo(GameObject go, K_Trans trans) {
        return this.WorkDo != null ? this.WorkDo(go, trans) : null;
    }

    protected virtual bool OnWorkDelay(GameObject go, K_Trans trans) {
        return this.WorkDelay != null ? this.WorkDelay(go, trans) : true;
    }

    protected virtual IEnumerator OnWorkDone(GameObject go, K_Trans trans) {
        return this.WorkDone != null ? this.WorkDone(go, trans) : null;
    }
    
    public K_TransTween(T from, T to, K_TimeCurve timeCurve) {
        this.From = from;
        this.To = to;
        this.TimeCurve = timeCurve;
        this.Direction = true;
        this.Repeat = false;
        this.RepeatRewind = false;
    }

    public K_TransTween<T> Next(T to) {
        K_TransTween<T> tt = MemberwiseClone() as K_TransTween<T>;
        tt.From = tt.To;
        tt.To = to;
        tt.TimeCurve = this.TimeCurve.Clone();
        tt.TimeCurve.Reset();
        return tt;
    }

    public K_TransTween<T> Next(T to, float duration) {
        K_TransTween<T> tt = this.Next(to);
        tt.TimeCurve.Duration = duration;
        return tt;
    }

    public IEnumerator Progress(GameObject go) {

        if (this.WorkDelay != null) { 
            while (!this.WorkDelay(go, this))
                yield return null;
        }

        Debug.Log("Work Do");
        // Main Work
        while (true) {
            // Time Progress
            if (!Pause)
                this.TimeCurve.Progress(this.Direction);

            if (this.WorkDo != null) {
                this.WorkDo(go, this);
            }

            if (this.TimeCurve.IsOver) {
                if (this.RepeatRewind)
                    this.Direction = false;
                else if (this.Repeat)
                    this.TimeCurve.Reset();
                else
                    break;
            } else if (this.TimeCurve.IsZero) {
                if (this.RepeatRewind)
                    this.Direction = true;
            }

            yield return null;
        }
        this.TimeCurve.Reset();
        Debug.Log("Work Done");

        if (this.WorkDone != null)
            this.WorkDone(go, this);
    }

    public override string ToString() {
        return string.Format("[K_TransTween: From={0}, To={1}]", From, To);
    }
}

public interface K_ICatridge
{
    void Play();

    bool Previous();

    bool Next();

    bool Loop {get; set;}

    void Add(K_Trans item);
}

public class K_Catridge<T> : K_ICatridge where T : K_Trans
{
    public K_MonoPlayer Deck { private set; get; }

    private LinkedList<T> collection;
    private LinkedListNode<T> current;

    public bool Loop { get; set; }

    public T Current { get { return this.current.Value; } }

    public K_Catridge(GameObject go, T seed) {
        this.collection = new LinkedList<T>();
        this.collection.AddLast(seed);
        this.current = this.collection.Last;

        this.Deck = go.AddComponent<K_MonoPlayer>();
        this.Deck.Cat = this;
    }

    public void Play() {
        this.Deck.Play();
    }

    public void Stop() {
        this.Deck.Stop();
    }
    
    bool SetCurrent(bool direction) {
        this.current = direction ? this.current.Next : this.current.Previous;

        if (this.current == null) {
            if (this.Loop)
                this.current = direction ? this.collection.First : this.collection.Last;
            else
                return false;
        }

        this.Deck.ExWork = this.current.Value.Progress;
        Debug.Log("Next Trans : " + this.current.Value.ToString());
        return true;
    }

    public bool Previous() {
        return this.SetCurrent(false);
    }

    public bool Next() {
        return this.SetCurrent(true);
    }

    public void Add(K_Trans item) {
        this.collection.AddLast((T)item);
    }   
}

public class K_MonoPlayer : MonoBehaviour
{
    public K_ICatridge Cat;
    public delegate IEnumerator Work(GameObject go);

    public Work ExWork;

    IEnumerator playing() {
        while (Cat.Next()) {
            yield return StartCoroutine(ExWork(gameObject));
        }
    }
    
    public void Play() {
        StartCoroutine("playing");
    }
    
    public void Stop() {
        StopCoroutine("playing");
    }    
}

public static class K_ProgressSet
{
    public static K_Catridge<K_TransTween<Vector3>> ReadyToMove(GameObject go, out K_TransTween<Vector3> seed) {
        seed = new K_TransTween<Vector3>(go.transform.position, go.transform.position, K_TimeCurve.Dummy);
        seed.WorkDo += (g, t) => {
            K_ITween<Vector3> tw = t as K_ITween<Vector3>;
            g.transform.position = Vector3.Lerp(tw.From, tw.To, t.TimeCurve.Eval);
            return null;
        };
        return new K_Catridge<K_TransTween<Vector3>>(go, seed);
    }

    public static K_Catridge<K_TransTween<Vector3>> ReadyToScale(GameObject go, out K_TransTween<Vector3> seed) {
        seed = new K_TransTween<Vector3>(go.transform.localScale, go.transform.localScale, K_TimeCurve.Dummy);
        seed.WorkDo += (g, t) => {
            K_ITween<Vector3> tw = t as K_ITween<Vector3>;
            g.transform.localScale = Vector3.Lerp(tw.From, tw.To, t.TimeCurve.Eval);
            return null;
        };
        return new K_Catridge<K_TransTween<Vector3>>(go, seed);
    }

    public static K_Catridge<K_TransTween<Color>> ReadyToColor(GameObject go, out K_TransTween<Color> seed) {
        seed = new K_TransTween<Color>(go.renderer.material.color, go.renderer.material.color, K_TimeCurve.Dummy);
        seed.WorkDo += (g, t) => {
            K_ITween<Color> tw = t as K_ITween<Color>;
            g.renderer.material.color = Color.Lerp(tw.From, tw.To, t.TimeCurve.Eval);
            return null;
        };
        return new K_Catridge<K_TransTween<Color>>(go, seed);
    }
}
