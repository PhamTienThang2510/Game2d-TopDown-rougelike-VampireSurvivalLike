using UnityEngine;

public class Treasure : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerExp>(out PlayerExp playerExp))
        {
            playerExp.LevelUpByTreasure();
            Destroy(this.gameObject);
        }
    }
}
