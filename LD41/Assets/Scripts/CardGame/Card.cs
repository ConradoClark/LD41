using Assets.Scripts.CardGame.CardLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mk = CardGameMakineryConstants;

public class Card : MonoBehaviour
{
    public class CardEvent : EventArgs
    {
        public Card Card { get; set; }
    }

    public Vector2 Destination;
    public CardEnum CardType;
    public PoolInstance PoolInstance;
    public GameObject Instance;
    public bool Used = false;
    private SpriteRenderer _spriteRenderer;
    public int QueuePosition { get; private set; }
    private Deck _deck;
    private bool _inUse;
    private bool _waitingForUse;
    private Makinery _mkDiscard = new Makinery(mk.Priority.CardDiscard) { QueueName = mk.Queues.DeckOperation };
    public bool FinishedUsing
    {
        get
        {
            return !Used || (!_inUse && !_waitingForUse);
        }
    }

    // Use this for initialization
    void Start()
    {
        Toolbox.TryGetDeck(out _deck);
        _mkDiscard.AddRoutine(() => Discard());
    }

    SpriteRenderer GetSpriteRenderer()
    {
        return _spriteRenderer ?? (_spriteRenderer = Instance.GetComponent<SpriteRenderer>());
    }

    public void Reset()
    {
        QueuePosition = 0;
        Used = false;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))
        {
            ICard card = CardType.GetAction();
            if (card.CanUse() && !Used)
            {
                Used = true;

                Makinery cardUse = new Makinery(mk.Priority.CardUse) { QueueName = "CardUse" };
                Toolbox.Instance.MainMakina.AddMakinery(cardUse);

                CardQueueCounter counter = null;

                cardUse.AddRoutine(() => DoLogic(card));
                cardUse.OnQueued += (sender, args) =>
                {
                    QueuePosition = _deck.AddToUsageQueue(this);
                    if (QueuePosition > 0)
                    {
                        var obj = Toolbox.Instance.Pool.Retrieve(Toolbox.Instance.DeckManager.UsageQueuePoolInstance);
                        counter = obj.GetComponent<CardQueueCounter>();
                        counter.Show(Instance.transform, QueuePosition);
                    }
                };

                cardUse.OnEnd += (sender, args) =>
                {
                    _deck.DequeueUsage();
                    if (counter != null)
                    {
                        counter.Hide();
                        Toolbox.Instance.Pool.Release(Toolbox.Instance.DeckManager.UsageQueuePoolInstance, counter.gameObject);
                    }
                };
            }
        }
    }

    public IEnumerator<MakineryGear> Discard(bool reorganize = true)
    {
        while (!FinishedUsing)
        {
            yield return new WaitForFrameCountGear();
        }

        PawSlot slot = _deck.FindCardInPaw(this);
        if (slot != null)
        {
            slot.ReleaseCard();
        }
        Used = false;

        Toolbox.Instance.Pool.Release(PoolInstance, Instance);

        Makinery reorganizeAction = new Makinery(mk.Priority.PawReorganize) { QueueName = mk.Queues.DeckOperation };
        reorganizeAction.AddRoutine(() => _deck.ReorganizePaw());
        Toolbox.Instance.MainMakina.AddMakinery(reorganizeAction);

        Reset();
        _deck.AddToDiscardPile(this);
    }

    private IEnumerator<MakineryGear> DoLogic(ICard card)
    {
        _inUse = true;
        _waitingForUse = false;

        Toolbox.Instance.StartCoroutine(AnimateActive());

        Makinery cardLogic = new Makinery(mk.Priority.CardUse);
        cardLogic.AddRoutine(() => card.DoLogic(this));
        yield return new InnerMakinery(cardLogic,Toolbox.Instance.MainMakina);

        _inUse = false;
        yield return new InnerMakinery(_mkDiscard, Toolbox.Instance.MainMakina);
    }

    private IEnumerator AnimateActive()
    {
        Toolbox.Instance.StartCoroutine(FlashActive());
        /* Vector3 localScale = _spriteRenderer.transform.localScale;
         float time = 0f;
         while (time < 1f && _inUse)
         {
             _spriteRenderer.transform.localScale = Vector3.Lerp(localScale, new Vector3(1.1f, 1.1f, 1f), time * time);
             time += Time.deltaTime;
             yield return new WaitForEndOfFrame();
         }

         while (_inUse)
         {
             yield return new WaitForEndOfFrame();
         }

         _spriteRenderer.transform.localScale = Vector3.one;*/
        yield break;
    }

    private IEnumerator FlashActive()
    {
        float time = 0f;
        while (time < 1f && _inUse)
        {
            _spriteRenderer.material.SetFloat("_LevelsMaxInput", 255 - Mathf.Sin(time*15f)*80f);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (_inUse)
        {
            yield return new WaitForEndOfFrame();
        }

        _spriteRenderer.material.SetFloat("_LevelsMaxInput", 255);
    }

    public void PlayDrawAnimation(Vector3 destination)
    {
        Destination = destination;
        var sprRenderer = GetSpriteRenderer();
        sprRenderer.material.SetFloat("_Opacity", 0f);

        Makinery fadeIn = new Makinery(mk.Priority.CardAnimations);        
        fadeIn.AddRoutine(
            MkLerp.LerpFloat((f) => sprRenderer.material.SetFloat("_Opacity", f),
            () => sprRenderer.material.GetFloat("_Opacity"),
            0.50f,
            () => 1f,
            MkEasing.EasingFunction.SineEaseIn
            ));

        Makinery moveAndFlash = new Makinery(mk.Priority.CardAnimations);
        moveAndFlash.AddRoutine(
            () => MoveToDestination(speed: 15f),
            () => Flash(speed: 2f));

        Toolbox.Instance.MainMakina.AddMakinery(fadeIn, moveAndFlash);
    }

    public IEnumerator<MakineryGear> MoveToDestination(float speed = 5f, bool damp = true)
    {
        Vector2 startingPos = Instance.transform.position;
        float dampFactor = damp ? (Destination - (Vector2)Instance.transform.position).magnitude * 2 : 1f;
        float time = 0f;
        while (time <= 1f || Vector2.Distance(Destination, Instance.transform.position) > 0.01f)
        {
            Instance.transform.position = Vector2.Lerp(startingPos, Destination, time);
            time += Time.deltaTime / dampFactor * speed;
            yield return new WaitForFrameCountGear();
        }
        Instance.transform.position = Destination;
    }

    public IEnumerator<MakineryGear> Flash(float speed = 1f)
    {
        var sprRenderer = GetSpriteRenderer();

        Makinery grow = new Makinery(mk.Priority.CardAnimations);
        grow.AddRoutine(MkLerp.LerpFloat(
            (f) => sprRenderer.transform.localScale = new Vector3(f,f,1),
            () => sprRenderer.transform.localScale.x,
            0.25f,
            () => 1.1f,
            MkEasing.EasingFunction.ExponentialEaseInOut
            ));

        grow.AddRoutine(MkLerp.LerpFloat(
            (f) => sprRenderer.transform.localScale = new Vector3(f, f, 1),
            () => sprRenderer.transform.localScale.x,
            0.25f,
            () => 1f,
            MkEasing.EasingFunction.CircularEaseIn
            ));

        Toolbox.Instance.MainMakina.AddMakinery(grow);

        float time = 0f;
        while (time <= 1f)
        {
            sprRenderer.material.SetFloat("_Luminance", time < 0.45f ? time * 0.35f : (1f - time*time));
            time += Time.deltaTime * speed;
            yield return new WaitForFrameCountGear();
        }
        sprRenderer.material.SetFloat("_Luminance", 0);
    }

}
