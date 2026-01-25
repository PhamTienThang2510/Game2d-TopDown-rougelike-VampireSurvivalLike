using UnityEngine;
using TMPro;

public class EnemySpawnerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timePlayedText;

    private float timePlayed;

    private void Start()
    {
        timePlayed = 0f;
    }

    private void Update()
    {
        UpdateTimePlayed();
    }

    // =========================
    // TIME PLAYED
    // =========================
    private void UpdateTimePlayed()
    {
        timePlayed += Time.deltaTime;

        int minutes = Mathf.FloorToInt(timePlayed / 60f);
        int seconds = Mathf.FloorToInt(timePlayed % 60f);

        timePlayedText.text = $"{minutes:00}:{seconds:00}";
    }
}
