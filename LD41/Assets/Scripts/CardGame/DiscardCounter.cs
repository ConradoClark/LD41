using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardCounter : MonoBehaviour {

    private Deck _deck;
    private MainCharacter _mainCharacter;
    public SpriteRenderer SpriteRenderer;
    public float widthGrowth;
    private float _discards;

	// Use this for initialization
	void Start () {
        Toolbox.Instance.DiscardCounter = this;
        Toolbox.Instance.OnPostInit += Instance_OnPostInit;
        Toolbox.TryGetMainCharacter(out _mainCharacter);
        SetDiscards(0);
    }

    private void Instance_OnPostInit(object sender, System.EventArgs e)
    {
        Toolbox.TryGetDeck(out _deck);
    }

    // Update is called once per frame
    void Update () {
        _discards += _mainCharacter.Stats.Agility * widthGrowth * 0.1f * Time.deltaTime;
        _discards = Mathf.Clamp(_discards, 0, _mainCharacter.Stats.Discard);
        UpdateSprite();
    }

    public void SetDiscards(int amount)
    {
        _discards = amount = Math.Min(amount, _mainCharacter.Stats.Discard);
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (SpriteRenderer == null) return;
        SpriteRenderer.size = new Vector2(widthGrowth * _discards, SpriteRenderer.size.y);
    }

    public void Decrease()
    {
        _discards = Math.Max(0, _discards - 1);
        UpdateSprite();
    }

    public int GetDiscards()
    {
        return Mathf.FloorToInt(_discards);
    }
}
