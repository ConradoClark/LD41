using UnityEngine;
using System.Collections;

public class PoolableObject : MonoBehaviour
{
    public PoolInstance poolInstance;
    public virtual void ResetState()
    {

    }

    public virtual void Connect(PoolInstance instance, bool isPostConector = false)
    {
        if (!isPostConector)
        {
            this.poolInstance = instance;
        }
    }
}
