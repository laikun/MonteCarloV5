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

    public static K_TimeCurve Linear(float duration) {
        return new K_TimeCurve(NormalCurve.Linear, duration);
    }

    public static K_TimeCurve EaseInOut(float duration) {
        return new K_TimeCurve(NormalCurve.EaseInOut, duration);
    }

    public static K_TimeCurve EaseIn(float duration) {
        return new K_TimeCurve(NormalCurve.EaseIn, duration);
    }

    public static K_TimeCurve EaseOut(float duration) {
        return new K_TimeCurve(NormalCurve.EaseOut, duration);
    }

    private K_TimeCurve(AnimationCurve curve = null, float duration = 1f) {
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
