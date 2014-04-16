using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class K_Flag
{
    public delegate void FlagHandle (int flag);
    static Dictionary<string, FlagHandle> flag = new Dictionary<string, FlagHandle>();
    static Dictionary<string, int> state = new Dictionary<string, int>();

    public static int State(string name) {
        return state[name];
    }

    public static void On(string name, bool f) {
        On(name, f ? 1 : 0);
    }

    public static void On(string name, int f) {
        state[name] = f;
        flag[name](f);
    }
    
    public static void Set(string name, FlagHandle handle){
        if (flag.ContainsKey(name)){
            flag[name] -= handle;
            flag[name] += handle;
            state[name] = 0;
        } else {
            FlagHandle temp = x => {};
            temp += handle;
            flag.Add(name, temp);
            state.Add(name, 1);
        }
    }

    public static void Init(string name) {
        state[name] = 0;
        flag[name] = x => {};
    }

    public static void Init(){
        flag = new Dictionary<string, FlagHandle>();
        state = new Dictionary<string, int>();
    }
}

