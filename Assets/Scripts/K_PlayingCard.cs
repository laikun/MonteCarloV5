using System.Collections;
using System.Linq;
using UnityEngine;

public class K_PlayingCard : MonoBehaviour
{
    public string Suit { private set; get; }

    public int Number{ private set; get; }

    public Vector2 Size{ private set; get; }

    public Color HoverColor;

    public UIEventTrigger UI;

    public K_PlayingCard SetCard(string str, int num)
    {
        this.Suit = str;
        this.Number = num;
        this.Size = GetComponentInChildren<SpriteRenderer>().bounds.size;

        this.UI = this.GetComponent<UIEventTrigger>();


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
}
