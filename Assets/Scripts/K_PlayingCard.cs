using System.Collections;
using Extensions;
using System.Linq;
using UnityEngine;

public class K_PlayingCard : MonoBehaviour
{
    public string Suit { private set; get; }

    public int Number{ private set; get; }

    public Vector2 Size{ private set; get; }

    public float ScaleN { set; get; }
   
    public UIEventTrigger UI;
    public K_ReadyToWork RTW;
    SpriteRenderer SR;

    public K_PlayingCard SetCard(string str, int num) {
        this.Suit = str;
        this.Number = num;
        this.SR = GetComponentInChildren<SpriteRenderer>();
        this.Size = SR.bounds.size;

        this.UI = this.GetComponent<UIEventTrigger>();
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

    ////////////////////////////////////////////////

    void select() {
        RTW.LerpScale(new Vector3(ScaleN * 0.9f, ScaleN * 0.9f, 1f), K_TimeCurve.EaseOut(0.05f));
        RTW.LerpScale(new Vector3(ScaleN * 1.1f, ScaleN * 1.1f, 1f), K_TimeCurve.EaseOut(0.1f));
        RTW.GoWork();
    }

    void unselect() {
        RTW.LerpScale(new Vector3(ScaleN, ScaleN, 1f), K_TimeCurve.EaseOut(0.05f));
        RTW.GoWork();
    }

    void hover() { 
        RTW.LerpColor(SR.material, Color.cyan, 0.2f);
        RTW.GoWork();
    }

    void hoverout() {
        RTW.LerpColor(SR.material, Color.white, 0.0f);
        RTW.GoWork();
    }
}