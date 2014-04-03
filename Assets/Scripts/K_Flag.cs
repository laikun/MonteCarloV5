using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class K_Flag
{
    public delegate void FlagHandle (bool flag);
    static Dictionary<string, FlagHandle> flag = new Dictionary<string, FlagHandle>();
    static Dictionary<string, bool> state = new Dictionary<string, bool>();

    public static void OnFlag(string name, bool f) {
        state[name] = f;
        flag[name](f);
    }

    public static void ConnectFlag(string name, FlagHandle handle){
        if (flag.ContainsKey(name)){
            flag[name] += handle;
            state[name] = false;
        } else {
            FlagHandle temp = x => {};
            temp += handle;
            flag.Add(name, temp);
            state.Add(name, false);
        }
    }
}

