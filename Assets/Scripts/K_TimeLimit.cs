using UnityEngine;
using System.Collections;

public class K_TimeLimit : MonoBehaviour
{
    float time;
    UILabel label;

    IEnumerator countDown(){
        while(time > 0) {
            yield return new WaitForFixedUpdate();
            time -= Time.fixedDeltaTime;
            label.text = time.ToString("###");
        }
    }

    void Init() {
        time = K_GameOptions.Instance.GetOptValue("TimeLimit");
        enabled = time != 0;
        label.text = enabled ? time.ToString("###") : "";
    }

    void Start() {
        this.enabled = false;
        label = GetComponent<UILabel>();
        label.text = "";

        K_Flag.Set("InnerPause", f => {
            if (f == 0 && time != 0)
                StartCoroutine("countDown");
            else if (f == 1 || time == 0)
                StopCoroutine("countDown");
        });

        K_Flag.Set("Ready", f => { if (f == 1) this.Init();});
    }
}

