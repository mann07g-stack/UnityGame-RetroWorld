using UnityEngine;
using UnityEngine.UI; // Required for accessing Slider and Text

public class LevelProgress : MonoBehaviour
{
    [Header("Objects")]
    public Transform player;      // Drag your Player object here
    public Transform finishLine;  // Drag your Finish Line object here

    [Header("UI")]
    public Slider progressSlider; // Drag the Slider UI here
    public Text percentageText;   // Drag the Text UI here (optional)

    float startX;
    float endX;
    float fullDistance;

    void Start()
    {
        // 1. Record the starting X position of the player
        if (player != null)
        {
            startX = player.position.x;
        }

        // 2. Record the X position of the finish line
        if (finishLine != null)
        {
            endX = finishLine.position.x;
        }

        // 3. Calculate the total length of the level
        fullDistance = endX - startX;
    }

    void Update()
    {
        if (player == null || finishLine == null) return;

        // 1. Calculate how far the player has moved from the start
        float currentDistance = player.position.x - startX;

        // 2. Convert to a 0.0 to 1.0 decimal (percentage)
        float progress = currentDistance / fullDistance;

        // 3. Clamp ensures we don't go below 0% or above 100%
        progress = Mathf.Clamp01(progress);

        // 4. Update the Slider visual
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }

        // 5. Update the Text visual (e.g. "65%")
        if (percentageText != null)
        {
            // Multiplying by 100 converts 0.65 to 65
            percentageText.text = Mathf.RoundToInt(progress * 100) + "%";
        }
    }
}