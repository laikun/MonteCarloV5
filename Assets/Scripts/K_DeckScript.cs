using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_DeckScript : Singleton<K_DeckScript>
{
    public UIEventTrigger UI { get { return this.GetComponent<UIEventTrigger>(); } }

    private Queue<K_PlayingCard> cards;

    public K_PlayingCard[] Cards {get {return cards.ToArray();}}
    
    public void Init(IList<K_PlayingCard> cards)
    {
//        this.transform.SetXY(Vector2.zero);
        foreach (K_PlayingCard card in cards){
            card.transform.localScale = this.transform.localScale;
            card.gameObject.layer = this.gameObject.layer;
            card.transform.SetXY(this.transform.GetXY());
        }

        cards.Shuffle();
        this.cards = new Queue<K_PlayingCard>(cards);
        this.GetOrAddComponent<K_OnStage>().InStage();
    }

    public void Clear(){
        this.cards.Clear();
        this.GetOrAddComponent<K_OnStage>().OutStage();
    }

    public K_PlayingCard Draw() {
        return cards.Count > 0 ? cards.Dequeue() : null;
    }

    public K_PlayingCard[] Get(int few){
        return null;
    }
}
