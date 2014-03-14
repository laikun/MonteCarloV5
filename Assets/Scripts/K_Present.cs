using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_Present
{
    public float Delay { get; set; }

    public float Duration { get; set; }

    public AnimationCurve Curve { get; set; }

    public bool Repeat { get; set; }

    public virtual void Work(GameObject go, float eval) {}

    public virtual bool Start(GameObject go) {
        return true;}

    public virtual bool End(GameObject go) {
        return true;}

    public K_Present() {
        this.Delay = 0f;
        this.Duration = 1f;
        this.Curve = K_Present.NormalLinear;
    }

    public K_Present Next<T>(T to) {
        return (this as K_Present<T>).Next(to); }

    public static AnimationCurve NormalLinear{ get { return AnimationCurve.Linear(0f, 0f, 1f, 1f); } }

    public static AnimationCurve NormalEaseInOut { get { return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); } }

    public static AnimationCurve NormalEaseIn{ get { return new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(1f, 1f, 2f, 0f)); } }

    public static AnimationCurve NormalEaseOut{ get { return new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1f, 1f, 0f, 0f)); } }

    public static K_Present Position(Vector3 from, Vector3 to) {
        K_Present<Vector3> present = new K_Present<Vector3>(from, to);
        present.ExWork = (g, v, p) => g.transform.position = Vector3.Lerp(p.From, p.To, v);
        present.EndFlag = (g, p) => g.transform.position == p.To;
        return present;
    }

    public static K_Present Scale(Vector3 from, Vector3 to) {
        K_Present<Vector3> present = new K_Present<Vector3>(from, to);
        present.ExWork = (g, v, p) => g.transform.localScale = Vector3.Lerp(p.From, p.To, v);
        present.EndFlag = (g, p) => g.transform.localScale == p.To;
        return present;
    }

    public static K_Present Colorr(Color from, Color to) {
        K_Present<Color> present = new K_Present<Color>(from, to);
        present.ExWork = (g, v, p) => g.renderer.material.color = Color.Lerp(p.From, p.To, v);
        present.EndFlag = (g, p) => g.renderer.material.color == p.To;
        return present;
    }
}

public class K_Present<T> : K_Present
{
    public T From { get; set; }

    public T To { get; set; }

    public delegate void DoWork(GameObject go,float eval,K_Present<T> p);

    public delegate bool Flag(GameObject go,K_Present<T> p);
    
    public DoWork ExWork = (x, y, z) => {};
    public Flag StartFlag = (x, y) => true;
    public Flag EndFlag = (x, y) => true;

    public K_Present(T from, T to) {
        this.From = from;
        this.To = to;
    }

    public override void Work(GameObject go, float eval) {
        ExWork(go, eval, this);
    }

    public override bool Start(GameObject go) {
        return StartFlag(go, this); }

    public override bool End(GameObject go) {
        return EndFlag(go, this);
    }

    public K_Present<T> Next(T to) {
        return Next(To, to);
    }

    public K_Present<T> Next(T from, T to) {
        K_Present<T> p = MemberwiseClone() as K_Present<T>;
        p.From = from;
        p.To = to;
        return p;
    }

    public override string ToString() {
        return string.Format("[K_Present: From => {0} To => {1}] ", From, To); }
}
