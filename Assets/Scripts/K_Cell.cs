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

        cellInX = K_GameOptions.Instance.screenSize.x / 2;
    }

    public IEnumerator DrawToCell(K_PlayingCard[] cards) {
        Queue<K_PlayingCard> cardsInCell = new Queue<K_PlayingCard>(Cards);
        Array.ForEach(cards, card => cardsInCell.Enqueue(card));
        Array.ForEach(cells, cell => cell.card = cardsInCell.Dequeue());
        yield return StartCoroutine(CellIn());
    }

    public void Clear(IList<K_PlayingCard> cards) {
        Array.ForEach(cards.ToArray(), card => Array.Find(cells, cell => card.Equals(cell.card)).card = null);
    }

    public void ClearAll() {
        Array.ForEach(cells, cell => cell.card = null);
    }

    public bool IsNeighbor(K_PlayingCard[] cards) {
        if (cards.Length < 2)
            return false;

        Vector2[] cp = cards.Where(x => x != null).Select(x => Array.Find(cells, cell => x.Equals(cell.card)).coordination).ToArray();

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

    public K_PlayingCard[] InLinearCards(K_PlayingCard card) {
        return this.InLinearCards(new K_PlayingCard[]{card});
    }
    public K_PlayingCard[] InLinearCards(K_PlayingCard[] card) {
        if (card.Length == 0)
            return null;

        Cell[] ucells = cells.Where(x => !card.Contains(x.card)).ToArray();
        if (card.Length == 1){
            Vector2 cd = Array.Find(cells, x => card[0].Equals(x.card)).coordination;
            return ucells.Where(x => x.card != null && Mathf.Abs(x.coordination.x - cd.x) <= 1 && Mathf.Abs(x.coordination.x - cd.x) <= 1).Select(x => x.card).ToArray();
        }

        // Two-point form
        Vector2[] ab = cells.Where(x => card.First().Equals(x.card) || card.Last().Equals(x.card)).Select(x => x.coordination).ToArray();
        Func<Vector2, bool> tpf = x => (ab[1].x - ab[0].x) * (x.y - ab[0].y) - (ab[1].y - ab[0].y) * (x.x - ab[0].x) == 0;
        return ucells.Where(x => tpf(x.coordination)).Select(x => x.card).ToArray();
    }
    
    IEnumerator CellIn() {
        K_PlayingCard[] reverseCards = Cards.Reverse().ToArray();

        foreach (K_PlayingCard card in reverseCards) {

            Cell cell = Array.Find(cells, c => card.Equals(c.card));
            if (cell.position == card.transform.GetXY())
                continue;

            card.RTW.MoreWork(x => K_OnStage.In(x));
            card.RTW.Delay(0.05f * cell.position.y);
            if (card.transform.position.y != cell.position.y) {
                card.RTW.LerpPosition(new Vector3(-cellInX, card.transform.position.y, card.transform.position.z), 
                                           K_TimeCurve.EaseIn(0.1f * Vector2.Distance(new Vector2(-cellInX, cell.position.y), card.transform.position)));
                card.RTW.LerpPosition(new Vector3(cellInX, cell.position.y, card.transform.position.z),
                                           cell.position.V3(card),
                                           K_TimeCurve.EaseOut(0.1f * Vector2.Distance(new Vector2(cellInX, cell.position.y), cell.position)));
            } else {
                float d = Vector2.Distance(cell.position, card.transform.position);
                card.RTW.LerpPosition(cell.position.V3(card), K_TimeCurve.EaseInOut(0.1f * d));
            }
            card.RTW.GoWork();
        }

        yield return StartCoroutine(this.WaitWork(() => !reverseCards.Last().RTW.IsWorkDone));
    }
}
