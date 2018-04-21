using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Toolbox : Singleton<Toolbox>
{
    //public FrostyTime time;
    //public FrostyPoolManager pool;
    //public FrostyRandom random;

    protected Toolbox() { } // guarantee this will be always a singleton only - can't use the constructor!

    void Awake()
    {
        //time = RegisterComponent<FrostyTime>();
        //pool = RegisterComponent<FrostyPoolManager>();
        //random = RegisterComponent<FrostyRandom>();
    }

    // (optional) allow runtime registration of global objects
    static public T RegisterComponent<T>() where T : Component
    {
        return Instance.GetOrAddComponent<T>();
    }
}
