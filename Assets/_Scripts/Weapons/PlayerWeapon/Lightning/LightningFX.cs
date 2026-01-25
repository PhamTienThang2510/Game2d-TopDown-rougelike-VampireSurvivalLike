using UnityEngine;

public class LightningFX : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.15f;
    [SerializeField] private Vector3 fixedScale = new(1f, 2f, 1f);
    [SerializeField] private float heightOffset = 3f;

    public void Play()
    {
        // luôn đánh từ trên xuống
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // scale cố định
        transform.localScale = fixedScale;

        // spawn cao hơn enemy 1 chút
        transform.position += Vector3.up * heightOffset;

        CancelInvoke();
        Invoke(nameof(Despawn), lifeTime);
    }

    private void Despawn()
    {
        PoolManager.Instance.Despawn(CONSTANT.Fx_Lightning, gameObject);
    }
}
