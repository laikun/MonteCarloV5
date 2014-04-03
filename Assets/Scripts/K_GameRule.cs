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
    K_ReadyToWork monoWork;

    public double Score;

    public void Ready() {
        K_OnStage.Init();
        monoWork = this.GetOrAddComponent<K_ReadyToWork>();
        if (cardManager.Cards == null)
            cardManager.CreateCards();

        foundation.Init(cardManager.Size, cardManager.Cards.Length);
        cell.Init(options.GetOptValue("Column"), options.GetOptValue("Row"), cardManager.Size, options.separate);

        Vector2 deckPosition = cell.LastCellPosition;
        deckPosition.x += options.separate + cardManager.Size.x * cell.Scale * 1.2f;

        deck.transform.SetScale(cell.Scale);
        deck.Init(cardManager.Cards, deckPosition);

        TriggerRe.SetActive(false);
        TriggerGo.SetActive(true);

        UIEventListener.Get(TriggerGo).onClick += x => {
            this.Play();
            K_Flag.OnFlag("ReadyToStart", false);
        };        

        UIEventListener.Get(TriggerGo).onHover += (x, b) => {
            TweenColor.Begin(TriggerGo, 0.4f, b ? Color.red : Color.white).PlayForward();
        };        
        
        K_Flag.ConnectFlag("ReadyToStart", b => {
            TweenColor.Begin(TriggerGo, 0.1f, Color.white).PlayForward();
            TriggerGo.SetActive(b);
        });

        K_Flag.ConnectFlag("AllowReGame", b => {
            TriggerRe.SetActive(b);
        });

        K_Flag.ConnectFlag("ViewInfo", b => {
            Jumbotron.collider.enabled = b;
            Jumbotron.GetComponentInChildren<UILabel>().text = "";
        });

        UIEventListener.Get(TriggerGo).onClick += x => {
            this.Play();
            K_Flag.OnFlag("ReadyToStart", false);
        };

//        timeLimit.Init(options.GetOptValue("TimeLimit"), () => GameOver("TIME OVER"));
//        turnLimit.Init(options.GetOptValue("TurnLimit"), () => GameOver("TURN OVER"));
//        hintLimit.Init(options.GetOptValue("Hint"), () => hintLimit.gameObject.SetActive(false));
    }

    public void GameOver(string str) {
        Jumbotron.SetActive(true);
        Jumbotron.collider.enabled = true;
        Jumbotron.GetComponentInChildren<UILabel>().text = str;
        UIEventListener.Get(Jumbotron.gameObject).onClick += go => {
            ReGame();
            //
            UIEventListener.Get(go).Init();
            go.collider.enabled = false;
        };
    }

    public void Play() {
        Action draw = () => {};
        // left cards in cell
        Func<K_PlayingCard[]> findCombination = () => {return null;};
        // Founding card
        Action founding = () => {};
        // Select Card
        Action<K_PlayingCard> select = x => {};        
        Action selectReset = () => {};
        List<K_PlayingCard> selectedCards = new List<K_PlayingCard>();

//        K_Flag.OnFlag("InPlay", true);

        bool autoDraw = options.GetOptValue("AutoDraw") != 0 ? true : false;

        if (!autoDraw)
            UIEventListener.Get(deck.gameObject).onClick += x => draw();

        deck.SendMessage("onDeckPosition");
        K_ReadyToWork rtw = K_ReadyToWork.All ["Deck"];
        monoWork.DelayWork(x => rtw.IsWorkDone, x => draw());
        monoWork.GoWork();

        UIEventListener.Get(hintLimit.gameObject).onClick += go => {
            Array.ForEach(findCombination(), card => card.SendMessage("select"));
            hintLimit.Down(-1);
        };

        findCombination = () => {
            Dictionary<K_PlayingCard[], string> hit = new Dictionary<K_PlayingCard[], string>();

            Action<K_PlayingCard[]> linear;
            linear = z => {
                z = z.OrderBy(x => x.Number).OrderBy(x => x.Suit).ToArray();
                string rank = cardManager.Rank(z);
                if (!string.IsNullOrEmpty(rank) && hit.Keys.Where(x => x.SequenceEqual(z)).Count() == 0)
                    hit.Add(z, rank);
                foreach(K_PlayingCard y in cell.InLinearCards(z)) {
                    K_PlayingCard[] temp = z.Concat(new K_PlayingCard[]{y}).ToArray();
                    linear(temp);
                }
            };

            Array.ForEach(cell.Cards, x => linear(new K_PlayingCard[]{x}));

            if (hit.Count < 1)
                return null;

            Debug.Log("////////////////////////////////////////////");
            foreach( KeyValuePair<K_PlayingCard[], string> x in hit )            {
                Debug.Log("RANK : " + x.Value + " = CARDS : " + x.Key.ToList().ToString<K_PlayingCard>());
            }
            Debug.Log("////////////////////////////////////////////");

            return hit.First().Key;
        };
        
        select = card => {

            if (card == null)
                return;

            if (selectedCards.Count > 0 && selectedCards.Last().Equals(card))
                return;

            card.SendMessage("select");

            selectedCards.Add(card);
            K_PlayingCard[] cnslc = cell.InLinearCards(selectedCards.ToArray());

            // No Card In Linear
            if (cnslc.Length == 0) {
                founding();
                return;
            }

            // Check Rank
            if (selectedCards.Count > 1) {
                string tempRank = cardManager.Rank(selectedCards.ToArray());

                if (!string.IsNullOrEmpty(tempRank)) {

                    tempRank = cardManager.Rank(selectedCards.ToArray().Concat(cnslc).ToArray());

                    if (string.IsNullOrEmpty(tempRank)) {
                        founding();
                        return;
                    }
                }
            }                

            Array.ForEach(cell.Cards, x => {
                UIEventListener ui = UIEventListener.Get(x.gameObject).Init();
                // add select
                if (selectedCards.Contains(x)) {
                    x.SendMessage("hover");
                } else if (cnslc.Contains(x)) {
                    x.SendMessage("canselect");
                    ui.onSelect += (a, b) => {
                        if (b)
                            select(x);
                    };          
                    ui.onHover += (a, b) => card.SendMessage(b ? "hover" : "hoverout");
                } else {
                    x.SendMessage("dontselect");
                    ui.onSelect += (g, b) => {
                        selectReset();
                        select(x);
                    };
                }
            });

        };

        selectReset = () => {
            selectedCards.Clear();
            Array.ForEach(cell.Cards, card => {
                card.SendMessage("canselect");
                card.SendMessage("unselect");
                UIEventListener ui = card.GetOrAddComponent<UIEventListener>();
                ui.Init();                
                ui.onSelect += (x, b) => {
                    if (b)
                        select(card);
                };                
                ui.onHover += (x, b) => card.SendMessage(b ? "hover" : "hoverout");
            });
        };

        founding = () => {
            string rank = cardManager.Rank(selectedCards.ToArray());
            Debug.Log("RANK : " + rank);
            K_ReadyToWork jrtw = Jumbotron.transform.GetOrAddComponent<K_ReadyToWork>();

            if (string.IsNullOrEmpty(rank)) {
                Jumbotron.GetComponentInChildren<UILabel>().text = "No Pair";
            } else {
                Jumbotron.GetComponentInChildren<UILabel>().text = rank;         
                selectedCards.ForEach(x => x.SendMessage("hoverout"));
                foundation.Founding(selectedCards.ToArray());
                cell.Clear(selectedCards.ToArray());

                score.Add(cardManager.ScoreTable(rank));

                if (autoDraw)
                    draw();
            }
            
            selectReset();

            jrtw.Delay(0.7f);
            jrtw.MoreWork(x => Jumbotron.GetComponentInChildren<UILabel>().text = "");
            jrtw.GoWork();
        };

        // Add Delegate Action To Card In Cell
        draw = () => {
            TriggerRe.SetActive(false);

            if (cell.BlankCell == 0)
                return;

            cell.DrawToCell(deck.Draws(cell.BlankCell));

            // Check GameOVer
            if (findCombination() == null) {
                GameOver("GAME OVER");
                return;
            }

            selectReset();
        };
    }

    public void ReGame() {
        K_Flag.OnFlag("AllowReGame", false);
        K_Flag.OnFlag("ViewInfo", false);
        K_Flag.OnFlag("ReadyToStart", false);

        cell.ClearAll();
        foundation.Clear();
        score.Reset();
        timeLimit.Reset();

        deck.SendMessage("shuffle", cardManager.Cards);
    }

    public void End() {
        cell.ClearAll();
        foundation.Clear();
        deck.Clear();
        deck.transform.SetXY(Vector2.zero);
        K_OnStage.Out(deck);

        Array.ForEach(cardManager.Cards, card => K_OnStage.Out(card));

        score.Reset();

        K_Flag.OnFlag("ReadyToStart", false);
        K_Flag.OnFlag("AllowReGame", false);
        K_Flag.OnFlag("ViewInfo", false);
    }

}
