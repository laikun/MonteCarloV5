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
    public UICamera uicamera;
    public GameObject TriggerGo;
    public GameObject TriggerRe;
    public GameObject Jumbotron;
    K_ReadyToWork monoWork;

    #region Ready

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

        autoDraw = options.GetOptValue("AutoDraw") != 0 ? true : false;

        UIEventListener.Get(TriggerGo).onClick = x => {
            StartCoroutine(Play());
            K_Flag.On("ReadyToStart", false);
        };        

        UIEventListener.Get(TriggerGo).onHover += (x, b) => {
            TweenColor.Begin(TriggerGo, 0.4f, b ? Color.red : Color.white).PlayForward();
        };        
        
        K_Flag.Set("ReadyToStart", b => {
            TweenColor.Begin(TriggerGo, 0.1f, Color.white).PlayForward();
            TriggerGo.SetActive(b == 1);
        });

        K_Flag.Set("AllowReGame", b => {
            TriggerRe.SetActive(b == 1);
        });

        K_Flag.Set("ViewInfo", b => {
            Jumbotron.collider.enabled = b == 1;
            Jumbotron.GetComponentInChildren<UILabel>().text = "";
        });

        UIEventListener.Get(TriggerGo).onClick += x => {
            StartCoroutine(Play());
            K_Flag.On("ReadyToStart", false);
        };

        #region GameOver
        K_Flag.Set("GameOver", f => {
            if (f == 1) {
                Jumbotron.SetActive(true);
                Jumbotron.collider.enabled = true;
                Jumbotron.GetComponentInChildren<UILabel>().text = "GAME OVER";
                UIEventListener.Get(Jumbotron.gameObject).onClick += go => {
                    K_Flag.On("GameOver", false);
                    ReGame();
                    //
                    UIEventListener.Get(go).Init();
                    go.collider.enabled = false;
                };
            } else if (f == 0) {
                Jumbotron.collider.enabled = false;
                Jumbotron.GetComponentInChildren<UILabel>().text = "";
            }
        });
        #endregion
        K_Flag.Set("GetHint", f => getHint(f));
        K_Flag.Set("InnerPause", f => Camera.main.GetComponent<UICamera>().enabled = f != 1);
        K_Flag.On("Ready", true);
    }
    #endregion

    bool autoDraw;
    List<K_PlayingCard> selectedCards = new List<K_PlayingCard>();

    IEnumerator Play() {
        if (!autoDraw)
            UIEventListener.Get(deck.gameObject).onClick += x => StartCoroutine(draw());

        deck.SendMessage("onDeckPosition");

        yield return StartCoroutine(this.WaitWork(() => !deck.enabled));

        yield return StartCoroutine(draw());
    }

    #region Enable Combinations
    K_PlayingCard[] enableCombi;
    K_PlayingCard[] findCombination() {
        Dictionary<K_PlayingCard[], string> hit = new Dictionary<K_PlayingCard[], string>();
        
        Action<K_PlayingCard[]> linear;
        linear = z => {
            z = z.OrderBy(x => x.Number).OrderBy(x => x.Suit).ToArray();
            string rank = cardManager.Rank(z);
            if (!string.IsNullOrEmpty(rank) && cell.IsNeighbor(z) && hit.Keys.Where(x => x.SequenceEqual(z)).Count() == 0)
                hit.Add(z, rank);
            foreach (K_PlayingCard y in cell.InLinearCards(z)) {
                K_PlayingCard[] temp = z.Concat(new K_PlayingCard[]{y}).ToArray();
                linear(temp);
            }
        };
        
        Array.ForEach(cell.Cards, x => linear(new K_PlayingCard[]{x}));
        
        if (hit.Count < 1)
            return null;

        enableCombi = hit.First(x => cardManager.ScoreTable(x.Value) == hit.Min(y => cardManager.ScoreTable(y.Value))).Key;

        Debug.Log("////////////////////////////////////////////");
        foreach (KeyValuePair<K_PlayingCard[], string> x in hit) {
            Debug.Log("Enable Combis : " + x.Value + " = CARDS : " + x.Key.ToList().ToString<K_PlayingCard>());
        }
        Debug.Log("////////////////////////////////////////////");
        
        return hit.First().Key;
    }
    void getHint(int f){
        if (enableCombi == null)
            return;
        Array.ForEach(enableCombi, x => x.SendMessage("hover"));
        enableCombi = null;
        K_Flag.On("SetHint", --f);
    }
    #endregion

    #region selectCard
    void select(K_PlayingCard card) {
        
        if (card == null)
            return;
        
        if (card.Equals(selectedCards.LastOrDefault()))
            return;
        
        card.SendMessage("select");
        
        selectedCards.Add(card);
        K_PlayingCard[] cnslc = cell.InLinearCards(selectedCards.ToArray());
        
        // No Card In Linear
        if (cnslc.Length == 0) {
            StartCoroutine(founding());
            return;
        }
        
        // Check Rank
        if (selectedCards.Count > 1) {
            string tempRank = cardManager.Rank(selectedCards.ToArray());
            
            if (!string.IsNullOrEmpty(tempRank)) {
                
                tempRank = cardManager.Rank(selectedCards.ToArray().Concat(cnslc).ToArray());
                
                if (string.IsNullOrEmpty(tempRank)) {
                    Debug.Log("CHECK"); 
                    StartCoroutine(founding());
                    return;
                }
            }
        }                
        
        Array.ForEach(cell.Cards, x => {
            // add select
            if (selectedCards.Contains(x)) {
                card.SendMessage("hoverout");
                x.UI.onHover = null;
                x.UI.onSelect = null;
            } else if (cnslc.Contains(x)) {
                x.SendMessage("canselect");
                x.UI.onSelect = (a, b) => {
                    if (b)
                        select(x);
                };          
                x.UI.onHover = (g, b) => g.SendMessage(b ? "hover" : "hoverout");
            } else {
                x.SendMessage("dontselect");
                x.UI.onHover = null;
                x.UI.onSelect = (g, b) => {
                    selectReset();
                    select(x);
                };
            }
        });
        
    }
    #endregion

    #region Draw
    IEnumerator draw() {
        TriggerRe.SetActive(false);
        K_Flag.On("InnerPause", true);
        K_Flag.On("AllowReGame", false);

        if (cell.BlankCell == 0)
            yield break;
        
        yield return StartCoroutine(cell.DrawToCell(deck.Draws(cell.BlankCell)));

        selectReset();

        K_Flag.On("InnerPause", false);
        K_Flag.On("AllowReGame", true);

        // Check GameOVer
        if (findCombination() == null) {
            K_Flag.On("GameOver", true);
        }
    }

    void selectReset() {
        selectedCards.Clear();
        
        Array.ForEach(cell.Cards, card => {
            card.SendMessage("unselect");
            card.SendMessage("canselect");
            card.UI.Init();
            card.UI.onClick = x => select(card);
            card.UI.onSelect = (x, b) => {
                if (b)
                    select(card);
            };                
            card.UI.onHover = (x, b) => x.SendMessage(b ? "hover" : "hoverout");
        });
    }
    #endregion
    IEnumerator founding() {
        string rank = cardManager.Rank(selectedCards.ToArray());
        Debug.Log("RANK : " + rank);

        yield return StartCoroutine(this.LoopWork(() => {}, () => !selectedCards.All(x => x.enabled == true)));

        if (string.IsNullOrEmpty(rank)) {
            Jumbotron.GetComponentInChildren<UILabel>().text = "No Pair";
        } else {
            Jumbotron.GetComponentInChildren<UILabel>().text = rank;

            selectedCards.ForEach(x => x.SendMessage("hoverout"));
            foundation.Founding(selectedCards.ToArray());
            cell.Clear(selectedCards.ToArray());            
            score.Add(cardManager.ScoreTable(rank));
            
            if (autoDraw)
                StartCoroutine(draw());
        }
        
        selectReset();

        yield return new WaitForSeconds(0.7f);

        if (K_Flag.State("GameOver") == 0)
            Jumbotron.GetComponentInChildren<UILabel>().text = "";
    }

    public void ReGame() {
        K_Flag.On("InnerPause", true);
        K_Flag.On("AllowReGame", false);
        K_Flag.On("ViewInfo", false);
        K_Flag.On("ReadyToStart", false);
        K_Flag.On("Ready", true);

        cell.ClearAll();
        foundation.Clear();
        score.Reset();

            
        deck.SendMessage("shuffle", cardManager.Cards);
    }

    public void End() {
        cell.ClearAll();
        foundation.Clear();
        deck.Clear();
        K_OnStage.Out(deck);
        deck.transform.SetXY(Vector2.zero);

        Array.ForEach(cardManager.Cards, card => K_OnStage.Out(card));

        score.Reset();

        K_Flag.On("ReadyToStart", false);
        K_Flag.On("AllowReGame", false);
        K_Flag.On("ViewInfo", false);
    }

}
