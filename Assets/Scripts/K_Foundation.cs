using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_Foundation : Singleton<K_Foundation>
{
    class Foundation
    {
        public K_PlayingCard card;
        public Vector2 position;
    }
    
    Foundation[] foundations;

    public Vector2 FoundInPosition{ private set; get; }

    public K_PlayingCard[] Cards { get { return foundations.Select(found => found.card).Where(card => card != null).ToArray(); } }

    public Vector2 Position(K_PlayingCard card)
    {
        return Array.Find(foundations, found => found.card.Equals(card)).position;
    }

    public float Scale{ private set; get; }

    public void Init(Vector2 size, int length)
    {
        foundations = new Foundation[length];
        Scale = this.transform.localScale.y / size.y;
        float unitx = this.transform.localScale.x / length;

        for (int i = 0; i < foundations.Length; i++)
        {
            foundations [i] = new Foundation();
            foundations [i].card = null;
            foundations [i].position = new Vector2((2 * i + 1) * unitx / 2 - this.transform.localScale.x / 2 - Math.Abs(this.transform.position.x), this.transform.position.y);
        }

        FoundInPosition = new Vector3(K_GameOptions.Instance.screenSize.x / 2 + size.x * Scale, this.transform.position.y);
    }

    public void Founding(K_PlayingCard[] cards)
    {
        Array.ForEach(cards, card => foundations.First(found => found.card == null).card = card);
        FoundIn(cards);
    }

    public void Clear()
    {
        Array.ForEach(foundations, x => x.card = null);
    }

    void FoundIn(K_PlayingCard[] cards)
    {
        cards = cards.OrderBy(card => card.transform.position.x).ToArray();
        int dly = 0;
        foreach (K_PlayingCard card in cards)
        {
            card.GetComponentInChildren<SpriteRenderer>().sortingOrder = Array.FindIndex(foundations, x => x.card.Equals(card));
            card.GetComponent<UIEventListener>().Init();
            card.RTW.Delay(0.04f * dly++);
            card.RTW.LerpPosition(new Vector3(-K_GameOptions.Instance.screenSize.x, card.transform.position.y, card.transform.position.z), K_TimeCurve.EaseIn(0.3f));
            card.RTW.MoreWork(x => {
                x.transform.SetScale(this.Scale);
                x.transform.SetXY(this.FoundInPosition);
            });
            card.RTW.LerpPosition(this.Position(card), K_TimeCurve.EaseOut(0.4f));
            card.RTW.GoWork();
        }
    }
}
