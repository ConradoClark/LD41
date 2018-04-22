using UnityEngine;
using System.Collections;

public class Pool : MonoBehaviour
{
    public PoolInstance[] pools;

    void Start()
    {
        for (int i = 0; i < pools.Length; i++)
        {
            Toolbox.Instance.Pool.AddInstanceToPool(pools[i]);
        }
    }
}
