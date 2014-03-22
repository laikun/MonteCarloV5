using UnityEngine;
using System.Collections;
using Extensions;

public class K_SandBox : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
//        K_ICatridge cat = K_ProgressSet.ReadyToScale(gameObject);
//
//        K_TransTween<Vector3> tt = new K_TransTween<Vector3>(transform.localScale, new Vector3(10f, 10f, 10f), new K_TimeCurve());
//        cat.AddLast(tt);
//        tt.RepeatRewind = true;
//
//        cat.Play();

        K_TransTween<Color> tt;
        K_ICatridge cat = K_ProgressSet.ReadyToColor(gameObject, out tt);
        cat.Add(tt = tt.Next(Color.red, 1f));
        cat.Play();
        cat.Add(tt.Next(Color.blue, 1f));
        cat.Loop = true;
    }
}
