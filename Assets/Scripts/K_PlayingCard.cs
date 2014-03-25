using System.Collections;
using Extensions;
using System.Linq;
using UnityEngine;

public class K_PlayingCard : MonoBehaviour
{
    public string Suit { private set; get; }

    public int Number{ private set; get; }

    public Vector2 Size{ private set; get; }

    public float ScaleN {set; get;}

    public Color HoverColor;

    public UIEventTrigger UI;

    public K_ReadyToWork RTW;

    public K_PlayingCard SetCard(string str, int num)
    {
        this.Suit = str;
        this.Number = num;
        this.Size = GetComponentInChildren<SpriteRenderer>().bounds.size;

        this.UI = this.GetComponent<UIEventTrigger>();
        this.RTW = transform.GetOrAddComponent<K_ReadyToWork>();

        return this;
    }

    public bool PairSuit(K_PlayingCard card)
    {
        return card.Suit.Equals(this.Suit);
    }

    public bool PairNumber(K_PlayingCard card)
    {
        return card.Number.Equals(this.Number);
    }

    public bool PairCard(K_PlayingCard card)
    {
        return !PairSuit(card) && PairNumber(card);
    }

    ////////////////////////////////////////////////

    void select(){
        RTW.LerpScale(new Vector3(ScaleN * 0.9f, ScaleN * 0.9f, 1f), K_TimeCurve.EaseOut(0.05f));
        RTW.LerpScale(new Vector3(ScaleN * 1.1f, ScaleN * 1.1f, 1f), K_TimeCurve.EaseOut(0.1f));
        RTW.Play();
    }

    void unselect(){
        RTW.LerpScale(new Vector3(ScaleN, ScaleN, 1f), K_TimeCurve.EaseOut(0.05f));
        RTW.Play();
    }

    void hover(){
        RTW.LerpColor(GetComponentInChildren<SpriteRenderer>().renderer.material, this.HoverColor, 0.5f);
        RTW.Play();
    }

    void hoverout(){
        RTW.LerpColor(GetComponentInChildren<SpriteRenderer>().renderer.material, Color.white, 0.5f);
        RTW.Play();
    }
}