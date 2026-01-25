using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [System.Serializable]
    public class PoolItem
    {
        public string key;
        public GameObject prefab;
        public int preloadAmount = 5;
    }

    public List<PoolItem> items;
    //LIFO
    private Dictionary<string, Queue<GameObject>> poolDict;

    private void Awake()
    {
        Instance = this;
        poolDict = new Dictionary<string, Queue<GameObject>>();

        foreach (var item in items)
        {
            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < item.preloadAmount; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.transform.name = item.key;
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDict.Add(item.key, queue);
        }
    }

    public GameObject Spawn(string key, Vector3 pos)
    {
        if (!poolDict.ContainsKey(key)) return null;

        GameObject obj;

        if (poolDict[key].Count > 0)
        {
            obj = poolDict[key].Dequeue();
        }
        else
        {
            obj = Instantiate(items.Find(i => i.key == key).prefab);
            obj.name = key; 
        }

        obj.transform.position = pos;
        obj.SetActive(true);
        return obj;
    }

    public bool HasKey(string key)
    {
        return poolDict.ContainsKey(key);
    }

    public void Despawn(string key, GameObject obj)
    {
        obj.SetActive(false);
        poolDict[key].Enqueue(obj);
    }
}
