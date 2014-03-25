using System;
using UnityEngine;
using System.Collections;

public class K_OverLap : MonoBehaviour
{
    public GameObject alpha;
    public GameObject beta;

    void Go(bool direct) {
        TweenAlpha ta = TweenAlpha.Begin(direct ? alpha : beta, 0.4f, 0f);
        TweenAlpha tb = TweenAlpha.Begin(direct ? beta : alpha, 0.4f, 1f);
        tb.delay = 0.5f;
        ta.PlayForward();
        tb.PlayForward();
    }

    public void Go(){
        this.Go(true);
    }

    public void Back(){
        this.Go(false);
    }    
}