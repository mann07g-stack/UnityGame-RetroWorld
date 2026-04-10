using UnityEngine;

public class Spawner : MonoBehaviour
{   
    [Header("Difficulty 1: Normal")]
    public GameObject[] easyPrefabs; // Your standard pipes

    [Header("Difficulty 2: Hard (Score > 10)")]
    public GameObject[] hardPrefabs; // Moving or rotating pipes

    [Header("Settings")]
    public float spawnRate = 1f;
    public float minHeight = -1f;
    public float maxHeight = 2f;
    private int pipesSpawned = 0;


    private void OnEnable()
    {
        InvokeRepeating(nameof(Spawn), spawnRate, spawnRate);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Spawn));
    }

    public void ResetSpawner()
    {
        pipesSpawned = 0;
    }


    private void Spawn()
    {
        GameObject prefabToSpawn;

        // First 10 pipes → EASY
        if (pipesSpawned < 10 && easyPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, easyPrefabs.Length);
            prefabToSpawn = easyPrefabs[randomIndex];
        }
        // After 10 pipes → HARD
        else if (hardPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, hardPrefabs.Length);
            prefabToSpawn = hardPrefabs[randomIndex];
        }
        else
        {
            return; // safety
        }

        GameObject pipes = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        pipes.transform.position += Vector3.up * Random.Range(minHeight, maxHeight);

        pipesSpawned++;
    }

}