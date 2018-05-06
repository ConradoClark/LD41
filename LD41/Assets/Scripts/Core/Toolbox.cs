using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Toolbox : Singleton<Toolbox>
{
    public PoolManager Pool { get; set; }
    public CardUI CardUI { get; set; }
    public InteractiveHelp InteractiveHelp { get; set; }
    public MainCharacter MainCharacter { get; set; }
    public Deck Deck { get; set; }
    public DiscardCounter DiscardCounter { get; set; }
    public DeckManager DeckManager { get; set; }
    public LevelGrid LevelGrid { get; set; }
    public Makina MainMakina { get; set; }
    public event EventHandler<EventArgs> OnPostInit;

    protected Toolbox() { } // guarantee this will be always a singleton only - can't use the constructor!

    void Awake()
    {
        CardUI = RegisterComponent<CardUI>();
        MainCharacter = RegisterComponent<MainCharacter>();
        Pool = RegisterComponent<PoolManager>();
        DeckManager = RegisterComponent<DeckManager>();
        LevelGrid = RegisterComponent<LevelGrid>();
        MainMakina = RegisterComponent<Makina>();
        StartCoroutine(PostInit());
    }

    IEnumerator PostInit()
    {
        yield return new WaitForEndOfFrame();
        InteractiveHelp = RegisterComponent<InteractiveHelp>();
        Deck = RegisterComponent<Deck>();
        if (OnPostInit != null)
        {
            OnPostInit.Invoke(this, new EventArgs());
        }
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
        bool cardUINotNull = new Validation(cardUI != null, "Init failed: CardUI is null.");
        return cardUINotNull;
    }

    public static bool TryGetMainCharacter(out MainCharacter character)
    {
        character = Instance.MainCharacter;
        bool characterNotNull = new Validation(character != null, "Init failed: Main Character is null.");
        return characterNotNull;
    }

    public static bool TryGetDeck(out Deck deck)
    {
        deck = Instance.Deck;
        bool deckNotNull = new Validation(deck != null, "Init failed: Deck is null.");
        return deckNotNull;
    }

    public static bool TryGetLevelGrid(out LevelGrid grid)
    {
        grid = Instance.LevelGrid;
        bool gridNotNull = new Validation(grid != null, "Init failed: Level Grid is null.");
        return gridNotNull;
    }
}
