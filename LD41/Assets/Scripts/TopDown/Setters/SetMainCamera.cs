using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMainCamera : MonoBehaviour {
    public Camera MainCamera;
	// Use this for initialization
	void Start () {
        Toolbox.Instance.MainCamera = MainCamera;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
