using Assets.Scripts.Core;
using TMPro;
using UnityEngine;

public class SetDiscardCountComponent : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        var tmp = GetComponent<TextMeshPro>();
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            if (new Validation(tmp != null, "SetDiscardCountComponent: Objeto nao possui componente TextMeshPro"))
            {
                cardUI.DiscardCount = tmp;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
