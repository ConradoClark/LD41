using UnityEngine;
using System.Collections;

public class Pool : MonoBehaviour
{
    public PoolInstance[] pools;

    void Start()
    {
        if (pools == null) return;
        Initialize();
    }

    public void Initialize()
    {
        for (int i = 0; i < pools.Length; i++)
        {
            Toolbox.Instance.Pool.AddInstanceToPool(pools[i]);
        }
    }
}
