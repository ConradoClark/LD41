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

    // Use this for initialization
    void Start() {
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            cardUI.AddPawSlot(this);
        }

        Toolbox.Instance.OnPostInit += Instance_OnPostInit;
    }

    private void Instance_OnPostInit(object sender, System.EventArgs e)
    {
        Toolbox.TryGetDeck(out _deck);
    }

    // Update is called once per frame
    void Update() {

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
        CurrentCard = null;
        Occupied = false;
        _deck.AddToDiscardPile(e.Card);
        // Start a coroutine for animations and stuff
        Toolbox.Instance.Pool.Release(e.Card.PoolInstance, e.Card.Instance);
        StartCoroutine(_deck.ReorganizePaw());
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
