using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData")]
public class MapData : ScriptableObject
{
    public string mapKey;
    public GameObject Map;
    public List<GameObject> obstacles;
    public List<Transform> spawnobstaclesPoints;
    public float mapRate;
}
