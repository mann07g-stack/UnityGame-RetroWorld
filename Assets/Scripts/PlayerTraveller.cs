using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTraveler : MonoBehaviour
{
    public static PlayerTraveler Instance;

    // We look for both types of renderers just in case
    private SpriteRenderer mySprite; 
    private MeshRenderer myMesh;

    private void Awake()
    {
        // 1. Singleton Logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 2. AUTO-DETECT VISUALS
        // We find the component that draws the player so we can turn ONLY that off
        mySprite = GetComponent<SpriteRenderer>();
        myMesh = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 3. VISIBILITY LOGIC
        // We toggle the RENDERER, not the GameObject.
        // This keeps the script running (so it can detect the next scene change)
        // but makes the player invisible to the eye.
        
        bool shouldBeVisible = (scene.name == "MainMap"); // Change this if your scene name is different

        if (mySprite != null) mySprite.enabled = shouldBeVisible;
        if (myMesh != null) myMesh.enabled = shouldBeVisible;

        // 4. SPAWN LOGIC (Snaps player to spawn point)
        GameObject spawnPoint = GameObject.Find("SpawnPoint");

        if (spawnPoint != null)
        {
            // Temporarily disable physics to stop glitching while moving
            Rigidbody rb = GetComponent<Rigidbody>();
            Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
            
            if (rb) rb.isKinematic = true;
            if (rb2d) rb2d.bodyType = RigidbodyType2D.Kinematic;

            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;

            if (rb) rb.isKinematic = false;
            if (rb2d) rb2d.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}