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

    public CardEnum CardType;
    public PoolInstance PoolInstance;
    public GameObject Instance;
    public event EventHandler<CardEvent> OnUsing;
    public event EventHandler<EventArgs> OnUsed;
    public bool Used = false;

    // Use this for initialization
    void Start () {    
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
            if (!Used && !Toolbox.Instance.Deck.Drawing && !Toolbox.Instance.Deck.Reorganizing)
            {
                Used = true;
                var coroutine = StartCoroutine(CardType.GetAction().DoLogic(this, OnUsed));

                if (OnUsing != null)
                {
                    OnUsing(this, new CardEvent() { Card = this });
                }
            }
        }
    }
}
