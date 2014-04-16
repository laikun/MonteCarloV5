using System.Collections;
using Extensions;
using System.Linq;
using UnityEngine;
using System;

public class K_PlayingCard : MonoBehaviour, IComparer
{
    public string Suit { private set; get; }

    public int Number{ private set; get; }

    public Vector2 Size{ private set; get; }

    public float ScaleN { set; get; }
   
    public UIEventListener UI {private set;get; }
    public K_ReadyToWork RTW;
    SpriteRenderer SR;

    public K_PlayingCard SetCard(string str, int num) {
        this.Suit = str;
        this.Number = num;
        this.SR = GetComponentInChildren<SpriteRenderer>();
        this.Size = SR.bounds.size;

        this.UI = this.GetOrAddComponent<UIEventListener>();
        this.RTW = transform.GetOrAddComponent<K_ReadyToWork>();

        return this;
    }

    public bool PairSuit(K_PlayingCard card) {
        return card.Suit.Equals(this.Suit);
    }

    public bool PairNumber(K_PlayingCard card) {
        return card.Number.Equals(this.Number);
    }

    public bool PairCard(K_PlayingCard card) {
        return !PairSuit(card) && PairNumber(card);
    }

    public int Compare(object x, object y) {
        K_PlayingCard a = x as K_PlayingCard;
        K_PlayingCard b = y as K_PlayingCard;

        if (a.Number != b.Number)
            return (a.Number != 1 ? a.Number : 14) - (b.Number != 1 ? b.Number : 14);

        return (a.Suit.Equals("h") ? 1 : a.Suit.Equals("d") ? 2 : a.Suit.Equals("c") ? 3 : 4) - (b.Suit.Equals("h") ? 1 : b.Suit.Equals("d") ? 2 : b.Suit.Equals("c") ? 3 : 4);
    }

    public override string ToString() {
        return string.Format("[K_PlayingCard: Suit={0}, Number={1}]", Suit, Number);
    }    

    IEnumerator select() {
        Vector3 f = transform.localScale;
        Vector3 t = new Vector3(ScaleN * 0.9f, ScaleN * 0.9f, 1f);

        yield return StartCoroutine(this.LoopWork(x => transform.localScale = Vector3.Lerp(f, t, x), K_TimeCurve.EaseOut(0.05f)));

        f = transform.localScale;
        t = new Vector3(ScaleN * 1.05f, ScaleN * 1.05f, 1f);
        Quaternion ff = transform.localRotation;
        Quaternion tt = Quaternion.Euler(0f, 0f, -7f);

        yield return StartCoroutine(this.LoopWork(x => {
            transform.localScale = Vector3.Lerp(f, t, x);
            transform.localRotation = Quaternion.Lerp(ff, tt, x);
        }, K_TimeCurve.EaseInOut(0.1f)));
    }

    IEnumerator unselect() {
        Vector3 f = transform.localScale;
        Vector3 t = new Vector3(ScaleN, ScaleN, 1f);
        Quaternion ff = transform.localRotation;
        Quaternion tt = Quaternion.Euler(0f, 0f, 0f);

        yield return StartCoroutine(this.LoopWork(x => {
            transform.localScale = Vector3.Lerp(f, t, x);
            transform.localRotation = Quaternion.Lerp(ff, tt, x);
        }, K_TimeCurve.EaseInOut(0.05f)));
    }

    IEnumerator canselect() {

        Color f = SR.material.color;
        Color t = Color.white;

        yield return StartCoroutine(this.LoopWork(x => SR.material.color = Color.Lerp(f, t, x), K_TimeCurve.EaseInOut(0.2f)));
    }
    
    IEnumerator dontselect() {
        Color f = SR.material.color;
        Color t = Color.gray;
        
        yield return StartCoroutine(this.LoopWork(x => SR.material.color = Color.Lerp(f, t, x), K_TimeCurve.EaseInOut(0.2f)));
    }
    
    IEnumerator hover() { 
        Color f = SR.material.color;
        Color t = Color.cyan;
        
        yield return StartCoroutine(this.LoopWork(x => SR.material.color = Color.Lerp(f, t, x), K_TimeCurve.EaseInOut(0.2f)));
    }

    IEnumerator hoverout() {
        Color f = SR.material.color;
        Color t = Color.white;
        
        yield return StartCoroutine(this.LoopWork(x => SR.material.color = Color.Lerp(f, t, x), K_TimeCurve.EaseInOut(0.2f)));
    }
}