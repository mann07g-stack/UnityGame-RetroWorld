using UnityEngine;
using System.Collections;

public class PlayerMovementSingle : MonoBehaviour
{
    public Rigidbody rb;
    public float Defaultforce = 2000F;
    public float forwardforce = 500F;
    public float rightforce = 500F;
    public float leftforce = 500F;
    public float backforce = 500F;
    private float currentSpeed;

    void Start()
    {
        currentSpeed = Defaultforce;
    }

    void FixedUpdate()
    {
        // --- 1. PAUSE / GAME OVER CHECK ---
        if (CubaceGameManager.Instance != null)
        {
            if (CubaceGameManager.Instance.gameHasEnded || CubaceGameManager.Instance.isPaused) 
                return;
        }

        rb.AddForce(0, 0, Defaultforce * Time.deltaTime);

        if (Input.GetKey("w")) rb.AddForce(0, 0, forwardforce * Time.deltaTime);
        if (Input.GetKey("a")) rb.AddForce(-leftforce * Time.deltaTime, 0, 0, ForceMode.VelocityChange);
        if (Input.GetKey("s")) rb.AddForce(0, 0, -backforce * Time.deltaTime);
        if (Input.GetKey("d")) rb.AddForce(rightforce * Time.deltaTime, 0, 0, ForceMode.VelocityChange);

        // Fall Check (Lose)
        if (rb.position.y < -5f)
        {
            if(CubaceGameManager.Instance != null)
                CubaceGameManager.Instance.EndGame();
        }
    }

    // --- NEW: CRASH DETECTION (LOSE) ---
    void OnCollisionEnter(Collision collision)
    {
        // If we hit an obstacle/wall, we DIE.
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("💥 Player crashed into wall!");
            if(CubaceGameManager.Instance != null)
                CubaceGameManager.Instance.EndGame(); // Calls Lose, NOT Win
        }
    }
    // -----------------------------------

    public void ModifySpeed(float factor)
    {
        Defaultforce *= factor;
        forwardforce *= factor;
        rightforce *= factor;
        leftforce *= factor;
        backforce *= factor;
    }

    public void ResetSpeed()
    {
        Defaultforce = currentSpeed;
        forwardforce = 500f;
        rightforce = 500f;
        leftforce = 500f;
        backforce = 500f;
    }
}