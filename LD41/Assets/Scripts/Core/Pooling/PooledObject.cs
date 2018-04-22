using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class PooledObject
{
    public Transform poolObject;
    public bool available;
    public PoolableObject[] poolBehaviours;
}
