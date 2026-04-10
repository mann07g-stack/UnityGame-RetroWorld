using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour
{
    [Header("Music Range (seconds)")]
    public float startTime = 0f;
    public float endTime = -1f; // -1 = play till end

    [Header("Fade")]
    public float fadeInDuration = 1.2f;

    AudioSource source;
    Coroutine fadeRoutine;

    void Awake()
    {
        source = GetComponent<AudioSource>();

        source.volume = 0f;
        source.time = startTime;
        source.Play();

        fadeRoutine = StartCoroutine(FadeIn());

        if (endTime > 0f)
            Invoke(nameof(StopImmediate), endTime - startTime);
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }
        source.volume = 1f;
    }

    // 🔴 USED ON DEATH
    public void StopImmediate()
    {
        GetComponent<AudioSource>().Stop();
    }


    // 🔹 USED BY INFO MENU
    public void PauseMusic()
    {
        source.Pause();
    }

    public void ResumeMusic()
    {
        source.UnPause();
    }
}
