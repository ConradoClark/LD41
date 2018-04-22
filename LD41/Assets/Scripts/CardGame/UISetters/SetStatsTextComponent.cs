using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetStatsTextComponent : MonoBehaviour {


	// Use this for initialization
	void Start () {
        var tmp = GetComponent<TextMeshPro>();
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            if (new Validation(tmp != null, "SetStatsTextComponent: Objeto nao possui componente TextMeshPro"))
            {
                cardUI.Stats = tmp;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
