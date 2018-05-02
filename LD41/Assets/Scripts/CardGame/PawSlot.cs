using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawSlot : MonoBehaviour
{

    public int Order;
    public Transform Transform;
    public bool Occupied { get; private set; }
    private Deck _deck;
    public bool Unlocked;
    public Card CurrentCard;
    public DiscardButton DiscardButton;
    private MainCharacter _mainCharacter;
    private bool _discarding;

    // Use this for initialization
    void Start()
    {
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            cardUI.AddPawSlot(this);
        }

        if (Toolbox.TryGetMainCharacter(out _mainCharacter))
        {
            Unlocked = _mainCharacter.Stats.PawSize >= Order;
        }

        Toolbox.Instance.OnPostInit += Instance_OnPostInit;
        DiscardButton.OnButtonPressed += DiscardButton_OnButtonPressed;
        DiscardButton.SlotOpen = Unlocked;

    }

    private void DiscardButton_OnButtonPressed(object sender, System.EventArgs e)
    {
        if (CurrentCard == null) return;
        Toolbox.Instance.DiscardCounter.Decrease();
        StartCoroutine(Discard(CurrentCard));
    }

    private void Instance_OnPostInit(object sender, System.EventArgs e)
    {
        Toolbox.TryGetDeck(out _deck);
    }

    // Update is called once per frame
    void Update()
    {
        DiscardButton.SlotOpen = Unlocked && Occupied && CurrentCard != null && !CurrentCard.Used;
    }

    public bool DrawCard(Card card)
    {
        if (card.Used || Occupied || !Unlocked || _discarding) return false;

        Occupied = true;
        card.Reset();
        card.Used = false;
        card.Instance.SetActive(true);
        card.Instance.transform.position = _deck.DeckSprite.transform.position;
        card.OnUsing += Card_OnUsing;
        CurrentCard = card;
        StartCoroutine(DrawCardAnimation(card));
        return true;
    }

    IEnumerator DrawCardAnimation(Card card)
    {
        card.Destination = Transform.position;
        StartCoroutine(card.FadeIn());
        yield return card.MoveToDestination(speed: 15f, onDestination: () => StartCoroutine(card.Flash(speed: 2f)));
    }


    private void Card_OnUsing(object sender, Card.CardEvent e)
    {
        StartCoroutine(Discard(e.Card));
    }

    private IEnumerator Discard(Card card, bool reorganize = true)
    {
        _discarding = true;
        while (!card.FinishedUsing)
        {
            yield return false;
        }

        if (!_discarding) yield break;

        CurrentCard = null;
        Occupied = card.Used = false;

        Toolbox.Instance.Pool.Release(card.PoolInstance, card.Instance);

        card.Reset();
        _deck.AddToDiscardPile(card);       
        _discarding = false;
        if (reorganize)
        {
            yield return StartCoroutine(_deck.ReorganizePaw());
        }
    }

    public void SendToDeck(Card card, bool reorganize = true)
    {
        Toolbox.Instance.StartCoroutine(Discard(card, reorganize));
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
        dest.CurrentCard.Destination = dest.Transform.position;
        dest.CurrentCard.StartCoroutine(dest.CurrentCard.MoveToDestination(damp: false));            
        source.CurrentCard = null;
        if (source._discarding)
        {
            source._discarding = false;
            Toolbox.Instance.StartCoroutine(dest.Discard(dest.CurrentCard));
        }
    }
}
