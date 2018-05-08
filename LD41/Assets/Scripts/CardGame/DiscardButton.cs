using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardButton : MonoBehaviour {

    public event EventHandler<EventArgs> OnButtonPressed;
    public bool ButtonActive;
    public bool SlotOpen;
    public Sprite ActiveSprite;
    public Sprite InactiveSprite;
    public SpriteRenderer SpriteRenderer;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Toolbox.Instance.DiscardCounter == null) return;
        ButtonActive = SlotOpen && Toolbox.Instance.DiscardCounter.GetDiscards() > 0;

        if (SpriteRenderer == null) return;
        SpriteRenderer.sprite = ButtonActive ? ActiveSprite : InactiveSprite;
    }

    void OnMouseOver()
    {
        if (!Toolbox.Instance.MainCharacter.IsDead && Input.GetMouseButtonUp(0))
        {
            if (SlotOpen && ButtonActive && OnButtonPressed != null && !Toolbox.Instance.Deck.Drawing && !Toolbox.Instance.Deck.Reorganizing)
            {                
                OnButtonPressed.Invoke(this, new EventArgs());
            }
        }
    }
}

