using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameStatsManager : MonoBehaviour
{
    public bool isRestartingFromCollision = false;
    public bool levelCompleted = false;


    public static GameStatsManager Instance;

    public int totalCollisions = 0;
    public float levelTime = 0f;

    private float timer = 0f;
    public int levelsCompleted = 0;
    private TextMeshProUGUI collisionText;
    private bool uiReady = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Only track time if a playable level is loaded
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("LEVEL"))
        {
            timer += Time.deltaTime;
            levelTime = timer;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    uiReady = false;
    Invoke(nameof(FindCollisionText), 0.1f);

    if (scene.name == "LEVEL01")
    {
        if (!isRestartingFromCollision)
        {
            ResetStats(); // ✅ Only reset on fresh play, not on collision
        }

        isRestartingFromCollision = false; // ✅ Reset flag after scene load
    }
}


    void FindCollisionText()
    {
        collisionText = GameObject.Find("canvas/CollisionText")?.GetComponent<TextMeshProUGUI>();

        if (collisionText != null)
        {
            collisionText.text = "Collisions: " + totalCollisions;
            uiReady = true;
            Debug.Log("✅ CollisionText found and updated.");
        }
        else
        {
            Debug.LogWarning("❌ CollisionText not found in scene.");
        }
    }

    public void IncrementCollision()
{
    if (levelCompleted) return; // ✅ Prevent collision counting after level finish

    totalCollisions++;
    Debug.Log("🔁 Total collisions: " + totalCollisions);

    if (uiReady && collisionText != null)
    {
        collisionText.text = "Collisions: " + totalCollisions;
    }
}

    public void ResetStats()
    {
        totalCollisions = 0;
        timer = 0f;
        levelTime = 0f;
        levelsCompleted=0;
        levelCompleted = false; 

        if (uiReady && collisionText != null)
        {
            collisionText.text = "Collisions: 0";
        }

        Debug.Log("📊 Game stats reset.");
    }
}
