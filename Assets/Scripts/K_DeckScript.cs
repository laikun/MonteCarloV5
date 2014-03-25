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

    Vector2 deckPosition;
    Vector2 nextCardPosition;

    // For ReGame
    public void ReInit(K_PlayingCard[] cards) {
        this.Init(cards, this.deckPosition);
    }

    public void Init(K_PlayingCard[] cards, Vector2 deckPosition)
    {
        K_OnStage.In(gameObject);
        foreach (K_PlayingCard card in cards){
            card.ScaleN = this.transform.localScale.x;
            card.transform.localScale = this.transform.localScale;
            card.transform.SetXY(this.transform.GetXY());
            K_OnStage.Out(card.gameObject);
        }
        cards.Shuffle();
        int sort = 0;
        Array.ForEach(cards, x => x.GetComponentInChildren<SpriteRenderer>().sortingOrder = sort++);
        this.cards = new Queue<K_PlayingCard>(cards);

        this.deckPosition = deckPosition;
        nextCardPosition = new Vector2(this.deckPosition.x - this.transform.localScale.x * 0.2f, this.deckPosition.y);
    }

    public void Clear(){
        this.cards.Clear();
        K_OnStage.Out(gameObject);
    }

    public K_PlayingCard Draw() {
        return cards.Count > 0 ? cards.Dequeue() : null;
    }

    public K_PlayingCard[] Draws(int count){
        K_PlayingCard[] cards = new K_PlayingCard[count];
        return cards.Select((c, i) => cards[i] = this.Draw()).ToArray();
    }
    ////////////////////////////////////////////////////////
    K_ReadyToWork RTW;
    void Start (){
        RTW = transform.GetOrAddComponent<K_ReadyToWork>();
    }

    void onDeckPosition(){
        Array.ForEach(Cards, x => {
            x.RTW.Delay(0.4f);
            K_OnStage.Out(x.gameObject);
            x.transform.SetXY(deckPosition);});
        RTW.LerfPosition(deckPosition.V3(this.transform.position), K_TimeCurve.EaseOut(0.4f));
        RTW.Play();
    }

    void showNextCard(){
        K_ReadyToWork rtw = cards.First().RTW;
        rtw.LerfPosition(new Vector3(nextCardPosition.x, nextCardPosition.y, rtw.transform.position.z), K_TimeCurve.EaseInOut(0.4f));
        rtw.Play();
    }

    void suffle(){

    }
}