using UnityEngine;
using System;
using System.Collections;

public class K_Counter : MonoBehaviour {

    public UILabel one;
    public UILabel ten;
    public UILabel cen;

    public delegate void CountOver();
    public CountOver countOver;
    
    int count;
    int start;
    int countStart;
    int over;
    int time;

    void refresh(){
        this.cen.gameObject.SetActive(count > 99 ? true : false);
        this.ten.gameObject.SetActive(count > 9 ? true : false);
        
        string ct = count.ToString("000");

        this.cen.text = ct.Substring(0, 1);
        this.ten.text = ct.Substring(1, 1);
        this.one.text = ct.Substring(2, 1);

        if (count == over && countOver != null){
            countOver();
            Debug.Log(this.name +" CountOver");
            this.Stop();
            return;
        }       
    }

    IEnumerator counting(){
        while(count != over){
            count += time;
            if (count <= countStart)
                refresh();
            yield return new WaitForSeconds(1);
        }
    }

    public void Init(int start){
        this.Init(start, start, 0, null);
    }    

    public void Init(int start, CountOver countOver){
        this.Init(start, start, 0, countOver);
    }

    public void Init(int start, int countStart, int over, CountOver countOver){
        this.start = start;
        this.count = start;
        this.countStart = countStart;
        this.over = over;

        this.countOver += start != over ? countOver : null;

        this.gameObject.SetActive(start != over ? true : false);

        this.refresh();
    }

    public void Pause(bool b){
        if (b) {
            StartCoroutine("counting");
        } else {
            StopCoroutine("counting");
        }
    }

    public void Stop(){
        StopCoroutine("counting");
    }

    public void Reset(){
        this.Stop();
        this.Init(start, countStart, over, countOver);
    }

    public void Down(){
        this.time = -1;
        if (!this.gameObject.activeSelf)
            return;
        StartCoroutine("counting");
        Debug.Log("CountDown Start : " + start);
    }

    public void Down(int time){
        this.count += time;
        this.refresh();
    }

}
