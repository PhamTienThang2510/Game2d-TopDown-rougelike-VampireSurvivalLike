using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Map Objects")]
    [SerializeField] private List<MapData> mapObjects;
    public LayerMask terrainMask;
    public GameObject currentMap;
    [SerializeField] private float radius = 0.2f;
    [SerializeField] private float spawnCooldown = 0f;
    [SerializeField] private float spawnDelay = 0.15f;
    [Header("PoolMap")]
    private Queue<GameObject> poolMap = new Queue<GameObject>();
    private PlayerController playerController;
    void Awake()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();
    }

    void Start()
    {

    }

    void Update()
    {
        if (!currentMap) return;
        spawnCooldown -= Time.deltaTime;
        if (spawnCooldown > 0) return;
        Vector2 dir = playerController.PlayerDirection;
        Transform target = null;

        if (dir.x > 0 && dir.y == 0) target = currentMap.transform.Find("Right");
        else if (dir.x < 0 && dir.y == 0) target = currentMap.transform.Find("Left");
        else if (dir.x == 0 && dir.y > 0) target = currentMap.transform.Find("Up");
        else if (dir.x == 0 && dir.y < 0) target = currentMap.transform.Find("Down");
        else if (dir.x > 0 && dir.y > 0) target = currentMap.transform.Find("RightUp");
        else if (dir.x > 0 && dir.y < 0) target = currentMap.transform.Find("RightDown");
        else if (dir.x < 0 && dir.y > 0) target = currentMap.transform.Find("LeftUp");
        else if (dir.x < 0 && dir.y < 0) target = currentMap.transform.Find("LeftDown");

        if (target && !Physics2D.OverlapCircle(target.position, radius, terrainMask))
        {
            GenMap(target.position);
            spawnCooldown = spawnDelay;
        }
    }

    private void GenMap(Vector2 pos)
    {
        Instantiate(getRandomMap(), pos, Quaternion.identity);
    }

    private GameObject getRandomMap()
    {
        float total = 0f;
        foreach (var data in mapObjects) total += data.mapRate;

        float rand = Random.value * total;
        float cumulative = 0f;

        foreach (var data in mapObjects)
        {
            cumulative += data.mapRate;
            if (rand <= cumulative) return data.Map;
        }

        return mapObjects[^1].Map;
    }
}
