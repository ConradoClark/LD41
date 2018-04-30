using Assets.Scripts.CardGame.CardLogic;
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

    // Use this for initialization
    void Start () {
    }

    SpriteRenderer GetSpriteRenderer()
    {
        return _spriteRenderer ?? (_spriteRenderer = Instance.GetComponent<SpriteRenderer>());
    }

    private void Instance_OnPostInit(object sender, System.EventArgs e)
    {
    }
    // Update is called once per frame
    void Update () {
		
	}

    public void Reset()
    {
        Used = false;
        ClearEvents();
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
            if (card.CanUse() && !Used && !Toolbox.Instance.Deck.Drawing && !Toolbox.Instance.Deck.Reorganizing)
            {
                Used = true;
                var coroutine = StartCoroutine(card.DoLogic(this, OnUsed));

                if (OnUsing != null)
                {
                    OnUsing(this, new CardEvent() { Card = this });
                }
            }
        }
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
