using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetHelpDescriptionComponent : MonoBehaviour {

    // Use this for initialization
    void Awake()
    {
        var tmp = GetComponent<TextMeshPro>();
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            if (new Validation(tmp != null, "SetHelpTitleComponent: Objeto nao possui componente TextMeshPro"))
            {
                cardUI.HelpDescription = tmp;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
