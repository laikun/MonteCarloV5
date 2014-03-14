using UnityEngine;
using System.Collections;
using Extensions;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    /** Returns the instance of this singleton. */
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));
                
                if (instance == null)
                {
                    Debug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
                }
                if (instance == null)
                {
                    GameObject singleton = new GameObject();
                    instance = singleton.AddComponent<T>();
                    singleton.name = "(singleton) " + typeof(T).ToString();
                    
                    DontDestroyOnLoad(singleton);
                    
                    Debug.Log("[Singleton] An instance of " + typeof(T) + 
                        " is needed in the scene, so '" + singleton +
                        "' was created with DontDestroyOnLoad.");
                }

            }
            
            return instance;
        }
    }
}

