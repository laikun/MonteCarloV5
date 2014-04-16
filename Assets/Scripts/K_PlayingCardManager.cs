using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Extensions;

public class K_PlayingCardManager : MonoBehaviour
{
    public K_PlayingCard[] Cards{ private set; get; }
    
    public GameObject prefab;

    public Vector2 Size{ private set; get; }

    public string Rank(K_PlayingCard[] cards) {

        if (cards.Length < 2)
            return null;

        // Count Number Of Cards
        int nums = cards.GroupBy(x => x.Number).Count();
        
        // Check Pair
        if (cards.Length < 3)
            return nums != 1 ? null : "One Pair";
        
        // Check Three of a Kind
        if (cards.Length < 4)
            return nums != 1 ? null : "Three Of A Kind";
        
        // Count Suits Of Cards
        int suits = cards.GroupBy(x => x.Suit).Count();
        
        // Check Two Pair or Full House
        if (nums == 2 && cards[0].PairCard(cards[1]) && cards[cards.Length -1].PairCard(cards[cards.Length -2]))
            return cards.Length != 5 ? "Two Pair" : "Full House";
        
        // Check Four Of A Kind
        if (cards.Length == 4 && nums == 1 && suits == 4)
            return "Four Of A Kind";
        
        // Check Straight
        if (nums != cards.Length)
            return null;
        
        cards = cards.OrderBy(x => x.Number).ToArray();
        int[] dis = cards.Skip(1).Select((x, i) => Mathf.Abs(x.Number - cards[i].Number))
            .GroupBy(x => x).Select(x => x.Key).ToArray();
        
        // Check Straight
        if (dis[0] == 1 && (dis.Length > 1 ? dis[1] == 12 : true)){
            if (suits != 1)
                return "Straight";
            
            return cards[0].Number != 1 ? "Straight Flush" : "Royal Straight Flush";
        } else {
            if (suits == 1)
                return "Flush";
        }
        
        return null;
    }

    public int ScoreTable(string rank) {
        switch (rank) {
            case "Royal Straight Flush":
                return 7650000;
            case "Straight Flush":
                return 255000;
            case "Four Of A Kind":
                return 12500;
            case "Full House":
                return 4500;
            case "Flush":
                return 1349;
            case "Straight":
                return 727;
            case "Three Of A Kind":
                return 59;
            case "Two Pair":
                return 17;
            case "One Pair":
                return 2;
            default:
                return 0;
        }
    }

    public void CreateCards()
    {
        List<K_PlayingCard> playingCards = new List<K_PlayingCard>();
        foreach (Sprite x in Resources.LoadAll<Sprite>("Images/PlayingCards")){
            Transform tr = transform.FindChild(x.name);
            GameObject card = tr != null ? tr.gameObject : Instantiate(prefab) as GameObject;
            card.name = x.name;
            card.transform.parent = this.transform;
            card.transform.position = this.transform.position;
            card.GetComponentInChildren<SpriteRenderer>().sprite = x;
            card.transform.SetScale(Vector2.one);
            K_PlayingCard pc = card.GetComponent<K_PlayingCard>();
            pc.SetCard(x.name.First().ToString(), int.Parse(Regex.Match(x.name, @"\d{2}").Value));
            playingCards.Add(pc);
        };
        Cards = playingCards.ToArray();
        Size = Cards[0].GetComponentInChildren<SpriteRenderer>().bounds.size;
    }
}

