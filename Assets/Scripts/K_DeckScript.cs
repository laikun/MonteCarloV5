using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_DeckScript : Singleton<K_DeckScript>
{
    public UIEventTrigger UI { get { return this.GetComponent<UIEventTrigger>(); } }

    private Queue<K_PlayingCard> _cards;

    public K_PlayingCard[] Cards {get {return _cards.ToArray();}}

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
        this._cards = new Queue<K_PlayingCard>(cards);

        this.deckPosition = deckPosition;
        nextCardPosition = new Vector2(this.deckPosition.x - this.transform.localScale.x * 0.2f, this.deckPosition.y);
    }

    public void Clear(){
        this._cards.Clear();
        K_OnStage.Out(gameObject);
    }

    public K_PlayingCard Draw() {
        return _cards.Count > 0 ? _cards.Dequeue() : null;
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
        RTW.LerpPosition(deckPosition.V3(this.transform.position), K_TimeCurve.EaseOut(0.4f));
        RTW.GoWork();
    }

    void showNextCard(){
        K_ReadyToWork rtw = _cards.First().RTW;
        rtw.LerpPosition(new Vector3(nextCardPosition.x, nextCardPosition.y, rtw.transform.position.z), K_TimeCurve.EaseInOut(0.4f));
        rtw.GoWork();
    }

    // ReDeck
    void shuffle(K_PlayingCard[] cards){
        Func<float, float, float> r = (n, m) => UnityEngine.Random.Range(n, m);
        Action<K_PlayingCard, K_DeckScript> w = (c, d) => {
            float distance = Vector2.Distance(c.transform.position, d.transform.position);
            float theta = Mathf.Atan2(c.transform.position.y, c.transform.position.x);
            Vector3 beta = new Vector3(Mathf.Cos(theta) * distance * r(1.1f, 1.4f) + r(-0.3f, 0.3f),
                                       Mathf.Sin(theta) * distance * r(1.1f, 1.4f) + r(-0.3f, 0.3f),
                                       c.transform.position.z);
            c.RTW.LerpPosition(c.transform.position, beta, K_TimeCurve.EaseOut(0.5f));
        };

        Array.ForEach(this._cards.ToArray(), card => {
            card.RTW.WorkGone();
            card.RTW.MoreWork(c => {
                c.transform.SetXY(Vector2.zero);
                K_OnStage.In(c);
            });
            card.RTW.LerpPosition(new Vector3(r(-0.4f, 0.4f), r(-0.4f, 0.4f), card.transform.position.z), K_TimeCurve.EaseOut(0.4f));
            card.RTW.LerpPosition(Vector2.zero, K_TimeCurve.EaseIn(r(0.2f, 0.4f)));
        });
        Array.ForEach(cards, card => {
            card.RTW.WorkGone();
            card.RTW.MoreWork(x => w(card, this));
            card.RTW.LerpPosition(Vector2.zero, K_TimeCurve.EaseIn(r(0.2f, 0.4f)));
        });

        RTW.LerpPosition(new Vector3(this.transform.position.x, K_GameOptions.Instance.screenSize.y / 1.5f, this.transform.position.z), K_TimeCurve.EaseIn(0.3f));
        RTW.LerpPosition(new Vector3(0f, K_GameOptions.Instance.screenSize.y, this.transform.position.z), Vector3.zero, K_TimeCurve.EaseIn(0.3f));
        RTW.MoreWork(x => Array.ForEach(cards.Concat(Cards).ToArray(), card => card.RTW.ForceWork()));
        RTW.MoreWork(x => RTW.NoIntercept = false);
        RTW.GoWork();
        RTW.NoIntercept = true;
    }
}