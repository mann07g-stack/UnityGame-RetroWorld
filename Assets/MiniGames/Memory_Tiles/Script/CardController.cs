using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement; // Required for Scene Loading

public class CardController : MonoBehaviour
{
    [Header("Game Configuration")]
    [SerializeField] Sprite[] sprites;
    [SerializeField] Transform gridTransform;
    [SerializeField] Card cardPrefab;

    [System.Serializable]
    public class Round
    {
        public int pairCount;   // number of pairs this round
        public string riddle;   // the riddle for this round
        public string answer;   // the correct answer
    }

    [SerializeField] Round[] rounds; 
    
    [Header("Riddle UI")]
    [SerializeField] GameObject riddlePanel; 
    [SerializeField] TMPro.TextMeshProUGUI riddleText;
    [SerializeField] TMP_InputField answerInput;

    [Header("New UI Elements")]
    [SerializeField] TMPro.TextMeshProUGUI timerText;   // Drag your Timer Text here
    [SerializeField] GameObject infoPanel;              // Drag your Info Panel here
    [SerializeField] Button infoButton;                 // Drag 'i' button
    [SerializeField] Button closeInfoButton;            // Drag 'X' button inside panel

    [Header("Backend & Scene")]
    [SerializeField] GameObject loadingSpinner;         // Drag Loading Spinner here
    [SerializeField] string winSceneName = "Test_NPC";  // Return to NPC scene

    // State Variables
    int currentRound = 0;
    int remainingPairs;
    private List<Sprite> spritePairs;
    bool isChecking = false;
    bool isGamePaused = false; 
    bool isTimerRunning = false;
    float timeElapsed = 0f;

    Card firstSelected;
    Card secondSelected;

    private void Start()
    {
        // Setup Buttons
        if (infoButton != null) 
            infoButton.onClick.AddListener(OpenInfo);
        if (closeInfoButton != null) 
            closeInfoButton.onClick.AddListener(CloseInfo);

        // Hide Panels
        if (infoPanel != null) infoPanel.SetActive(false);
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        if (riddlePanel != null) riddlePanel.SetActive(false);

        StartRound();
    }

    private void Update()
    {
        // Timer Logic
        if (isTimerRunning && !isGamePaused)
        {
            timeElapsed += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeElapsed / 60F);
            int seconds = Mathf.FloorToInt(timeElapsed % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // --- INFO PANEL LOGIC ---
    public void OpenInfo()
    {
        isGamePaused = true;
        isTimerRunning = false; // Pause Timer
        if (infoPanel) infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        isGamePaused = false;
        isTimerRunning = true; // Resume Timer
        if (infoPanel) infoPanel.SetActive(false);
    }
    // ------------------------

    void StartRound()
    {
        ClearGrid(); 

        if (currentRound < rounds.Length)
        {
            Round round = rounds[currentRound];
            remainingPairs = round.pairCount;
            PrepareSprites(round.pairCount);
            CreateCards();
            
            riddlePanel.SetActive(false);
            isTimerRunning = true; // Start Timer
        }
    }

    private void PrepareSprites(int pairCount)
    {
        spritePairs = new List<Sprite>();
        for (int i = 0; i < pairCount; i++)
        {
            // Ensure we don't go out of bounds if not enough sprites
            int spriteIndex = i % sprites.Length; 
            spritePairs.Add(sprites[spriteIndex]);
            spritePairs.Add(sprites[spriteIndex]);
        }
        ShuffleSprites(spritePairs);
    }

    public void SetSelected(Card card)
    {
        // Stop interaction if checking, paused, or card already done
        if (isChecking || card.isSelected || isGamePaused) return;

        card.Show();

        if (firstSelected == null)
        {
            firstSelected = card;
            return;
        }

        if (firstSelected == card) return;

        secondSelected = card;
        StartCoroutine(CheckMatching(firstSelected, secondSelected));
    }

    IEnumerator CheckMatching(Card a, Card b)
    {
        isChecking = true;
        yield return new WaitForSeconds(0.5f); // Increased slightly for better feel

        if (a.iconSprite == b.iconSprite)
        {
            // Match Found
            a.gameObject.SetActive(false);
            b.gameObject.SetActive(false);

            remainingPairs--;
            if (remainingPairs == 0)
            {
                ShowRiddle();
            }
        }
        else
        {
            // No Match
            a.Hide();
            b.Hide();
        }

        firstSelected = null;
        secondSelected = null;
        isChecking = false;
    }

    void ShowRiddle()
    {
        isTimerRunning = false; // Pause timer while solving riddle
        riddlePanel.SetActive(true);
        riddleText.text = rounds[currentRound].riddle;
        answerInput.text = "";
    }

    public void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);
            card.controller = this;
        }
    }

    void ShuffleSprites(List<Sprite> spriteList)
    {
        for (int i = spriteList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Sprite temp = spriteList[i];
            spriteList[i] = spriteList[randomIndex];
            spriteList[randomIndex] = temp;
        }
    }

    void ClearGrid()
    {
        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SubmitAnswer()
    {
        string input = answerInput.text.ToLower().Trim();
        string correct = rounds[currentRound].answer.ToLower();

        if (input == correct)
        {
            currentRound++;

            if (currentRound < rounds.Length)
            {
                StartRound(); 
            }
            else
            {
                // ALL ROUNDS COMPLETE
                Debug.Log("GAME COMPLETED 🎉");
                riddlePanel.SetActive(false);
                StartCoroutine(WinSequence());
            }
        }
        else
        {
            answerInput.text = "";
            answerInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Wrong! Try again...";
        }
    }

    // --- WIN SEQUENCE (Backend + Scene Load) ---
    IEnumerator WinSequence()
    {
        isTimerRunning = false;
        if (loadingSpinner) loadingSpinner.SetActive(true);

        // 1. Call Backend
        bool apiSuccess = false;
        if (APIManager.Instance != null)
        {
            APIManager.Instance.UnlockNextLevel((success) => 
            {
                apiSuccess = success;
                if(success) Debug.Log("✅ Backend Confirmed!");
                else Debug.LogError("❌ Backend Failed.");
            });
        }
        else
        {
            apiSuccess = true; // Fallback
        }

        // 2. Wait for API
        float timeout = 5f;
        while (timeout > 0 && (loadingSpinner != null && loadingSpinner.activeSelf))
        {
            if (APIManager.Instance == null || apiSuccess) break;
            timeout -= Time.deltaTime;
            yield return null;
        }

        // 3. Set Global Flags
        GlobalGameState.isReturningFromGame = true;
        if (GlobalGameState.activeGameData != null)
        {
            GlobalGameState.completedGames.Add(GlobalGameState.activeGameData.gameName);
        }

        yield return new WaitForSeconds(1.0f);

        // 4. Load Scene
        SceneManager.LoadScene(winSceneName);
    }
}