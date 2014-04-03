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

    public K_PlayingCard[] Cards { get { return _cards.ToArray(); } }

    Vector2 deckPosition;
    Vector2 nextCardPosition;

    // For ReGame
    public void ReInit(K_PlayingCard[] cards) {
        this.Init(cards, this.deckPosition);
    }

    public void Init(K_PlayingCard[] cards, Vector2 deckPosition) {
        cards.Shuffle();
        K_OnStage.In(gameObject);
        RTW.LerpAlpha(0f, 1f, K_TimeCurve.Linear(0.5f));
        RTW.GoWork();
        int sort = 0;
        foreach (K_PlayingCard card in cards) {
            card.ScaleN = this.transform.localScale.x;
            card.transform.localScale = this.transform.localScale;
            card.transform.SetXY(this.transform.GetXY());
            K_OnStage.Out(card.gameObject);
            card.GetComponentInChildren<SpriteRenderer>().sortingOrder = sort++;
        }
        this._cards = new Queue<K_PlayingCard>(cards);

        this.deckPosition = deckPosition;
        nextCardPosition = new Vector2(this.deckPosition.x - this.transform.localScale.x * 0.2f, this.deckPosition.y);
    }

    public void Clear() {
        this._cards.Clear();
        K_OnStage.Out(gameObject);
    }

    public K_PlayingCard Draw() {
        return _cards.Count > 0 ? _cards.Dequeue() : null;
    }

    public K_PlayingCard[] Draws(int count) {
        K_PlayingCard[] cards = new K_PlayingCard[count];
        return cards.Select((c, i) => cards [i] = this.Draw()).ToArray();
    }
    ////////////////////////////////////////////////////////
    K_ReadyToWork RTW;

    void Start() {
        RTW = transform.GetOrAddComponent<K_ReadyToWork>();
    }

    void onDeckPosition() {
        Array.ForEach(Cards, x => {
            K_OnStage.Out(x.gameObject);
            x.RTW.Delay(0.4f);
            x.transform.SetXY(deckPosition);});

        RTW.LerpPosition(deckPosition.V3(this.transform.position), K_TimeCurve.EaseOut(0.4f));
        RTW.GoWork();
    }

    void showNextCard() {
        K_ReadyToWork rtw = _cards.First().RTW;
        rtw.LerpPosition(new Vector3(nextCardPosition.x, nextCardPosition.y, rtw.transform.position.z), K_TimeCurve.EaseInOut(0.4f));
        rtw.GoWork();
    }

    // ReDeck
    void shuffle(K_PlayingCard[] cards) {
        Func<float, float, float> r = (n, m) => UnityEngine.Random.Range(n, m);
        Action<K_PlayingCard, K_DeckScript> w = (c, d) => {
            Vector2 op = c.transform.GetXY() != Vector2.zero ? c.transform.GetXY() : new Vector2(r(-0.5f, 0.5f), r(-0.5f, 0.5f));
            float distance = Vector2.Distance(op, d.transform.position);
            float theta = Mathf.Atan2(op.y, op.x);
            Vector3 beta = new Vector3(Mathf.Cos(theta) * distance * r(1.1f, 1.4f) + r(-0.3f, 0.3f),
                                       Mathf.Sin(theta) * distance * r(1.1f, 1.4f) + r(-0.3f, 0.3f),
                                       c.transform.position.z);
            Func<Vector3> s = () => c.transform.position;
            Action<Vector3, Vector3, float> loop = (f, t, v) => c.transform.position = Vector3.Lerp(f, t, v);
            K_ReadyToWork.TweenAnimate(c.RTW, s, loop, () => beta, s, K_TimeCurve.EaseOut(r(0.3f, 0.4f)));
            c.RTW.Delay(r(0.0f, 0.1f));
            K_ReadyToWork.TweenAnimate(c.RTW, s, loop, () => new Vector3(this.transform.position.x, this.transform.position.y, c.transform.position.z), s, K_TimeCurve.EaseIn(r(0.3f, 0.4f)));
        };
        RTW.MoreWork(x => K_Flag.OnFlag("ReadyToStart", false));
        RTW.LerpPosition(new Vector3(this.transform.position.x, K_GameOptions.Instance.screenSize.y / 1.5f, this.transform.position.z), K_TimeCurve.EaseIn(0.3f));
        RTW.LerpPosition(new Vector3(0f, K_GameOptions.Instance.screenSize.y, this.transform.position.z), new Vector3(0f, 0f, this.transform.position.z), K_TimeCurve.EaseIn(0.3f));
        RTW.MoreWork(x => Array.ForEach(this._cards.ToArray(), card => {
            card.transform.SetXY(this.transform.GetXY());
            K_OnStage.In(card);
        }));
        RTW.MoreWork(x => Array.ForEach(cards, card => {
            card.RTW.WorkGone();
            w(card, this);
            card.RTW.ForceWork();
        }));
        RTW.Delay(1f);
        RTW.MoreWork(x => {
            RTW.NoIntercept = false;
            this.ReInit(cards);
            K_Flag.OnFlag("ReadyToStart", true);
        });
        RTW.GoWork();
        RTW.NoIntercept = true;
    }
}