using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SlimeAnimation : MonoBehaviour
{
    [Header("Damage feedback")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private float deathFadeDuration = 0.5f;

    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        spriteRenderer.color = originalColor; // reset khi spawn lại từ pool
    }

    // 🔴 Gọi khi slime bị damage
    public void PlayDamageFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
    }

    // ☠️ Gọi khi slime chết
    public void PlayDeathFade(System.Action onComplete)
    {
        StartCoroutine(DeathFade(onComplete));
    }

    private IEnumerator DeathFade(System.Action onComplete)
    {
        float t = 0f;
        Color c = spriteRenderer.color;

        while (t < deathFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / deathFadeDuration);
            spriteRenderer.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        spriteRenderer.color = new Color(c.r, c.g, c.b, 0f);
        onComplete?.Invoke();
    }
}
