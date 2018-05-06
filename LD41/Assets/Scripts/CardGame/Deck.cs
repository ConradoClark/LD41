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
    private Queue<Card> _useQueue = new Queue<Card>();

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

    public bool AddToUsageQueue(Card card, out int queuePosition)
    {
        int pos = -1;
        if (_useQueue.Count > 0)
        {
            pos = _useQueue.Last().QueuePosition;
        }
        _useQueue.Enqueue(card);
        queuePosition = pos + 1;

        return _useQueue.Count > 1;
    }

    public void DequeueUsage()
    {
        _useQueue.Dequeue();
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

        if (!Reorganizing && !Drawing && _slots.Length > 0 && _slots.Count(s => s.Occupied) <= _stats.Draw)
        {
            Drawing = true;
            Makinery draw = new Makinery(100) { QueueName = "DeckOp" };
            draw.AddRoutine(() => DrawCards());
            Toolbox.Instance.MainMakina.AddMakinery(draw);
        }
    }

    void UpdateUI()
    {
        _cardUI.CardCount.text = _cardPile.Count.ToString().PadLeft(2, '0');
        _cardUI.DiscardCount.text = _discardPile.Count.ToString().PadLeft(2, '0');
    }

    IEnumerator<MakineryGear> DrawCards()
    {
        Drawing = (_slots.Length > 0 && _slots.Count(s => s.Occupied) <= _stats.Draw);
        if (!Drawing) yield break; 
        
        if (_cardPile.Count == 0)
        {
            Makinery shuffleDiscard = new Makinery(100);
            shuffleDiscard.AddRoutine(() => ShuffleDiscardPileIntoCardPile());
            yield return new InnerMakinery(shuffleDiscard, Toolbox.Instance.MainMakina);
        }

        foreach (var slot in _slots)
        {
            if (_cardPile.Count == 0)
            {
                Makinery shuffleDiscard = new Makinery(100);
                shuffleDiscard.AddRoutine(() => ShuffleDiscardPileIntoCardPile());
                yield return new InnerMakinery(shuffleDiscard, Toolbox.Instance.MainMakina);
            }
            if (slot.Occupied) continue;
            if (slot.DrawCard(_cardPile.Peek()))
            {
                _cardPile.Pop();
            }
            UpdateUI();
            if (slot != _slots.Last())
            {
                yield return new WaitForSecondsGear(opSpeed);
            }
        }

        Drawing = false;
    }

    IEnumerator<MakineryGear> ShuffleDiscardPileIntoCardPile()
    {
        while (_discardPile.Count > 0)
        {
            Card card = _discardPile.Pop();
            _cardPile.Push(card);
            UpdateUI();
            if (_discardPile.Count > 0)
            {
                yield return new WaitForSecondsGear(opSpeed);
            }
        }
        Shuffle(_cardPile);
    }

    public PawSlot FindCardInPaw(Card card)
    {
        return _slots.FirstOrDefault(s => s.CurrentCard == card);
    }

    public IEnumerator<MakineryGear> ShufflePawIntoCardPile()
    {
        var list = _slots.Where(s => s.Occupied).ToList();

        while (list.Count > 0)
        {
            var slot = list[0];

            if (slot.CurrentCard == null || slot.CurrentCard.Used)
            {
                list.Remove(slot);
                continue;
            }

            yield return new InnerMakinery(slot.SendToDeck(slot.CurrentCard), Toolbox.Instance.MainMakina);
            UpdateUI();

            if (slot != list.Last())
            {
                yield return new WaitForSecondsGear(opSpeed);
            }
        }
        Shuffle(_cardPile);
        Makinery reorganize = new Makinery(100) { QueueName = "DeckOp" };
        reorganize.AddRoutine(() => ReorganizePaw());
        Toolbox.Instance.MainMakina.AddMakinery(reorganize);
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

    public IEnumerator<MakineryGear> ReorganizePaw()
    {
        var cardsInSlots = new Stack<PawSlot>(_slots.Where(s => s.Occupied).OrderByDescending(s => s.Order).ToList());
        foreach (var slot in _slots.OrderBy(s => s.Order).ToList())
        {
            if (cardsInSlots.Count == 0) break;
            if (slot.Occupied || !slot.Unlocked)
            {
                var usedSlot = cardsInSlots.FirstOrDefault(c => c.CurrentCard == slot.CurrentCard);
                if (usedSlot != null)
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
                    yield return new WaitForSecondsGear(opSpeed);
                }
            }
        }
    }

    void Shuffle<T>(Stack<T> stack)
    {
        var values = stack.ToArray();
        stack.Clear();
        foreach (var value in values.OrderBy(x => Random.Range(0f, 1f)))
            stack.Push(value);
    }
}
