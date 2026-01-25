using UnityEngine;

public class GemPickUp : MonoBehaviour
{
    [SerializeField] private int expValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(CONSTANT.PLAYER_TAG)) return;

        PlayerExp playerExp = other.GetComponent<PlayerExp>();
        if (playerExp != null)
        {
            playerExp.AddExp(expValue);
        }

        PoolManager.Instance.Despawn(CONSTANT.Gem_exp,this.gameObject);
    }
}
