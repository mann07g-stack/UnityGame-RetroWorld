using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class MastermindManager : MonoBehaviour
{
    [Header("Game Data")]
    public Color[] availableColors; 
    public int maxTurns = 7;
    public int codeLength = 5;
    
    private Color[] secretCode;
    private Color[] currentGuess;
    private int currentTurn = 0; 
    private int activeSlotIndex = 0;

    [Header("UI Connections")]
    public GameObject[] guessRows; // Element 0 = Bottom Row
    public Image[] answerSlots;    
    
    [Tooltip("Drag the specific CHILD object that hides the answer dots here. Do NOT drag the whole board.")]
    public GameObject secretCodeCover;

    public GameObject palettePanel;
    public TextMeshProUGUI statusText;
    public GameObject loadingSpinner; 

    [Header("Audio")]
    public AudioSource musicSource;       
    public AudioSource sfxSource;         
    public AudioClip backgroundMusic;     
    public AudioClip winSound;            
    public AudioClip loseSound;           
    public AudioClip clickSound;          

    [Header("Scene Settings")]
    public string winSceneName = "Test_NPC";

    private bool gameEnded = false;

    void Start()
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        if (palettePanel != null) palettePanel.SetActive(false);

        // ✅ FIX: Reset status on load
        if (statusText != null)
            statusText.text = "Guess the code!";

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        StartNewGame();
    }

    void StartNewGame()
    {
        gameEnded = false;
        secretCode = new Color[codeLength];
        currentGuess = new Color[codeLength];
        
        for(int i = 0; i < codeLength; i++) currentGuess[i] = new Color(0,0,0,0);

        currentTurn = 0;
        
        GenerateSecretCode();
        ResetVisuals();
        UpdateInteractivity();
        
        // ✅ FIX: Reset status on new game
        if(statusText) statusText.text = "Guess the code!";
    }

    void GenerateSecretCode()
    {
        for (int i = 0; i < codeLength; i++)
        {
            secretCode[i] = availableColors[Random.Range(0, availableColors.Length)];
            
            if (answerSlots.Length > i)
            {
                answerSlots[i].color = secretCode[i];
                Color c = answerSlots[i].color;
                c.a = 1f;
                answerSlots[i].color = c;
            }
        }
    }

    public void SetActiveSlot(int index)
    {
        if (gameEnded) return;
        activeSlotIndex = index;
        if(palettePanel) palettePanel.SetActive(true);
    }

    public void SelectColor(int colorIndex)
    {
        if (gameEnded || colorIndex < 0 || colorIndex >= availableColors.Length) return;

        Color picked = availableColors[colorIndex];
        picked.a = 1f; 
        currentGuess[activeSlotIndex] = picked;
        
        Button[] rowButtons = guessRows[currentTurn].GetComponentsInChildren<Button>();
        if (rowButtons.Length > activeSlotIndex)
        {
            rowButtons[activeSlotIndex].GetComponent<Image>().color = picked;
        }

        if(palettePanel) palettePanel.SetActive(false);

        if (sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
    }

    public void CheckGuess()
    {
        if (gameEnded) return;

        for (int i = 0; i < codeLength; i++)
        {
            if (currentGuess[i].a < 0.1f) 
            { 
                if(statusText) statusText.text = "Fill all slots!"; 
                return; 
            }
        }

        int blackPegs = 0;
        int whitePegs = 0;

        List<Color> tempSecret = new List<Color>(secretCode);
        List<Color> tempGuess = new List<Color>(currentGuess);

        for (int i = codeLength - 1; i >= 0; i--)
        {
            if (tempGuess[i] == tempSecret[i])
            {
                blackPegs++;
                tempSecret.RemoveAt(i);
                tempGuess.RemoveAt(i);
            }
        }

        foreach (Color c in tempGuess)
        {
            if (tempSecret.Contains(c))
            {
                whitePegs++;
                tempSecret.Remove(c);
            }
        }

        UpdatePegs(blackPegs, whitePegs);

        if (blackPegs == codeLength)
        {
            WinGame();
        }
        else
        {
            currentTurn++;
            if (currentTurn >= maxTurns)
            {
                LoseGame();
            }
            else
            {
                currentGuess = new Color[codeLength];
                for(int i = 0; i < codeLength; i++) currentGuess[i] = new Color(0,0,0,0);
                UpdateInteractivity();
                if(statusText) statusText.text = "Try Again...";
            }
        }
    }

    void WinGame()
    {
        gameEnded = true;
        if(statusText) statusText.text = "YOU WON!";
        
        if(secretCodeCover != null) 
        {
            Animator coverAnim = secretCodeCover.GetComponent<Animator>();
            if (coverAnim != null && coverAnim.runtimeAnimatorController != null)
            {
                coverAnim.SetTrigger("Reveal");
            }
            else
            {
                StartCoroutine(AnimateCoverSlide());
            }
        }

        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null && winSound != null) sfxSource.PlayOneShot(winSound);

        StartCoroutine(WinSequence());
    }

    IEnumerator AnimateCoverSlide()
    {
        RectTransform rt = secretCodeCover.GetComponent<RectTransform>();
        if (rt == null) 
        {
            secretCodeCover.SetActive(false);
            yield break;
        }

        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos - new Vector2(0, 100f); 
        
        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; 
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        rt.anchoredPosition = endPos;
    }

    void LoseGame()
    {
        gameEnded = true;

        // ✅ FIX: Show Try Again instead of Game Over
        if(statusText) statusText.text = "Try Again...";

        if(secretCodeCover != null) secretCodeCover.SetActive(false);

        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null && loseSound != null) sfxSource.PlayOneShot(loseSound);

        StartCoroutine(AutoRestart());
    }

    IEnumerator WinSequence()
    {
        yield return new WaitForSecondsRealtime(5.0f);

        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        bool apiSuccess = false;

        if (APIManager.Instance != null)
        {
            APIManager.Instance.UnlockNextLevel((success) => 
            {
                apiSuccess = success;
            });
        }
        else
        {
            apiSuccess = true; 
        }

        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        GlobalGameState.isReturningFromGame = true;

        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        Time.timeScale = 1f; 
        SceneManager.LoadScene(winSceneName);
    }

    IEnumerator AutoRestart()
    {
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateInteractivity()
    {
        for (int i = 0; i < guessRows.Length; i++)
        {
            CanvasGroup cg = guessRows[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = guessRows[i].AddComponent<CanvasGroup>();
            
            bool isCurrent = (i == currentTurn);
            cg.interactable = isCurrent;
            cg.blocksRaycasts = isCurrent;
            cg.alpha = isCurrent ? 1f : 0.4f; 
        }
    }

    void UpdatePegs(int b, int w)
    {
        if (currentTurn >= guessRows.Length) return;

        Transform pegArea = guessRows[currentTurn].transform.Find("PegArea");
        if (pegArea == null) return;

        Image[] pegs = pegArea.GetComponentsInChildren<Image>();

        for (int i = 0; i < pegs.Length; i++)
        {
            if (i < b) pegs[i].color = Color.red;        
            else if (i < b + w) pegs[i].color = Color.white;   
            else pegs[i].color = new Color(0.2f, 0.2f, 0.2f, 1f); 
        }
    }

    void ResetVisuals()
    {
        if(secretCodeCover != null) 
        {
            secretCodeCover.SetActive(true);
        }

        foreach (GameObject row in guessRows)
        {
            foreach (Button b in row.GetComponentsInChildren<Button>())
            {
                b.GetComponent<Image>().color = Color.white;
            }

            Transform pg = row.transform.Find("PegArea");
            if (pg != null)
            {
                foreach (Transform child in pg)
                {
                    Image p = child.GetComponent<Image>();
                    if (p != null) p.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                }
            }
        }
    }
}