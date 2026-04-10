using UnityEngine;
using System.Collections;

public class CrossyPlayerController : MonoBehaviour
{
    public float gridSize = 1f;
    public float hopTime = 0.15f;
    public bool isAlive = true;
    public float wrapX = 50f;
    public float minY = 0f;

    private bool isMoving;
    private Vector3 originalScale; 

    void Start()
    {
        originalScale = transform.localScale; 
    }

    void Update()
    {
        if (!isAlive) return;

        Vector3 dir = Vector3.zero;

        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.W)) dir = Vector3.up;
            else if (Input.GetKeyDown(KeyCode.A)) dir = Vector3.left;
            else if (Input.GetKeyDown(KeyCode.D)) dir = Vector3.right;
            
            else if (Input.GetKeyDown(KeyCode.S))
            {
                if (transform.position.y - gridSize >= minY)
                {
                    dir = Vector3.down;
                }
            }

            if (dir != Vector3.zero)
                StartCoroutine(Hop(dir));
        }

        WrapPosition();
    }

    public void Die()
    {
        if (!isAlive) return;
        
        if (AudioManager.Instance != null) 
            AudioManager.Instance.PlayHitSound();
            
        isAlive = false;
        StartCoroutine(DeathRoutine());
    }

    IEnumerator Hop(Vector3 dir)
    {
        isMoving = true;
        Vector3 start = transform.position;
        Vector3 target = start + dir * gridSize;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / hopTime;
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    void WrapPosition()
    {
        Vector3 pos = transform.position;

        if (pos.x > wrapX)
            pos.x = -wrapX;
        else if (pos.x < -wrapX)
            pos.x = wrapX;

        transform.position = pos;
    }

    IEnumerator DeathRoutine()
    {
        float t = 0f;
        Vector3 currentScale = transform.localScale;

        // 1. Shrink
        while (t < 1f)
        {
            t += Time.deltaTime * 3f; 
            transform.localScale = Vector3.Lerp(currentScale, Vector3.zero, t);
            yield return null;
        }
        transform.localScale = Vector3.zero;

        // 2. Reset via Manager
        if (CrossyGameManager.Instance != null)
        {
            CrossyGameManager.Instance.ResetToStart();
        }

        // 3. Regrow
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }
        transform.localScale = originalScale;
    }
}