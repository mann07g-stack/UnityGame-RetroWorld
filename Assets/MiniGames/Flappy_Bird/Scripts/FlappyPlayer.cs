using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 

public class FlappyPlayer : MonoBehaviour
{   
    [Header("Settings")]
    public float strength = 5f;
    public float gravity = -9.8f;
    public float tilt = 5f;

    [Header("Death Settings")]
    public GameObject deathEffectPrefab; 
    public float deathDelay = 1.0f;      

    [Header("Visuals")]
    public Sprite[] sprites;
    private SpriteRenderer spriteRenderer;
    private int spriteIndex;
    
    [Header("Audio Settings")]
    public AudioSource sfxSource;   // Drag AudioSource here (or script creates one)
    public AudioClip flapSound;     // Drag Flap sound
    public AudioClip hitSound;      // Drag Hit sound

    private Vector3 direction;
    private bool isDead = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(AnimateSprite), 0.15f, 0.15f);

        // Auto-create AudioSource if you forgot to add one
        if (sfxSource == null) 
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        Vector3 position = transform.position;
        position.y = 0f;
        transform.position = position;
        direction = Vector3.zero;
        
        isDead = false;
        GetComponent<Collider2D>().enabled = true;
        if(spriteRenderer != null) spriteRenderer.enabled = true;
    }

    private void Update()
    {
        if (isDead) return;

        // 1. INPUT: Added KeyCode.Backspace here
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Backspace))
        {
            direction = Vector3.up * strength;
            
            // --- PLAY FLAP SOUND ---
            if (sfxSource != null && flapSound != null)
            {
                sfxSource.PlayOneShot(flapSound);
            }
        }

        // Touch Input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                direction = Vector3.up * strength;
                // Touch Flap Sound
                if (sfxSource != null && flapSound != null) sfxSource.PlayOneShot(flapSound);
            }
        }

        direction.y += gravity * Time.deltaTime;
        transform.position += direction * Time.deltaTime;

        Vector3 rotation = transform.eulerAngles;
        rotation.z = direction.y * tilt;
        transform.eulerAngles = rotation;
    }

    private void AnimateSprite()
    {
        if (isDead) return; 

        spriteIndex++;
        if (spriteIndex >= sprites.Length)
        {
            spriteIndex = 0;
        }
        if (spriteIndex < sprites.Length && spriteIndex >= 0) {
            spriteRenderer.sprite = sprites[spriteIndex];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.gameObject.CompareTag("Obstacle"))
        {
            // --- PLAY HIT SOUND ---
            if (sfxSource != null && hitSound != null)
            {
                sfxSource.PlayOneShot(hitSound);
            }

            StartCoroutine(DieSequence());
        }
        else if (other.CompareTag("Scoring"))
        {
            FlappyGameManager.Instance.IncreaseScore();
        }
    }

    IEnumerator DieSequence()
    {
        isDead = true;

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        spriteRenderer.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        
        direction = Vector3.zero;

        yield return new WaitForSeconds(deathDelay);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}