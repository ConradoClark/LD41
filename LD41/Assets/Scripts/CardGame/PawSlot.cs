using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawSlot : MonoBehaviour {

    public int Order;
    public Transform Transform;
    public bool Occupied { get; private set; }
    private Deck _deck;
    public bool Unlocked;
    public Card CurrentCard;
    public DiscardButton DiscardButton;

    // Use this for initialization
    void Start() {
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            cardUI.AddPawSlot(this);
        }

        Toolbox.Instance.OnPostInit += Instance_OnPostInit;
        DiscardButton.OnButtonPressed += DiscardButton_OnButtonPressed;
        DiscardButton.SlotOpen = Unlocked;

    }

    private void DiscardButton_OnButtonPressed(object sender, System.EventArgs e)
    {
        if (CurrentCard == null) return;
        Toolbox.Instance.DiscardCounter.Decrease();
        Discard(this.CurrentCard);
    }

    private void Instance_OnPostInit(object sender, System.EventArgs e)
    {
        Toolbox.TryGetDeck(out _deck);
    }

    // Update is called once per frame
    void Update() {
        DiscardButton.SlotOpen = Unlocked && Occupied;
    }

    public bool DrawCard(Card card)
    {
        if (Occupied || !Unlocked) return false;

        Occupied = true;
        card.Used = false;
        card.Instance.SetActive(true);
        card.Instance.transform.position = Transform.position;
        card.OnUsing += Card_OnUsing;
        CurrentCard = card;
        return true;
    }

    private void Card_OnUsing(object sender, Card.CardEvent e)
    {
        Discard(e.Card);
    }

    private void Discard(Card card)
    {
        CurrentCard = null;
        Occupied = false;
        _deck.AddToDiscardPile(card);
        card.OnUsing -= Card_OnUsing;
        // Start a coroutine for animations and stuff
        Toolbox.Instance.Pool.Release(card.PoolInstance, card.Instance);
        StartCoroutine(_deck.ReorganizePaw());
    }

    public void SendToDeck(Card card, bool reorganize = true)
    {
        CurrentCard = null;
        Occupied = false;
        _deck.AddToDrawPile(card);
        card.OnUsing -= Card_OnUsing;
        // Start a coroutine for animations and stuff
        Toolbox.Instance.Pool.Release(card.PoolInstance, card.Instance);
        if (reorganize)
        {
            StartCoroutine(_deck.ReorganizePaw());
        }
    }

    public static void Migrate(PawSlot source, PawSlot dest)
    {
        if (source.CurrentCard == null)
        {
            return;
        }
        source.CurrentCard.OnUsing -= source.Card_OnUsing;        
        source.Occupied = false;
        dest.Occupied = true;
        dest.CurrentCard = source.CurrentCard;
        dest.CurrentCard.OnUsing += dest.Card_OnUsing;
        dest.CurrentCard.Instance.transform.position = dest.Transform.position;
        source.CurrentCard = null;
    }
}
