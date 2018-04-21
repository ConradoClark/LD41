using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawSlot : MonoBehaviour {

    public int Order;
    public Transform Transform;
	// Use this for initialization
	void Start () {
        CardUI cardUI;
        if (Toolbox.TryGetCardUI(out cardUI))
        {
            cardUI.AddPawSlot(this);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
