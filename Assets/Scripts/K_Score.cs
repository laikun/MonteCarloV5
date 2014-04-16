using UnityEngine;
using System.Collections;

public class K_Score : MonoBehaviour {

    public UILabel label;
    public long Score {private set; get;}

    void refresh(){
        label.text = Score.ToString("00000000");
    }

    public void Add(int score) {
        this.Score += score;
        this.refresh();
    }

    public void Reset(){
        this.Score = 0;
        this.refresh();
    }
}
