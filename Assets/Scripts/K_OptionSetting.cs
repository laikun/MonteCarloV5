using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class K_OptionSetting : MonoBehaviour
{

    public string OptName{ private set; get; }

    public int OptValue { private set; get; }

    private int min;
    private int max;
    private string unit;
    private UILabel[] labelValue;
    private static int chkgrp = 1;
    private delegate void Mola();

    private Mola mola;

    public K_OptionSetting SetOption(GameObject go, String str, int[] minMax, string unit)
    {
        Array.Find(GetComponentsInChildren<UILabel>(), x => x.name.Equals("Name")).text = str;
        this.OptName = str;
        this.unit = unit;
        this.min = minMax != null ? minMax.First() : 0;
        this.max = minMax != null ? minMax.Last() : 1;
        if (go.name.Contains("Checkbox"))
        {

            this.OptValue = this.min;

            labelValue [0].text = this.min.ToString();   
            labelValue [1].text = this.max.ToString();

            if (minMax == null)
            {
                Destroy(labelValue [1].transform.parent.gameObject);
                labelValue [0].text = "On";
                labelValue [0].transform.parent.GetComponent<UIToggle>().startsActive = true;
                labelValue [0].transform.parent.GetComponent<UIToggle>().group = 0;
                labelValue = new UILabel[]{labelValue [0]};

                mola += () => {
                    if (UIToggle.current == null)
                        return;
                    this.OptValue = UIToggle.current.value ? 1 : 0;
                    labelValue [0].text = UIToggle.current.value ? "On" : "Off";
                };

            } else
            {
                chkgrp++;
                Array.ForEach(labelValue, x => x.transform.parent.GetComponent<UIToggle>().group = chkgrp);
                mola += () => {
                    if (UIToggle.current == null)
                        return;
                    if (UIToggle.current.value)
                    {
                        this.OptValue = int.Parse(UIToggle.current.GetComponentInChildren<UILabel>().text);
                    }
                };
            }

        } else
        {
            mola += () => {
                if (UIProgressBar.current != null)
                {
                    this.OptValue = Mathf.RoundToInt(UIProgressBar.current.value * (max - min));
                    labelValue [0].text = this.OptValue != 0 ? (this.OptValue += this.min) + unit : "NoLimit";
                }
            };
        }

        return this;
    }
    
    public void SetValue()
    {
        mola();
    }
    
    // Use this for initialization
    void Awake()
    {
        labelValue = Array.FindAll(this.GetComponentsInChildren<UILabel>(), x => x.name.Equals("Value"));
    }
    
    // Update is called once per frame
    void Update()
    {
    
    }


}
