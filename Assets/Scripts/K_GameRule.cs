using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_GameRule : Singleton<K_GameRule>
{
    public K_Cell cell;
    public K_Foundation foundation;
    public K_PlayingCardManager cardManager;
    public K_DeckScript deck;
    public K_GameOptions options;
    public K_Score score;
    public K_Counter timeLimit;
    public K_Counter turnLimit;
    public K_Counter hintLimit;
    public UICamera uicamera;
    public GameObject TriggerGo;
    public GameObject TriggerRe;
    public GameObject Jumbotron;
    public K_Time time;
    Present present;
    K_ReadyToWork monoWork;

    class Present
    {
        K_GameRule rule = K_GameRule.Instance;
        UICamera input;

        public Present() {
            input = rule.uicamera;

        }

        private void inputAllow(bool on) {
            input.enabled = on;
            Debug.Log("Allow Input : " + on);
        }

        public void ReGame(Action action) {
//            inputAllow(false);
//
//            Array.ForEach(rule.deck.Cards, card => card.GetOrAddComponent<K_OnStage>().Out());
//            List<K_PlayingCard> temp = new List<K_PlayingCard>(rule.cardManager.Cards.Length);
//            temp.AddRange(rule.cell.Cards.Reverse());
//            temp.AddRange(rule.foundation.Cards.Reverse());
//
//            // imaiti...
//            temp.ForEach(card => card.transform.SetScale(rule.cell.Scale));
//
//            TweenPosition tw = TweenPosition.Begin(rule.deck.gameObject, 0.3f, Vector3.zero);
//            tw.method = UITweener.Method.EaseOut;
//            tw.onFinished.Add(new EventDelegate(() => {
//                foreach (K_PlayingCard card in temp) {
//                    TweenPosition tp = TweenPosition.Begin(card.gameObject, 0.5f, Vector3.zero);
//                    tp.delay = rule.time.NextDelayTime(0.03f);
//                    tp.PlayForward();
//                }
//            }));            
//
//            temp.Last().GetComponentInChildren<TweenPosition>().onFinished.Add(new EventDelegate(() => {
//                inputAllow(true);
//                Array.ForEach(rule.cardManager.Cards, card => {
//                    card.transform.localScale = Vector2.zero;
//                    card.gameObject.SetActive(true);
//                    card.GetOrAddComponent<K_OnStage>().In();
//                });
//                rule.cardManager.GetOrAddComponent<K_OnStage>().In();
//                action();
//            }));
        }
    }

    public void Ready() {
        K_OnStage.Init();
        monoWork = this.GetOrAddComponent<K_ReadyToWork>();
        cardManager.CreateCards();
        foundation.Init(cardManager.Size, cardManager.Cards.Length);
        cell.Init(options.GetOptValue("Column"), options.GetOptValue("Row"), cardManager.Size, options.separate);

        Vector2 deckPosition = cell.LastCellPosition;
        deckPosition.x += options.separate + cardManager.Size.x * cell.Scale * 1.2f;

        deck.transform.SetScale(cell.Scale);
        deck.Init(cardManager.Cards, deckPosition);

        present = new Present();

        TriggerRe.SetActive(false);
        TriggerGo.SetActive(true);

        timeLimit.Init(options.GetOptValue("TimeLimit"), () => GameOver("TIME OVER"));
        turnLimit.Init(options.GetOptValue("TurnLimit"), () => GameOver("TURN OVER"));
        hintLimit.Init(options.GetOptValue("Hint"), () => hintLimit.gameObject.SetActive(false));
    }

    public void GameOver(string str) {
        uicamera.enabled = false;
        Jumbotron.SetActive(true);
        Jumbotron.GetComponentInChildren<UILabel>().text = str;
        UIEventListener.Get(Jumbotron.gameObject).onClick += go => {
            uicamera.enabled = true;
            ReGame();
            //
            UIEventListener.Get(go).Init();
            go.SetActive(false);
        };
    }

    public void Play() {
        Action draw = () => {};

        TriggerGo.gameObject.SetActive(false);

        bool autoDraw = options.GetOptValue("AutoDraw") != 0 ? true : false;

        Func<K_PlayingCard[]> hit = () => {
            // left cards in cell
            List<K_PlayingCard> cards = new List<K_PlayingCard>(cell.Cards);
            return cards.Where((card, index) => 
                                               cards.Where((beta) => 
                        !beta.Equals(card) && beta.PairNumber(card) && cell.IsNeighbor(new K_PlayingCard[]
            {
                card,
                beta
            })).Count() > 0).ToArray();
        };

        UIEventListener.Get(hintLimit.gameObject).onClick += go => {
            Array.ForEach(hit(), card => card.SendMessage("select"));
            hintLimit.Down(-1);
        };

        // Check Selected Cards
        List<K_PlayingCard> selected = new List<K_PlayingCard>();

        Action<K_PlayingCard> select = card => {
            if (selected.Count > 0 && selected.Last().Equals(card)) {
                selected.RemoveAll(x => x.Equals(card));
                card.SendMessage("unselect");
            } else {
                selected.Add(card);
                card.SendMessage("select");
            }
        };

        Action reSelection = () => {
            if (selected.Count < 1)
                return;
            selected.Where(card => !card.Equals(selected.Last())).ToList().ForEach(card => card.SendMessage("unselect"));
            selected = new List<K_PlayingCard>(new K_PlayingCard[]{selected.Last()});
        };

        Func<bool> checkSelected = () => {
            if (selected.Count < 2)
                return false;

            if (!cell.IsNeighbor(selected.ToArray()) || 1 != selected.GroupBy(card => card.Number).Count()) {
                reSelection();
                return false;
            }

            return true;
        };

        // Founding card
        Action founding = () => {
            if (!checkSelected())
                return;

            K_PlayingCard[] foundingCards = selected.ToArray();
            selected.Clear();
            cell.Clear(foundingCards);

            foundation.Founding(foundingCards);

            Array.ForEach(foundingCards, card => {
                UIEventListener ui = UIEventListener.Get(card.gameObject).Init();
                ui.onHover += (x, b) => card.SendMessage(b ? "hover" : "hoverout");
            });

            score.Add(1);
            if (autoDraw)
                draw();
        };

        // Add Delegate Action To Card In Cell
        draw = () => {
            TriggerRe.SetActive(false);

            if (cell.BlankCell == 0)
                return;

            cell.DrawToCell(deck.Draws(cell.BlankCell));

            selected.Clear();

            // Check GameOVer
            if (hit().Length < 1) {
                GameOver("GAME OVER");
                return;
            }

            Array.ForEach(cell.Cards, card => {
                turnLimit.Reset();
                card.SendMessage("unselect");

                UIEventListener ui = UIEventListener.Get(card.gameObject);
                ui.Init();

                ui.onSelect += (x, b) => {
                    if (!b)
                        return;
                    select(card);
                    founding();
                };

                ui.onHover += (x, b) => card.SendMessage(b ? "hover" : "hoverout");
            });

            TriggerRe.SetActive(true);
        };

        deck.SendMessage("onDeckPosition");
        K_ReadyToWork rtw = K_ReadyToWork.All ["Deck"];
        monoWork.DelayWork(x => rtw.IsWorkDone, x => draw());
        monoWork.GoWork();
    }

    public void ReGame() {
        uicamera.enabled = false;

        Jumbotron.SetActive(false);
        TriggerRe.SetActive(false);

        Array.ForEach(cardManager.Cards, card => card.GetOrAddComponent<UIEventListener>().Init());

        deck.SendMessage("shuffle", new K_PlayingCard[]{}.Concat(foundation.Cards).Concat(cell.Cards).ToArray());

        // Sync Problem?
        cell.ClearAll();
        foundation.Clear();
        score.Reset();
        timeLimit.Reset();

    }

    public void End() {
        TriggerRe.SetActive(false);
        TriggerGo.SetActive(false);
        Jumbotron.SetActive(false);
        cell.ClearAll();
        foundation.Clear();
        deck.Clear();
        K_OnStage.Out(deck);
        Array.ForEach(cardManager.Cards, card => K_OnStage.Out(card));

        score.Reset();
        timeLimit.Stop();
    }

}
