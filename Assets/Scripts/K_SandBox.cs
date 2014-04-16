using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Extensions;

public class K_SandBox : MonoBehaviour
{
    K_ReadyToWork mp;
    // Use this for initialization
    void Start()
    {
    }

    
    void OnGUI(){
        if (!this.enabled)
            return;
        
//        GUI.skin = this.skin;
//        
//        GUILayout.BeginArea(new Rect(Screen.width / 2 - 200f, 50f, 150, 50));
//        GUILayout.Label(time.ToString("###"), style);
//        GUILayout.EndArea();
        
        //        public GUIContent buttonText = new GUIContent("some button");
        //        public GUIStyle buttonStyle = GUIStyle.none;
        //        void OnGUI() {
        //            Rect rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        //            if (rt.Contains(Event.current.mousePosition))
        //                GUI.Label(new Rect(0, 20, 200, 70), "PosX: " + rt.x + "\nPosY: " + rt.y + "\nWidth: " + rt.width + "\nHeight: " + rt.height);
        //            
        //            GUI.Button(rt, buttonText, buttonStyle);
        //        }
        //        
    }
}
