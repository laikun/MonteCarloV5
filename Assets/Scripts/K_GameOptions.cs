using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_GameOptions : Singleton<K_GameOptions>
{
	private Dictionary<string, K_OptionSetting> options = new Dictionary<string, K_OptionSetting>();
	public GameObject optPanel;
    public GameObject optVslider;
    public GameObject optCheckbox;
	public GameObject Deck;
	public float separate;
    public Vector2 screenSize;

	public int GetOptValue(string OptName)
    {
        return options [OptName].OptValue;
    }

	private Dictionary<string, object> backOptions = new Dictionary<string, object>();
	public void SetBackOptValue (string name, object value){
		backOptions.Add(name, value);
	}
	public object GetBackOptValue (string name){
		return backOptions[name];
	}

    void Awake()
    {
        screenSize = Vector2.zero;
        screenSize.y = 2 * Camera.main.orthographicSize;// * 100f;/*pixels to unit*/;
        screenSize.x = screenSize.y * Camera.main.aspect;

		backOptions.Add("Separate", separate);
		backOptions.Add("CardSize", Deck.GetComponentInChildren<SpriteRenderer>().bounds.size);

		Func<GameObject, string, int[], string, GameObject> newOpt = (opt, name, minMax, unit) => {
            GameObject go = Instantiate(opt) as GameObject;
            go.name = name;
            go.transform.parent = optPanel.transform;
            go.transform.localScale = Vector3.one;
            options.Add(name, go.GetComponent<K_OptionSetting>().SetOption(opt, name, minMax, unit));
            go.layer = optPanel.layer;

            return go;
        };

//        newOpt(optVslider, "Hint", new int[]{0, 5}, "H");
//        newOpt(optVslider, "TurnLimit", new int[]{5, 30}, "s");
//        newOpt(optVslider, "TimeLimit", new int[]{60, 300}, "s");
//        newOpt(optVslider, "Coin", new int[]{1, 10}, "$");

        newOpt(optCheckbox, "AutoDraw", null, null);
        newOpt(optCheckbox, "Column", new int[]{4, 5}, null);
        newOpt(optCheckbox, "Row", new int[]{4, 5}, null);

        optPanel.transform.parent.GetComponent<UIScrollView>().ResetPosition();
    }
}
