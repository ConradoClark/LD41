using Assets.Scripts.CardGame.CardLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public event EventHandler<CardEvent> OnUsing;
    public event EventHandler<EventArgs> OnUsed;
    public bool Used = false;
    private SpriteRenderer _spriteRenderer;
    public int QueuePosition { get; private set; }
    private Deck _deck;
    private bool _inUse;
    private bool _waitingForUse;
    public bool FinishedUsing
    {
        get
        {
            return !Used || (!_inUse && !_waitingForUse);
        }
    }
    public PawSlot CurrentSlot;

    // Use this for initialization
    void Start()
    {
        Toolbox.TryGetDeck(out _deck);
    }

    SpriteRenderer GetSpriteRenderer()
    {
        return _spriteRenderer ?? (_spriteRenderer = Instance.GetComponent<SpriteRenderer>());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Reset()
    {
        QueuePosition = 0;
        Used = false;
        ClearEvents();
    }

    bool QueueUse()
    {
        int queuePosition;
        bool result = _deck.AddToUsageQueue(this, out queuePosition);
        QueuePosition = queuePosition;
        return result;
    }

    public void ClearEvents()
    {
        if (OnUsing != null)
        {
            foreach (var del in OnUsing.GetInvocationList())
            {
                OnUsing -= (EventHandler<CardEvent>)del;
            }
        }

        if (OnUsed != null)
        {
            foreach (var del in OnUsed.GetInvocationList())
            {
                OnUsed -= (EventHandler<EventArgs>)del;
            }
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0))
        {
            ICard card = CardType.GetAction();
            if (card.CanUse() && !Used)
            {
                Used = true;

                Makinery cardUse = new Makinery(50) { QueueName = "CardUse" };
                Toolbox.Instance.MainMakina.AddMakinery(cardUse);

                CardQueueCounter counter = null;

                cardUse.AddRoutine(() => DoLogic(card));
                cardUse.OnQueued += (sender, args) =>
                {
                    QueuePosition = Toolbox.Instance.MainMakina.GetQueuePosition(cardUse);
                    Debug.Log(QueuePosition);
                    if (QueuePosition > 0)
                    {
                        var obj = Toolbox.Instance.Pool.Retrieve(Toolbox.Instance.DeckManager.UsageQueuePoolInstance);
                        counter = obj.GetComponent<CardQueueCounter>();
                        counter.Show(Instance.transform, QueuePosition);
                    }
                };

                cardUse.OnEnd += (sender, args) =>
                {
                    if (counter != null)
                    {
                        counter.Hide();
                        Toolbox.Instance.Pool.Release(Toolbox.Instance.DeckManager.UsageQueuePoolInstance, counter.gameObject);
                    }
                };

                /*if (QueueUse())
                {
                    var peek = _deck.PeekPreviousCard();
                    

                    _waitingForUse = true;
                    //Debug.Log("QUE" + gameObject.ToString());
                    peek.OnUsed += (args, sender) =>
                    {
                        //Debug.Log("QUE NEXT" + gameObject.ToString());
                        Toolbox.Instance.StartCoroutine(DoLogic(card, counter));
                    };
                }
                else
                {
                    //Debug.Log("QUE" + gameObject.ToString());
                    Toolbox.Instance.StartCoroutine(DoLogic(card));
                }*/
            }
        }
    }

    public IEnumerator<MakineryGear> Discard(bool reorganize = true)
    {
        while (!FinishedUsing)
        {
            yield return new WaitForFrameCountGear();
        }

        Debug.Log("Discarding " + gameObject.ToString());
        PawSlot slot = _deck.FindCardInPaw(this);
        if (slot != null)
        {
            slot.ReleaseCard();
        }
        Used = false;

        Toolbox.Instance.Pool.Release(PoolInstance, Instance);

        Makinery reorganizeAction = new Makinery(200) { QueueName = "DeckOp" };
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
        if (OnUsing != null)
        {
            OnUsing(this, new CardEvent() { Card = this });
        }

        Makinery cardLogic = new Makinery(50);
        cardLogic.AddRoutine(() => card.DoLogic(this, null));
        yield return new InnerMakinery(cardLogic,Toolbox.Instance.MainMakina);

        //Debug.Log("DEQ" + gameObject.ToString());

        if (OnUsed != null)
        {
            //Debug.Log("CALLED USED" + gameObject.ToString());
            OnUsed(this, new EventArgs());
        }

        _inUse = false;

        Makinery discard = new Makinery(50);
        discard.AddRoutine(() => Discard());
        yield return new InnerMakinery(discard, Toolbox.Instance.MainMakina);
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

    public IEnumerator MoveToDestination(float speed = 5f, bool damp = true, Action onDestination = null)
    {
        Vector2 startingPos = Instance.transform.position;
        float dampFactor = damp ? (Destination - (Vector2)Instance.transform.position).magnitude * 2 : 1f;
        float time = 0f;
        while (time <= 1f || Vector2.Distance(Destination, Instance.transform.position) > 0.01f)
        {
            Instance.transform.position = Vector2.Lerp(startingPos, Destination, time);
            time += Time.deltaTime / dampFactor * speed;
            yield return new WaitForEndOfFrame();
        }
        Instance.transform.position = Destination;
        if (onDestination != null)
        {
            onDestination();
        }
    }

    public IEnumerator FadeIn(float speed = 1f)
    {
        var sprRenderer = GetSpriteRenderer();
        float time = 0f;
        while (time <= 1f)
        {
            sprRenderer.material.SetFloat("_Opacity", time);
            time += Time.deltaTime * speed;
            yield return new WaitForEndOfFrame();
        }
        sprRenderer.material.SetFloat("_Opacity", 1f);
    }

    public IEnumerator Flash(float speed = 1f)
    {
        var sprRenderer = GetSpriteRenderer();
        float time = 0f;
        while (time <= 1f)
        {
            sprRenderer.material.SetFloat("_Luminance", time < 0.45f ? time * 0.35f : (1f - time));
            time += Time.deltaTime * speed;
            yield return new WaitForEndOfFrame();
        }
        sprRenderer.material.SetFloat("_Luminance", 0);
    }

}
