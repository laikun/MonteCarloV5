using UnityEngine;
using System.Collections;
using Extensions;

public class K_SandBox : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        K_PresentAction pa = K_PresentAction.Ready(this.gameObject);

        K_Present p = K_Present.Position(this.transform.position, new Vector3(10,0,0));
        p.Delay = 2f;

        pa.Next(p);
        pa.Next(new Vector3(0,10,0));
    }
	
}

