using UnityEngine;
using TMPro;
using System.Collections;

public class DameTextAnimation : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Pool")]
    [SerializeField] private string poolKey = "DameText";

    private TMP_Text text;
    private Color originColor;

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
        originColor = text.color;
    }

    private void OnEnable()
    {
        // reset alpha mỗi lần spawn
        Color c = originColor;
        c.a = 1f;
        text.color = c;

        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            Color c = text.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            text.color = c;

            yield return null;
        }

        // đảm bảo alpha = 0
        Color end = text.color;
        end.a = 0f;
        text.color = end;

        // trả về pool
        PoolManager.Instance.Despawn(poolKey, gameObject);
    }

    public void SetDamage(int damage)
    {
        text.text = damage.ToString();
    }
}
