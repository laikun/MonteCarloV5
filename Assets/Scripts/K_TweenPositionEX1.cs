using UnityEngine;

public class K_TweenPositionEX1 : MonoBehaviour
{
    [HideInInspector]
    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
    [HideInInspector]
    public UITweener.Style style;
    [HideInInspector]
    public bool ignoreTimeScale;
    [HideInInspector]
    public int tweenGroup;
    [HideInInspector]
    public float duration;
    [HideInInspector]
    public float delay;
    [HideInInspector]
    private TweenPosition tw;

    private void GoScreen(int direct){
        Vector2 to = new Vector2((direct == 0 ? 1 : direct == 1 ? -1 : 0) * K_GameOptions.Instance.screenSize.x * 100f,
                                 (direct == 2 ? 1 : direct == 3 ? -1 : 0) * K_GameOptions.Instance.screenSize.y * 100f);
        NGUIDebug.Log(K_GameOptions.Instance.screenSize);
        tw = TweenPosition.Begin(this.gameObject, duration * (direct == 0 || direct == 1 ? Camera.main.aspect : 1), to);
        tw.animationCurve = this.animationCurve;
        tw.delay = this.delay;

        tw.PlayForward();
    }

    public void GoScreenRight(){
        this.GoScreen(0);
    }

    public void GoScreenLeft(){
        this.GoScreen(1);
    }

    public void GoScreenUp(){
        this.GoScreen(2);
    }

    public void GoScreenDown(){
        this.GoScreen(3);
    }

    public void GoScreenBack(){
        tw.PlayReverse();
    }
}

