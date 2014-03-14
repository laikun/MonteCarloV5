﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class K_PlayingCard : MonoBehaviour
{
    public K_PlayingCard[] Cards{ private set; get; }

    public GameObject prefab;

    public string Suit   { private set; get; }

    public int Number{ private set; get; }

    public K_PlayingCard SetCard(string str, int num, GameObject go)
    {
        this.prefab = go;
        this.Suit = str;
        this.Number = num;
        return this;
    }

    public UIEventTrigger UI { get { return this.GetComponent<UIEventTrigger>(); } }

    public Vector2 Size{ private set; get;}

    public bool PairSuit(K_PlayingCard card)
    {
        return card.Suit.Equals(this.Suit);
    }

    public bool PairNumber(K_PlayingCard card)
    {
        return card.Number.Equals(this.Number);
    }

    public bool PairCard(K_PlayingCard card)
    {
        return !PairSuit(card) && PairNumber(card);
    }

    public string Rank(K_PlayingCard[] cards) {

        // Check Pair
        if (cards.Length < 3)
            return !cards[0].PairCard(cards[1]) ? null : "One Pair";

        // Count Number Of Cards
        int nums = cards.GroupBy(x => x.Number).Count();

        // Check Three of a Kind
        if (cards.Length < 4)
            return nums != 1 ? null : "Three Of A Kind";

        // Count Suits Of Cards
        int suits = cards.GroupBy(x => x.Suit).Count();

        // Check Two Pair
        if (nums == 2 && suits == 2)
            return cards.Length != 5 ? "Two Pair" : "Full House";

        // Check Four Of A Kind
        if (nums < 3 && suits == 4)
            return "Four Of A Kind";

        // 
        if (nums != cards.Length)
            return null;

        cards = cards.OrderBy(x => x.Number).ToArray();
        int[] dis = cards.Skip(1).Select((x, i) => Mathf.Abs(x.Number - cards[i-1].Number))
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

    public void CreateCards()
    {
        List<K_PlayingCard> playingCards = new List<K_PlayingCard>();
        foreach (Sprite x in Resources.LoadAll<Sprite>("Images/PlayingCards")){
            Transform tr = transform.FindChild(x.name);
            GameObject card = tr != null ? tr.gameObject : Instantiate(prefab) as GameObject;
            card.name = x.name;
            card.transform.parent = this.transform;
            card.transform.position = this.transform.position;
            card.gameObject.layer = this.gameObject.layer;
            card.GetComponentInChildren<SpriteRenderer>().sprite = x;
            card.transform.localScale = Vector2.one;
            playingCards.Add(card.GetComponent<K_PlayingCard>()
                             .SetCard(x.name.First() + "", int.Parse(Regex.Match(x.name, @"\d{2}").Value), prefab));
        };
        Cards = playingCards.ToArray();
        Size = Cards[0].GetComponentInChildren<SpriteRenderer>().bounds.size;
    }

}
