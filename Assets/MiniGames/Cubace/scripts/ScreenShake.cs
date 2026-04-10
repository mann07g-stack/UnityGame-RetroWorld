using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance; // ✅ Singleton reference

    private Transform camTransform;
    private Vector3 originalPos;

    void Awake()
    {
        // Assign the singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        camTransform = Camera.main.transform;
        originalPos = camTransform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(DoShake(duration, magnitude));
    }

    private System.Collections.IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            camTransform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;

            yield return null;
        }

        camTransform.localPosition = originalPos; // Reset position
    }
}
