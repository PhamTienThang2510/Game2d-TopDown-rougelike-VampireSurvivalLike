using UnityEngine;

public class Despawn : MonoBehaviour
{
    [SerializeField] protected bool canDespawn = true;
    protected virtual void DespawnObj()
    {
        // To be overridden in derived classes
    }
}
