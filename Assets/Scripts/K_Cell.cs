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

    public Vector2 LastCellPosition { get { return cells.Last().position; } }
    
    float cellInX;
    
    public Vector2 Position(K_PlayingCard card) {
        return Array.Find(cells, cell => cell.card.Equals(card)).position;
    }
    
    public void Init(int column, int row, Vector2 size, float separate) {
        float[] px = new float[column];
        float[] py = new float[row];

        Unit = new Vector2((this.transform.localScale.x - (column - 1) * separate) / column,
                           (this.transform.localScale.y - (row - 1) * separate) / row);
        Scale = Mathf.Min(Unit.x / size.x, Unit.y / size.y);

        Vector3 zp = new Vector3(this.transform.position.x - this.transform.localScale.x / 2,
                                 this.transform.localScale.y / 2 + this.transform.position.y);

        for (int i = 0; i < px.Length; i++) {
            px [i] = (2 * i + 1) * Unit.x / 2 + separate * i + zp.x;
        }
        for (int i = 0; i < py.Length; i++) {
            py [i] = ((2 * i + 1) * Unit.y / 2 + separate * i) * -1 + zp.y;
        }

        cells = new Cell[py.Length * px.Length];

        for (int i = 0; i < cells.Length; i++) {
            cells [i] = new Cell();
            cells [i].card = null;
            cells [i].coordination = new Vector2(i % px.Length, i / px.Length);
            cells [i].position = new Vector2(px [(int)cells [i].coordination.x], py [(int)cells [i].coordination.y]);
        }

        cellInX = K_GameOptions.Instance.screenSize.x / 2 + size.x * Scale;
    }

    public void DrawToCell(K_PlayingCard[] cards) {
        Queue<K_PlayingCard> cardsInCell = new Queue<K_PlayingCard>(Cards);
        Array.ForEach(cards, card => cardsInCell.Enqueue(card));
        Array.ForEach(cells, cell => cell.card = cardsInCell.Dequeue());
        CellIn();
    }

    public void Clear(IList<K_PlayingCard> cards) {
        Array.ForEach(cards.ToArray(), card => Array.Find(cells, cell => card.Equals(cell.card)).card = null);
    }

    public void ClearAll() {
        Array.ForEach(cells, cell => cell.card = null);
    }

    public bool IsNeighbor(IList<K_PlayingCard> cards) {
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

    void CellIn() {
        int idx = 0;
        foreach (K_PlayingCard card in Cards.Reverse()) {
            Cell cell = Array.Find(cells, c => card.Equals(c.card));
            if (cell.position == card.transform.GetXY())
                continue;

            card.RTW.Delay(0.05f * idx++);
            card.RTW.AddWork(x => K_OnStage.In(x));
            if (card.transform.position.y != cell.position.y) {
                card.RTW.LerfPosition(new Vector3(-cellInX, card.transform.position.y, card.transform.position.z), 
                                           K_TimeCurve.EaseIn(0.1f * Vector2.Distance(new Vector2(-cellInX, cell.position.y), card.transform.position)));
                card.RTW.LerfPosition(new Vector3(cellInX, cell.position.y, card.transform.position.z),
                                           cell.position.V3(card),
                                           K_TimeCurve.EaseOut(0.1f * Vector2.Distance(new Vector2(cellInX, cell.position.y), cell.position)));
            } else {
                float d = Vector2.Distance(cell.position, card.transform.position);
                card.RTW.LerfPosition(cell.position.V3(card), K_TimeCurve.EaseInOut(0.1f * d));
            }
            card.RTW.Play();
        }
    }
}
