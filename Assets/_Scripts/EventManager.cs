using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField] private List<EventData> eventDatas;
    [SerializeField] public float eventInterval = 90f;
    public float eventTimer = 0f;
    Transform Player;
    public static EventManager instance;
    private void Awake()
    {
        instance = this;
        Player = GameObject.FindGameObjectWithTag(CONSTANT.PLAYER_TAG).transform;
    }

    public void PickRandomEvent()
    {
        //PlayEvent(eventDatas[Random.Range(0, eventDatas.Count)]);
        PlayEvent(eventDatas[0]);
    }

    public void PlayEvent(EventData eventData)
    {
        if (eventData == null)
            return;
        eventTimer = eventData.spawnCount;
        float angleStep = 360f / Mathf.Max(1, eventData.spawnCount);

        for (int i = 0; i < eventData.spawnCount; i++)
        {
            float angle = angleStep * i;
            Vector3 spawnPos = GetPositionOnCircle(angle);
            GameObject enemyRing = PoolManager.Instance.Spawn(eventData.prefabkey, spawnPos);
        }
    }
    public void EndEvent()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(CONSTANT.ENEMY_TAG);
        foreach (GameObject enemy in enemies)
        {
            PoolManager.Instance.Despawn(eventDatas[0].prefabkey, enemy);
        }
    }
    private Vector3 GetPositionOnCircle(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        float x = Player.position.x + Mathf.Cos(rad) * 7f;
        float y = Player.position.y + Mathf.Sin(rad) * 7f;

        return new Vector3(x, y, 0f);
    }
}

