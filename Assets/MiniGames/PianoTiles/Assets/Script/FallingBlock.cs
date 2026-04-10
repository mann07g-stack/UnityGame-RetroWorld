//Fallingblock.cs
using UnityEngine;

public class FallingBlock : MonoBehaviour
{
    [HideInInspector] public float speed; 
    public int laneIndex;
    public PianoGameScript manager;
    private RectTransform rt;

    void Awake() => rt = GetComponent<RectTransform>();

    void Update()
    {
        if (manager == null) return;

        rt.anchoredPosition += Vector2.down * speed * Time.deltaTime;

        // If the block moves significantly past the target ref, it's a miss
        // -700 is a safe guess; adjust if your laneRef is lower on screen
        if (rt.anchoredPosition.y < -1000f)
        {
            manager.HandleMiss(this);
            Destroy(gameObject);
        }
    }
}