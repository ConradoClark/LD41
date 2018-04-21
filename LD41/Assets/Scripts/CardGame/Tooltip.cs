using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour {

    public string HelpCaption;
    public string HelpDescription;
    CardUI _cardUI;
    bool _enabled = false;
    // Use this for initialization
    void Start() {
        _enabled = Toolbox.TryGetCardUI(out _cardUI);
	}
	
	// Update is called once per frame
	void Update () {
	}

    void OnMouseEnter()
    {
        if (!_enabled) return;
        _cardUI.UpdateHelp(HelpCaption, HelpDescription);
    }

    void OnMouseExit()
    {
        if (!_enabled) return;
        _cardUI.ResetHelp();
    }
}
