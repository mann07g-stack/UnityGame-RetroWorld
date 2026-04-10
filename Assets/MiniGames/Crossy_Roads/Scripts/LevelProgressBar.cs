using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelProgressBar : MonoBehaviour
{
    [Header("Game Objects")]
    public Transform player;       
    public Transform startPoint;   
    public Transform finishWall;   

    [Header("UI Components")]
    public Slider slider;          
    public TextMeshProUGUI percentText; 
    
    [Header("Styling")]
    public RectTransform playerFaceIcon; 
    public float smoothingSpeed = 5f;    

    private float startY;
    private float maxDistance;

    void Start()
    {
        // Safety Check: Ensure we have references
        if (startPoint != null && finishWall != null)
        {
            startY = startPoint.position.y;
            // Calculate distance, ensure it's not zero to avoid division errors
            maxDistance = Mathf.Max(finishWall.position.y - startY, 1f); 
        }

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
        }
    }

    void Update()
    {
        // 1. SAFETY: If game is paused (Info Panel Open), DO NOT UPDATE THE BAR.
        if (Time.timeScale == 0f) return;

        // 2. SAFETY: If references are missing, stop to prevent errors.
        if (player == null || startPoint == null || finishWall == null) return;

        float currentY = player.position.y;
        float currentDist = currentY - startY;
        
        // Prevent negative numbers
        if (currentDist < 0) currentDist = 0;

        // Calculate percentage
        float targetProgress = Mathf.Clamp01(currentDist / maxDistance);

        // Smooth move
        if (slider != null)
        {
            slider.value = Mathf.Lerp(slider.value, targetProgress, Time.deltaTime * smoothingSpeed);
        }

        // Update Text
        if (percentText != null)
        {
            float currentPercent = slider.value * 100f;
            percentText.text = currentPercent.ToString("F0") + "%";
        }

        // Move Icon
        if (playerFaceIcon != null && slider != null)
        {
            float sliderWidth = slider.GetComponent<RectTransform>().rect.width;
            float newX = slider.value * sliderWidth;
            playerFaceIcon.anchoredPosition = new Vector2(newX, 0);
        }
    }
}