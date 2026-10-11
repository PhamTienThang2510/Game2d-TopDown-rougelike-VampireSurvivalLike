using UnityEngine;

public enum SpecialEventType
{
    RingAmbush = 0,     // Vòng vây siết chặt 360 độ quanh Player
    SwarmStampede = 1,  // Đàn quái ùa ngang qua màn hình theo đội hình
    EliteHunter = 2,    // Quái Tinh Anh (Elite) săn đuổi, giết rơi Rương Treasure
    SniperSquad = 3     // Đội hình quái bắn xa bao vây các góc
}

[CreateAssetMenu(fileName = "EventData", menuName = "Scriptable Objects/EventData")]
public class EventData : ScriptableObject
{
    [SerializeField] public string eventName;
    [SerializeField] public SpecialEventType eventType = SpecialEventType.RingAmbush;

    [Header("Prefabs")]
    [SerializeField] public string prefabkey;
    [SerializeField] public GameObject prefabs;
    [SerializeField] public int spawnCount = 20;

    [Header("Timing & Weight")]
    [SerializeField] public float eventDuration = 25f;
    [SerializeField] public float probability = 40f;

    [Header("Event Parameters")]
    [SerializeField] public float spawnRadius = 7.5f;
    [SerializeField] public float stampedeSpeedMultiplier = 1.75f;
}
