﻿using Assets.Scripts.CardGame.CardLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {

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

    // Use this for initialization
    void Start () {
        Toolbox.TryGetDeck(out _deck);
    }

    SpriteRenderer GetSpriteRenderer()
    {
        return _spriteRenderer ?? (_spriteRenderer = Instance.GetComponent<SpriteRenderer>());
    }

    // Update is called once per frame
    void Update () {
		
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

                if (QueueUse())
                {
                    var peek = _deck.PeekPreviousCard();
                    CardQueueCounter counter = null;
                    if (QueuePosition > 0)
                    {
                        var obj = Toolbox.Instance.Pool.Retrieve(Toolbox.Instance.DeckManager.UsageQueuePoolInstance);
                        counter = obj.GetComponent<CardQueueCounter>();
                        counter.Show(Instance.transform, QueuePosition);
                    }

                    Debug.Log("QUE" + gameObject.ToString());
                    peek.OnUsed += (args, sender) =>
                    {
                        Debug.Log("QUE NEXT" + gameObject.ToString());
                        Toolbox.Instance.StartCoroutine(DoLogic(card, counter));
                    };
                }
                else
                {
                    Debug.Log("QUE" + gameObject.ToString());
                    Toolbox.Instance.StartCoroutine(DoLogic(card));
                }               
            }
        }
    }

    private IEnumerator DoLogic(ICard card, CardQueueCounter counter = null)
    {
        if (OnUsing != null)
        {
            OnUsing(this, new CardEvent() { Card = this });
        }

        if (counter != null)
        {
            counter.Hide();
            Toolbox.Instance.Pool.Release(Toolbox.Instance.DeckManager.UsageQueuePoolInstance, counter.gameObject);
        }

        yield return Toolbox.Instance.StartCoroutine(card.DoLogic(this, null));
        Debug.Log("DEQ" + gameObject.ToString());

        if (OnUsed != null)
        {
            Debug.Log("CALLED USED" + gameObject.ToString());
            OnUsed(this, new EventArgs());
        }
        else
        {
            Debug.Log("CALLED WAS NULL!" + gameObject.ToString());
        }

        _deck.DequeueUsage();
    }

    public IEnumerator MoveToDestination(float speed = 5f, bool damp = true, Action onDestination = null)
    {
        Vector2 startingPos = Instance.transform.position;
        float dampFactor = damp ? (Destination - (Vector2)Instance.transform.position).magnitude * 2 : 1f;
        float time = 0f;
        while (time <= 1f || Vector2.Distance(Destination, Instance.transform.position)>0.01f)
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
