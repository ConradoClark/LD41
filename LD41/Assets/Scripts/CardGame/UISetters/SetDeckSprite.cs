using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDeckSprite : MonoBehaviour {

    public SpriteRenderer DeckSprite;
    private Deck _deck;
    // Use this for initialization
    void Start () {
        Toolbox.Instance.OnPostInit += Instance_OnPostInit;
    }

    private void Instance_OnPostInit(object sender, System.EventArgs e)
    {
        if (Toolbox.TryGetDeck(out _deck))
        {
            _deck.DeckSprite = DeckSprite;
        }        
    }

    // Update is called once per frame
    void Update () {
		
	}
}
