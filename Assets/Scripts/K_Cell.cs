using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_Cell : Singleton<K_Cell>
{
    class Cell
    {
        public K_PlayingCard card;
        public Vector2 coordination;
        public Vector2 position;
    }
    Cell[] cells;

    public Vector2 Unit { private set; get; }

    public float Scale { private set; get; }

    public K_PlayingCard[] Cards { get { return cells.Select(cell => cell.card).Where(card => card != null).ToArray(); } }
    
    public int BlankCell { get { return cells.Count(cell => cell.card == null); } }

    public Vector2 Position(K_PlayingCard card)
    {
        return Array.Find(cells, cell => cell.card.Equals(card)).position;
    }
    
    public void Init(int column, int row, Vector2 size, float separate)
    {
        float[] px = new float[column];
        float[] py = new float[row];

        Unit = new Vector2((this.transform.localScale.x - (column - 1) * separate) / column,
                           (this.transform.localScale.y - (row - 1) * separate) / row);
        Scale = Mathf.Min(Unit.x / size.x, Unit.y / size.y);

        for (int i = 0; i < px.Length; i++)
        {
            px [i] = (2 * i + 1) * Unit.x / 2 + separate * i - this.transform.localScale.x / 2 - Math.Abs(this.transform.localScale.x);
        }
        
        for (int i = 0; i < py.Length; i++)
        {
            py [i] = ((2 * i + 1) * Unit.y / 2 + separate * i) * -1 + this.transform.localScale.y / 2 - Math.Abs(this.transform.localScale.y);
        }

        cells = new Cell[py.Length * px.Length];

        for (int i = 0; i < cells.Length; i++)
        {
            cells [i] = new Cell();
            cells [i].card = null;
            cells [i].coordination = new Vector2(i % px.Length, i / px.Length);
            cells [i].position = new Vector2(px [(int)cells [i].coordination.x], py [(int)cells [i].coordination.y]);
        }

        cellInX = K_GameOptions.Instance.screenSize.x / 2 + size.x * Scale;
    }

    public void Pack()
    {
        Queue<K_PlayingCard> cardsInCell = new Queue<K_PlayingCard>(this.Cards);
        Array.ForEach(cells, cell => cell.card = cardsInCell.Count > 0 ? cardsInCell.Dequeue() : null);
    }

    public void DrawToCell(IList<K_PlayingCard> cards)
    {
        Queue<K_PlayingCard> cardsInCell = new Queue<K_PlayingCard>(cards);
        Array.ForEach(cells, cell => cell.card = cell.card ?? cardsInCell.Dequeue());
        CellIn();
    }

    public void Clear(IList<K_PlayingCard> cards)
    {
        Array.ForEach(cards.ToArray(), card => Array.Find(cells, cell => card.Equals(cell.card)).card = null);
    }

    public void ClearAll()
    {
        Array.ForEach(cells, cell => cell.card = null);
    }

    public Vector2 LastCellPosition { get { return cells.Last().position; } }

    public bool IsNeighbor(IList<K_PlayingCard> cards)
    {
        if (cards.Count < 2)
            return false;

        Vector2[] cp = cards.Select(card => Array.Find(cells, cell => card.Equals(cell.card)).coordination).ToArray();

        // 
        if (Mathf.Abs(cp [cp.Length - 2].x - cp [cp.Length - 1].x) > 1 || Mathf.Abs(cp [cp.Length - 2].y - cp [cp.Length - 1].y) > 1)
            return false;

        int linex = cp.GroupBy(z => z.x).Count();
        int liney = cp.GroupBy(z => z.y).Count();

        // On the Same Line or Diagonal
        if (linex == 1 || liney == 1 || (linex == cp.Length && liney == cp.Length))
            return true;

        return false;
    }

    float cellInX;

    void CellIn()
    {
        foreach (K_PlayingCard card in Cards.Reverse())
        {
            Cell cell = Array.Find(cells, c => card.Equals(c.card));
            if (cell.position == card.transform.GetXY())
                continue;

            TweenPosition tp;
            float d = (K_GameOptions.Instance.screenSize.x / 2 + card.transform.localScale.x) / 10 - 0.1f;

            if (card.transform.localScale.y != cell.position.y)
            {
                tp = TweenPosition.Begin(card.gameObject, d, new Vector3(-cellInX, card.transform.localScale.y));
                tp.method = UITweener.Method.EaseIn;
                tp.onFinished.Add(new EventDelegate(() => {
                    K_PlayingCard c = UITweener.current.GetComponentInChildren<K_PlayingCard>();
                    Vector2 p = this.Position(c);
                    c.transform.SetXY(new Vector2(cellInX, p.y));
                    tp = TweenPosition.Begin(c.gameObject, (K_GameOptions.Instance.screenSize.x / 2 - p.x) / 10 - 0.1f, p);
                    tp.method = UITweener.Method.EaseOut;
                    tp.PlayForward();
                }));
            } else
            {
                tp = TweenPosition.Begin(card.gameObject, d, cell.position);
                tp.method = UITweener.Method.EaseInOut;
            }
            tp.delay = K_Time.Instance.NextDelayTime(0.05f);
            tp.PlayForward();
        }
    }
}
