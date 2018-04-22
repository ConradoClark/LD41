using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PoolManager : MonoBehaviour
{
    public Dictionary<PoolInstance, PooledObject[]> pools { get; private set; }
    private GameObject pool;

    void Awake()
    {
        pools = new Dictionary<PoolInstance, PooledObject[]>();
        pool = new GameObject("pooling");
        pool.transform.SetParent(this.transform);
    }

    public void AddInstanceToPool(PoolInstance instance)
    {
        this.pools.Add(instance, new PooledObject[instance.count]);
        for (int i = 0; i < this.pools[instance].Length; i++)
        {
            this.pools[instance][i] = new PooledObject() { available = true };
            this.pools[instance][i].poolObject = GameObject.Instantiate(instance.objectType).transform;
            this.pools[instance][i].poolObject.gameObject.SetActive(false);
            this.pools[instance][i].poolObject.transform.SetParent(this.pool.transform, false);
            var poolBehaviours = this.pools[instance][i].poolObject.GetComponentsInChildren<PoolableObject>();
            this.pools[instance][i].poolBehaviours = poolBehaviours;
            for (int p = 0; p < poolBehaviours.Length; p++)
            {
                poolBehaviours[p].Connect(instance);
            }
        }
    }

    public GameObject Retrieve(PoolInstance poolInstance)
    {
        return Retrieve(poolInstance, Vector3.zero, Quaternion.identity);
    }

    public GameObject Retrieve(PoolInstance poolInstance, Vector3 position)
    {
        return Retrieve(poolInstance, position, Quaternion.identity);
    }

    public GameObject Retrieve(PoolableObject pool)
    {
        return Retrieve(pool, Vector3.zero, Quaternion.identity);
    }

    public GameObject Retrieve(PoolableObject pool, Vector3 position)
    {
        return Retrieve(pool, position, Quaternion.identity);
    }

    public GameObject Retrieve(PoolableObject pool, Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        if (pool == null || pool.poolInstance == null)
        {
            throw new System.Exception("Invalid pool Object");
        }

        // No pool? 
        if (!pools.ContainsKey(pool.poolInstance))
        {
            bool found = false;
            // Check if a similar pool exists and reconnect
            for (int i = 0; i < pools.Keys.Count; i++)
            {
                var elem = pools.Keys.ElementAt(i);
                if (elem.objectType.GetInstanceID() == pool.poolInstance.objectType.GetInstanceID())
                {
                    pool.Connect(pools.Keys.ElementAt(i), true);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // If not, use Unity's built-in instantiate
                obj = Instantiate(pool.poolInstance.objectType);
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                return obj;
            }
        }
        return Retrieve(pool.poolInstance, position, rotation);
    }

    public GameObject Retrieve(PoolInstance pool, Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        if (pool == null)
        {
            throw new System.Exception("Invalid or disconnected pool instance");
        }

        // No pool? 
        if (!pools.ContainsKey(pool))
        {
            obj = Instantiate(pool.objectType);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        // Try to find an available slot from pool
        for (int i = 0; i < pools[pool].Length; i++)
        {
            if (pools[pool][i].available)
            {
                pools[pool][i].poolObject.gameObject.SetActive(true);
                for (int p = 0; p < pools[pool][i].poolBehaviours.Length; p++)
                {
                    pools[pool][i].poolBehaviours[p].ResetState();
                }
                pools[pool][i].poolObject.position = position;
                pools[pool][i].poolObject.rotation = rotation;
                pools[pool][i].available = false;
                return pools[pool][i].poolObject.gameObject;
            }
        }

        // No room for pool, instantiate
        obj = Instantiate(pool.objectType);
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    public void Release(PoolableObject pool, GameObject obj)
    {
        // No pool? Destroy object
        if (pool == null || pool.poolInstance == null)
        {
            GameObject.Destroy(obj);
            return;
        }

        // Unknown pool? Get out
        if (!pools.ContainsKey(pool.poolInstance))
        {
            bool found = false;
            // Check if a similar pool exists and reconnect
            for (int i = 0; i < pools.Keys.Count; i++)
            {
                if (pools.Keys.ElementAt(i).objectType.GetInstanceID() == obj.GetInstanceID())
                {
                    pool.Connect(pools.Keys.ElementAt(i), true);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // If not, get out
                return;
            }
        }

        Release(pool.poolInstance, obj);
    }

    public void Release(PoolInstance poolInstance, GameObject obj)
    {
        // No pool instance? Destroy object
        if (poolInstance == null)
        {
            GameObject.Destroy(obj);
            return;
        }

        // Unknown pool? Get out
        if (!pools.ContainsKey(poolInstance))
        {
            return;
        }

        // Try to find from pool and deactivate it
        for (int i = 0; i < pools[poolInstance].Length; i++)
        {
            if (pools[poolInstance][i].poolObject.transform.GetInstanceID() == obj.transform.GetInstanceID())
            {
                pools[poolInstance][i].available = true;
                pools[poolInstance][i].poolObject.gameObject.SetActive(false);
                return;
            }
        }

        // No pool, kill the object
        GameObject.Destroy(obj);
    }
}
