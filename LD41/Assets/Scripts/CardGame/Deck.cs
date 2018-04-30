using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private PawSlot[] _slots;
    private CharacterStats _stats;
    private CardUI _cardUI;
    private Stack<Card> _discardPile = new Stack<Card>();
    private Stack<Card> _cardPile = new Stack<Card>();
    public bool Drawing { get; private set; }
    public bool Reorganizing { get; private set; }
    private float opSpeed = 0.1f;
    public SpriteRenderer DeckSprite;

    // Use this for initialization
    void Start()
    {
        if (Toolbox.TryGetCardUI(out _cardUI))
        {
            _slots = _cardUI.GetPawSlots();
            UpdateUI();
        }
        MainCharacter mainCharacter;
        if (Toolbox.TryGetMainCharacter(out mainCharacter))
        {
            _stats = mainCharacter.Stats;
        }

        // Generate deck, for now
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[0]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[0]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[1]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[1]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[2]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[2]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[3]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[3]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[4]);
        AddCard(Toolbox.Instance.DeckManager.CardDatabase[4]);

        for (int i = 0; i < 5; i++)
        {
            AddCard(Toolbox.Instance.DeckManager.CardDatabase[Random.Range(0, Toolbox.Instance.DeckManager.CardDatabase.Length)]);
        }
        Shuffle(_cardPile);
        UpdateUI();
    }

    void AddCard(DeckManager.CardAssociation ca)
    {
        var go = Toolbox.Instance.Pool.Retrieve(ca.PrefabPoolInstance);
        var card = go.GetComponent<Card>();
        if (card == null)
        {
            card = go.AddComponent<Card>();
        }
        card.Instance = go;
        card.PoolInstance = ca.PrefabPoolInstance;
        card.CardType = ca.CardType;
        card.Instance.SetActive(false);
        _cardPile.Push(card);
    }

    // Update is called once per frame
    void Update()
    {
        // No cards? Shouldn't happen
        if (_discardPile.Count == 0 && _cardPile.Count == 0) return;

        if (!Drawing && _slots.Length > 0 && _slots.Count(s => s.Occupied) <= _stats.Draw)
        {
            StartCoroutine(DrawCards());
        }
    }

    void UpdateUI()
    {
        _cardUI.CardCount.text = _cardPile.Count.ToString().PadLeft(2, '0');
        _cardUI.DiscardCount.text = _discardPile.Count.ToString().PadLeft(2, '0');
    }

    IEnumerator DrawCards()
    {
        Drawing = true;
        if (_cardPile.Count == 0)
        {
            yield return StartCoroutine(ShuffleDiscardPileIntoCardPile());
        }

        foreach (var slot in _slots)
        {
            if (_cardPile.Count == 0)
            {
                yield return StartCoroutine(ShuffleDiscardPileIntoCardPile());
            }
            if (slot.DrawCard(_cardPile.Peek()))
            {
                _cardPile.Pop();
            }
            UpdateUI();
            if (slot != _slots.Last())
            {
                yield return new WaitForSeconds(opSpeed);
            }
        }

        Drawing = false;
    }

    IEnumerator ShuffleDiscardPileIntoCardPile()
    {
        while (_discardPile.Count > 0)
        {
            Card card = _discardPile.Pop();
            card.Reset();
            _cardPile.Push(card);
            UpdateUI();
            if (_discardPile.Count > 0)
            {
                yield return new WaitForSeconds(opSpeed);
            }
        }
        Shuffle(_cardPile);
    }

    public IEnumerator ShufflePawIntoCardPile()
    {
        var list = _slots.Where(s => s.Occupied).ToList();
        foreach (var slot in list)
        {
            slot.SendToDeck(slot.CurrentCard, false);
            UpdateUI();

            if (slot != list.Last())
            {
                yield return new WaitForSeconds(opSpeed);
            }
        }
        Shuffle(_cardPile);
        StartCoroutine(ReorganizePaw());
    }

    public void AddToDiscardPile(Card card)
    {
        _discardPile.Push(card);
        UpdateUI();
    }

    public void AddToDrawPile(Card card)
    {
        _cardPile.Push(card);
        UpdateUI();
    }

    public IEnumerator ReorganizePaw()
    {
        Reorganizing = true;
        while (Drawing) yield return new WaitForEndOfFrame();
        var cardsInSlots = new Stack<PawSlot>(_slots.Where(s => s.Occupied).OrderByDescending(s=>s.Order).ToList());
        foreach(var slot in _slots.OrderBy(s => s.Order).ToList())
        {
            if (cardsInSlots.Count == 0) break;
            if (slot.Occupied || !slot.Unlocked)
            {
                var usedSlot = cardsInSlots.FirstOrDefault(c=>c.CurrentCard == slot.CurrentCard);
                if (usedSlot!=null)
                {
                    cardsInSlots = new Stack<PawSlot>(cardsInSlots.Except(new[] { usedSlot }).Reverse());
                }
                continue;
            }
            PawSlot ps = cardsInSlots.Pop();
            if (ps != slot)
            {
                PawSlot.Migrate(ps, slot);
                UpdateUI();
                if (slot != _slots.Last())
                {
                    yield return new WaitForSeconds(opSpeed);
                }
            }
        }
        Reorganizing = false;
    }

    void Shuffle<T>(Stack<T> stack)
    {
        var values = stack.ToArray();
        stack.Clear();
        foreach (var value in values.OrderBy(x => Random.Range(0f, 1f)))
            stack.Push(value);
    }
}
