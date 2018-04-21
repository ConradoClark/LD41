using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Toolbox : Singleton<Toolbox>
{
    //public FrostyTime time;
    //public FrostyPoolManager pool;
    //public FrostyRandom random;
    public CardUI CardUI { get; set; }
    public InteractiveHelp InteractiveHelp { get; set; }
    public static readonly int PawNumberOfSlots = 7;
    protected Toolbox() { } // guarantee this will be always a singleton only - can't use the constructor!

    void Awake()
    {
        CardUI = RegisterComponent<CardUI>();
        StartCoroutine(PostInit());
        //InteractiveHelp = RegisterComponent<InteractiveHelp>();
        //time = RegisterComponent<FrostyTime>();
        //pool = RegisterComponent<FrostyPoolManager>();
        //random = RegisterComponent<FrostyRandom>();
    }

    IEnumerator PostInit()
    {
        yield return new WaitForEndOfFrame();
        InteractiveHelp = RegisterComponent<InteractiveHelp>();
        yield break;
    }

    // (optional) allow runtime registration of global objects
    static public T RegisterComponent<T>() where T : Component
    {
        return Instance.GetOrAddComponent<T>();
    }

    public static bool TryGetCardUI(out CardUI cardUI)
    {
        cardUI = Instance.CardUI;
        bool cardUINotNull = new Validation(cardUI != null, "InteractiveHelp init failed: CardUI is null.");
        return cardUINotNull;
    }
}
