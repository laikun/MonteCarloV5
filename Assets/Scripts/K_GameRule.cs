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
    public K_PlayingCard playingCard;
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

    class Present
    {
        K_GameRule rule = K_GameRule.Instance;
        float scaleN;
        Vector2 nextCardPosition;
        Vector2 deckPosition;
        UICamera input;

        public Present()
        {
            input = rule.uicamera;

            scaleN = rule.cell.Scale * rule.options.separate;

            deckPosition = rule.cell.LastCellPosition;
            deckPosition.x += rule.options.separate + rule.playingCard.Size.x * rule.cell.Scale * 1.2f;
            
            nextCardPosition = new Vector2(deckPosition.x - rule.playingCard.Size.x * rule.cell.Scale * 0.2f, deckPosition.y);
        }

        private void inputAllow(bool on)
        {
            input.enabled = on;
            Debug.Log("Allow Input : " + on);
        }

        public void SelectCard(K_PlayingCard card)
        {
            K_PresentAction pa = K_PresentAction.Ready(card);
            pa.QuickScale(new Vector3(rule.cell.Scale - scaleN, rule.cell.Scale - scaleN, 1f)).Duration = 0.05f;
            pa.Next<Vector3>(new Vector3(rule.cell.Scale + scaleN, rule.cell.Scale + scaleN, 1f)).Duration = 0.1f;
        }

        public void UnSelectCard(K_PlayingCard card)
        {
            TweenScale.Begin(card.gameObject, 0.1f, new Vector2(rule.cell.Scale, rule.cell.Scale)).PlayForward();
        }

        public void ShowNextCard()
        {
            TweenPosition.Begin(rule.deck.Cards.Length > 0 ? rule.deck.Cards.First().gameObject : rule.deck.gameObject, 0.4f, nextCardPosition).PlayForward();
        }

        public void OnDeckPosition(Action afterAction)
        {
            inputAllow(false);
            Array.ForEach(rule.playingCard.Cards, card => card.GetOrAddComponent<K_OnStage>().OutStage());

            TweenPosition tw = TweenPosition.Begin(rule.deck.gameObject, 0.4f, new Vector3(deckPosition.x, deckPosition.y));
            tw.method = UITweener.Method.EaseOut;        
            tw.onFinished.Add(new EventDelegate(() => {
                foreach (K_PlayingCard card in rule.playingCard.Cards)
                {
                    card.transform.localScale = deckPosition;
                    card.GetOrAddComponent<K_OnStage>().InStage();
                }
                afterAction();
            }));
            tw.PlayForward();
        }

        public void ReGame(Action action)
        {
            inputAllow(false);

            Array.ForEach(rule.deck.Cards, card => card.GetOrAddComponent<K_OnStage>().OutStage());
            List<K_PlayingCard> temp = new List<K_PlayingCard>(rule.playingCard.Cards.Length);
            temp.AddRange(rule.cell.Cards.Reverse());
            temp.AddRange(rule.foundation.Cards.Reverse());

            // imaiti...
            temp.ForEach(card => card.transform.SetScale(rule.cell.Scale));

            TweenPosition tw = TweenPosition.Begin(rule.deck.gameObject, 0.3f, Vector3.zero);
            tw.method = UITweener.Method.EaseOut;
            tw.onFinished.Add(new EventDelegate(() => {
                foreach (K_PlayingCard card in temp)
                {
                    TweenPosition tp = TweenPosition.Begin(card.gameObject, 0.5f, Vector3.zero);
                    tp.delay = rule.time.NextDelayTime(0.03f);
                    tp.PlayForward();
                }
            }));            

            temp.Last().GetComponentInChildren<TweenPosition>().onFinished.Add(new EventDelegate(() => {
                inputAllow(true);
                Array.ForEach(rule.playingCard.Cards, card => {
                    card.transform.localScale = Vector2.zero;
                    card.gameObject.SetActive(true);
                    card.GetOrAddComponent<K_OnStage>().InStage();
                });
                rule.playingCard.GetOrAddComponent<K_OnStage>().InStage();
                action();
            }));
        }
    }

    public void Ready()
    {
        playingCard.CreateCards();
        foundation.Init(playingCard.Size, playingCard.Cards.Length);
        cell.Init(options.GetOptValue("Column"), options.GetOptValue("Row"), playingCard.Size, options.separate);
        deck.transform.localScale = new Vector2(cell.Scale, cell.Scale);
        Debug.Log(deck.transform.localScale);
        deck.Init(playingCard.Cards);
        Debug.Log(deck.transform.localScale);

        present = new Present();

        TriggerRe.SetActive(false);
        TriggerGo.SetActive(true);
        Array.ForEach(playingCard.Cards, card => card.GetOrAddComponent<K_OnStage>().InStage());

        timeLimit.Init(options.GetOptValue("TimeLimit"), () => GameOver("TIME OVER"));
        turnLimit.Init(options.GetOptValue("TurnLimit"), () => GameOver("TURN OVER"));
        hintLimit.Init(options.GetOptValue("Hint"), () => hintLimit.gameObject.SetActive(false));
    }

    public void GameOver(string str)
    {
        uicamera.enabled = false;
        Jumbotron.SetActive(true);
        Jumbotron.GetComponentInChildren<UILabel>().text = str;
        UIEventListener.Get(Jumbotron.gameObject).onClick += go => {
            uicamera.enabled = true;
            ReGame();
            //
            UIEventListener.Get(go).ResetEventListener();
            go.SetActive(false);
        };
    }

    public void Play()
    {
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
            Array.ForEach(hit(), card => present.SelectCard(card));
            hintLimit.Down(-1);
        };

        // Check Selected Cards
        List<K_PlayingCard> selected = new List<K_PlayingCard>();

        Action<K_PlayingCard> select = card => {
            if (selected.Count > 0 && selected.Last().Equals(card))
            {
                selected.RemoveAll(x => x.Equals(card));
                present.UnSelectCard(card);
            } else
            {
                selected.Add(card);
                present.SelectCard(card);
            }
        };

        Action reSelection = () => {
            if (selected.Count < 1)
                return;
            selected.Where(card => !card.Equals(selected.Last())).ToList().ForEach(card => present.UnSelectCard(card));
            selected = new List<K_PlayingCard>(new K_PlayingCard[]{selected.Last()});
        };

        Func<bool> checkSelected = () => {
            if (selected.Count < 2)
                return false;

            if (!cell.IsNeighbor(selected.ToArray()) || 1 != selected.GroupBy(card => card.Number).Count())
            {
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
                UIEventListener ui = UIEventListener.Get(card.gameObject);
                ui.onClick = null;
                ui.onSelect = null;
                ui.onHover = null;
                ui.onPress = null;

                card.GetComponentInChildren<UIButton>().enabled = false;
            });

            score.Add(1);
            if (autoDraw)
                draw();
        };

        int drawCount = 0;

        // Add Delegate Action To Card In Cell
        draw = () => {
            TriggerRe.SetActive(false);

            if (cell.BlankCell == 0)
                return;

            cell.Pack();
            K_PlayingCard[] cards = new K_PlayingCard[cell.BlankCell];
            for (int i=0; i<cards.Length; i++)
            {
                cards [i] = deck.Draw();
            }
            cell.DrawToCell(cards);

            selected.Clear();

            // Check GameOVer
            if (hit().Length < 1)
            {
                GameOver("GAME OVER");
                return;
            }

            Array.ForEach(cell.Cards, card => {
                turnLimit.Reset();
                present.UnSelectCard(card);
                card.GetComponentInChildren<UIButton>().enabled = true;

                UIEventListener ui = UIEventListener.Get(card.gameObject);
                ui.ResetEventListener();

                ui.onSelect += (x, b) => {
                    if (!b)
                        return;
                    select(card);
                    founding();
                };

//                ui.onHover += (x, b) => {
//                    if (!b)
//                        return;
//                    selected.Add(x.GetComponentInChildren<K_PlayingCard>());
//                    present.SelectCard(selected.Last());
//                };
//
//                ui.onPress += (x, b) => {
//                    if (!b)
//                        foundion();
//                };
            });

            cell.Cards.Last().GetComponentInChildren<TweenPosition>().onFinished.Add(new EventDelegate(() => {
                turnLimit.Down();
                present.ShowNextCard();
                uicamera.enabled = true;
            }));

            TriggerRe.SetActive(true);
        };

        present.OnDeckPosition(() => {
            timeLimit.Down();
            draw();
        });
    }

    public void ReGame()
    {
        uicamera.enabled = false;

        Jumbotron.SetActive(false);
        TriggerRe.SetActive(false);

        Array.ForEach(playingCard.Cards, card => card.GetOrAddComponent<UIEventListener>().ResetEventListener());

        present.ReGame(() => {
            uicamera.enabled = true;
            TriggerGo.SetActive(true);
            deck.Init(playingCard.Cards);
        });

        // Sync Problem?
        cell.ClearAll();
        foundation.Clear();
        score.Reset();
        timeLimit.Reset();
    }

    public void End()
    {
        TriggerRe.SetActive(false);
        TriggerGo.SetActive(false);
        Jumbotron.SetActive(false);
        cell.ClearAll();
        foundation.Clear();
        deck.Clear();
        deck.GetOrAddComponent<K_OnStage>().OutStage();
        Array.ForEach(playingCard.Cards, card => card.GetOrAddComponent<K_OnStage>().OutStage());

        score.Reset();
        timeLimit.Stop();
    }

}
