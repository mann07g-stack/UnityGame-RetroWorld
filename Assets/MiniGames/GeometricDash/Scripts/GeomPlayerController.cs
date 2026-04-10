using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GeomPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10.4f;
    public float jumpForce = 26.6581f;

    [Header("Orb Jump (Fixed Height)")]
    public float orbJumpVelocity = 22.5f;
    public float miniOrbMultiplier = 0.75f;

    [Header("State")]
    public bool gravityInverted = false;

    [Header("Speed Portals")]
    public float slowSpeed = 8.6f;
    public float normalSpeed = 10.4f;
    public float fastSpeed = 15.6f;

    [Header("Death Effects")]
    public GameObject deathEffectPrefab;
    public float deathDelay = 1.0f;

    [Header("Audio")]
    public MusicController musicController; // drag MusicPlayer here
    public AudioSource deathAudio;           // drag Death AudioSource here

    [Header("Rotation")]
    public float airRotationSpeed = 360f;
    Transform visual;

    bool isMini = false;
    Rigidbody2D rb;
    bool isGrounded;
    bool jumpRequested;
    bool orbJumpRequested;

    bool isOverlappingOrb = false;
    bool orbUsed = false;
    bool inBlueOrb = false;

    bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.None;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        visual = transform.Find("Visual");
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetKey(KeyCode.Space) && isGrounded)
            jumpRequested = true;

        if (Input.GetKey(KeyCode.Space) && isOverlappingOrb && !orbUsed)
        {
            if (inBlueOrb) FlipGravity();
            jumpRequested = true;
            orbJumpRequested = true;
            orbUsed = true;
        }

        HandleRotation();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            float force = orbJumpRequested ? orbJumpVelocity : jumpForce;
            if (orbJumpRequested && isMini) force *= miniOrbMultiplier;
            if (gravityInverted) force = -force;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
            jumpRequested = false;
            orbJumpRequested = false;
        }
    }

    public void SetSpeed(float newSpeed) { moveSpeed = newSpeed; }

    public void ToggleMini(bool mini)
    {
        isMini = mini;
        float baseScale = mini ? 0.5f : 1f;
        float yDir = gravityInverted ? -1f : 1f;
        transform.localScale = new Vector3(baseScale, baseScale * yDir, 1f);
    }

    public void SetOrbOverlap(bool state, bool blue)
    {
        if (!state) { orbUsed = false; inBlueOrb = false; }
        isOverlappingOrb = state;
        inBlueOrb = blue;
    }

    void FlipGravity()
    {
        gravityInverted = !gravityInverted;
        rb.gravityScale *= -1;

        float yScale = Mathf.Abs(transform.localScale.y);
        transform.localScale = new Vector3(
            transform.localScale.x,
            gravityInverted ? -yScale : yScale,
            1
        );
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;

        if (col.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D c in col.contacts)
            {
                Vector2 n = c.normal;

                if (!gravityInverted && n.y > 0.5f) { isGrounded = true; return; }
                if (gravityInverted && n.y < -0.5f) { isGrounded = true; return; }

                if (Mathf.Abs(n.x) > 0.5f)
                {
                    StartCoroutine(Die());
                    return;
                }
            }
        }

        if (col.gameObject.CompareTag("Spike"))
            StartCoroutine(Die());
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    void HandleRotation()
    {
        if (visual == null) return;

        if (!isGrounded)
        {
            float dir = gravityInverted ? -1f : 1f;
            visual.Rotate(0f, 0f, -airRotationSpeed * dir * Time.deltaTime);
        }
        else
        {
            float z = visual.eulerAngles.z;
            z = Mathf.Round(z / 90f) * 90f;
            visual.rotation = Quaternion.Euler(0f, 0f, z);
        }
    }

    IEnumerator Die()
    {
        isDead = true;

        // Disable physics & visuals
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;

        if (visual != null)
            visual.gameObject.SetActive(false);

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        // Wait for animation length
        yield return new WaitForSeconds(deathDelay);

        // Restart via GameManager
        GeomGameManager.Instance.RestartLevel(0f);
    }

}
