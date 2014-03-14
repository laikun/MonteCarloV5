using UnityEngine;
using System.Collections;

public class K_Time : Singleton<K_Time>
{
    static float playTime;
    
    public static bool Pause {private set ; get; }

    public static float DeltaTime {private set ; get; }

    public static float Timeline { private set; get; }
    
    public float NextDelayTime(float delay)
    {
        Timeline = (Timeline < playTime ? playTime : Timeline) + delay;
        return Timeline - playTime;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!Pause){
            playTime += Time.deltaTime;
            DeltaTime = Time.deltaTime;
        } else {
            DeltaTime = 0f;
        }
    }
}

