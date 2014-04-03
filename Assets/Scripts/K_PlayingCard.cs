using System.Collections;
using Extensions;
using System.Linq;
using UnityEngine;

public class K_PlayingCard : MonoBehaviour, IComparer
{
    public string Suit { private set; get; }

    public int Number{ private set; get; }

    public Vector2 Size{ private set; get; }

    public float ScaleN { set; get; }
   
    public UIEventListener UI;
    public K_ReadyToWork RTW;
    SpriteRenderer SR;

    public K_PlayingCard SetCard(string str, int num) {
        this.Suit = str;
        this.Number = num;
        this.SR = GetComponentInChildren<SpriteRenderer>();
        this.Size = SR.bounds.size;

        this.UI = this.GetComponent<UIEventListener>();
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

    ////////////////////////////////////////////////

    void select() {
        RTW.LerpScale(new Vector3(ScaleN * 0.9f, ScaleN * 0.9f, 1f), K_TimeCurve.EaseOut(0.05f));
        RTW.LerpScale(new Vector3(ScaleN * 1.15f, ScaleN * 1.15f, 1f), K_TimeCurve.EaseOut(0.1f));
        RTW.GoWork();
    }

    void unselect() {
        RTW.LerpScale(new Vector3(ScaleN, ScaleN, 1f), K_TimeCurve.EaseOut(0.05f));
        RTW.GoWork();
    }

    void canselect() {
        RTW.LerpColor(SR.material, Color.white, 0.2f);
        RTW.GoWork();
    }
    
    void dontselect() {
        RTW.LerpColor(SR.material, Color.gray, 0.2f);
        RTW.GoWork();
    }
    
    void hover() { 
        RTW.LerpColor(SR.material, Color.cyan, 0.2f);
        RTW.GoWork();
    }

    void hoverout() {
        RTW.LerpColor(SR.material, Color.white, 0.2f);
        RTW.GoWork();
    }
}