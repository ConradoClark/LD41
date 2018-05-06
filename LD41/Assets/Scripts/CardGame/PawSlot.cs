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
        Makinery discard = new Makinery(250) { QueueName = "DeckOp" };
        discard.AddRoutine(() => CurrentCard.Discard());
        Toolbox.Instance.MainMakina.AddMakinery(discard);
        _discarding = true;
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
        if (card.Used || Occupied || !Unlocked) return false;

        Occupied = true;
        card.Reset();
        card.Used = false;
        card.Instance.SetActive(true);
        card.Instance.transform.position = _deck.DeckSprite.transform.position;
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

    public Makinery SendToDeck(Card card)
    {
        Makinery discard = new Makinery(250);
        discard.AddRoutine(() => card.Discard(false));
        _discarding = true;
        return discard;
    }

    public void ReleaseCard()
    {
        Occupied = false;
        CurrentCard = null;
    }

    public static void Migrate(PawSlot source, PawSlot dest)
    {
        if (source.CurrentCard == null)
        {
            return;
        }
        source.Occupied = false;
        dest.Occupied = true;
        dest.CurrentCard = source.CurrentCard;
        dest.CurrentCard.Destination = dest.Transform.position;
        dest.CurrentCard.StartCoroutine(dest.CurrentCard.MoveToDestination(damp: false));            
        source.CurrentCard = null;
    }
}
