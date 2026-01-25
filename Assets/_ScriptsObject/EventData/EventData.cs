using UnityEngine;

[CreateAssetMenu(fileName = "EventData", menuName = "Scriptable Objects/EventData")]
public class EventData : ScriptableObject
{
    [SerializeField] public string eventName;
    [Header("Prefabs")]
    [SerializeField] public string prefabkey;
    [SerializeField] public GameObject prefabs;
    [SerializeField] public int spawnCount;
}
