using UnityEngine;
using System.Collections;

public class K_Hint : MonoBehaviour {

    int hints;
    UILabel label;
    
    void Init() {
        hints = K_GameOptions.Instance.GetOptValue("Hint");

        if (hints == 0)
            return;
        
        this.enabled = true;
        label.text = new string('H', hints);
    }
    
    void Start() {
        this.enabled = false;
        label = GetComponent<UILabel>();
        label.text = "";

        // dummy
        K_Flag.Set("SetHint", f => {
            label.text = new string('H', f);
            hints = f;
        });

        K_Flag.Set("Ready", f => { if (f == 1) this.Init();});
    }

    void OnClick() {
        if (!enabled || hints < 1)
            return;

        K_Flag.On("GetHint", hints);
    }
}
