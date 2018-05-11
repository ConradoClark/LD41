using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour {

    [Flags]
    public enum GridObjectType
    {
        Nothing = 1,
        Floor = 2,
        Player = 4,
        Enemy = 8,        
    }

    public bool Blocking;
    public GridObjectType Type;
    public int Weight;
    public bool Active = true;
    private bool _registered;

    // Use this for initialization
    void Start () {
        if (_registered) return;
        LevelGrid levelGrid;
		if (Toolbox.TryGetLevelGrid(out levelGrid))
        {
            levelGrid.RegisterGridObject(this);
        }
	}
	
    public void PreRegister()
    {
        LevelGrid levelGrid;
        if (Toolbox.TryGetLevelGrid(out levelGrid))
        {
            levelGrid.RegisterGridObject(this);
            _registered = true;
        }        
    }

	// Update is called once per frame
	void Update () {
		
	}
}
