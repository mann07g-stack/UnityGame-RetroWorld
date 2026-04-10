using UnityEngine;
using TMPro;
using System.Collections;

public class LevelStartCountdown : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public GameObject player;

    private bool hasStarted = false;

    void Start()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            StartCoroutine(CountdownRoutine());
        }
    }

    IEnumerator CountdownRoutine()
    {
        hasStarted = true;

        // Disable player movement
        PlayerMovementSingle movement = player.GetComponent<PlayerMovementSingle>();
        if (movement != null)
            movement.enabled = false;
            

        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        // Countdown values and colors
        string[] countdownValues = { "3", "2", "1", "Go!" };
        Color[] countdownColors = {
            Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green
        };

        for (int i = 0; i < countdownValues.Length; i++)
        {
            countdownText.text = countdownValues[i];
            countdownText.color = countdownColors[i];
            countdownText.transform.localScale = Vector3.one * 2f;
            countdownText.alpha = 1f;
            countdownText.gameObject.SetActive(true);

            float duration = 0.5f;
            float elapsed = 0f;

            // Animate scale and alpha
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                countdownText.transform.localScale = Vector3.Lerp(Vector3.one * 2f, Vector3.one, t);
                countdownText.alpha = Mathf.Lerp(1f, 0.3f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            countdownText.alpha = 0f;
            yield return new WaitForSeconds(0.5f);
        }

        countdownText.gameObject.SetActive(false);

        // Re-enable player movement
        if (movement != null)
            movement.enabled = true;

        // Start timer if available
        if (GameTimer.Instance != null)
            GameTimer.Instance.StartTimer();
    }
}
