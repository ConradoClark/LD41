using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour {


	// Use this for initialization
	void Start () {
        LevelGrid levelGrid;
		if (Toolbox.TryGetLevelGrid(out levelGrid))
        {
            levelGrid.RegisterGridObject(this);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
