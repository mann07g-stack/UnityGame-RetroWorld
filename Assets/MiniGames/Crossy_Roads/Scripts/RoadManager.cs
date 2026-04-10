using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public static RoadManager Instance;
    public Transform[] roads;

    void Awake()
    {
        Instance = this;
    }

    public Vector3 GetRoad1Spawn()
    {
        return roads[0].position + Vector3.up * 1f;
    }
}
