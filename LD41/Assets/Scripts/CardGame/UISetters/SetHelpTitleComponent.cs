using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetHelpTitleComponent : MonoBehaviour {

    // Use this for initialization
    void Awake() {
        var tmp = GetComponent<TextMeshPro>();
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            if (new Validation(tmp != null, "SetHelpTitleComponent: Objeto nao possui componente TextMeshPro"))
            {
                cardUI.HelpTitle = tmp;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
