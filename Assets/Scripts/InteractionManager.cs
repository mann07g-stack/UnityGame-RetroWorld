using System;
using System.Collections;
using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using UnityEngine.Networking; 
using UnityEngine.SceneManagement; 

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Hide UI on start
        if(dialoguePanel != null) dialoguePanel.SetActive(false);
        if(questionPanel != null) questionPanel.SetActive(false);
        if(loadingSpinner != null) loadingSpinner.SetActive(false);
        if(backgroundPanel != null) backgroundPanel.SetActive(false);
    }
    [Header("Background Visuals")]
    public Image backgroundDisplay; 
    public GameObject backgroundPanel;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText; 
    
    [Header("Answer Terminal UI")]
    public GameObject questionPanel;
    public GameObject loadingSpinner;    
    
    // REMOVED: public TextMeshProUGUI questionText; (No longer needed)
    // REMOVED: public Image questionImageDisplay;   (No longer needed)
    
    public TMP_InputField answerInput;
    public Button submitButton;
    public TextMeshProUGUI feedbackText; // Optional: To say "Correct" or "Incorrect"

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typingSound;
    public float typingSpeed = 0.05f;

    // Internal State
    private GameInteractionData currentData;
    private QuestionResponse currentApiQuestion; 
    private bool isTyping = false;
    private string currentFullSentence = "";
    private Coroutine typingCoroutine;
    private int dialogueIndex = 0;
    private bool isInteractionComplete = false;
    private bool isPostGameDialogue = false; 
    private void SetBackground(Sprite photo)
    {
        if (backgroundDisplay != null && photo != null)
        {
            backgroundDisplay.sprite = photo;
            backgroundDisplay.preserveAspect = true; // Optional: Keeps photo from stretching weirdly
            
            if(backgroundPanel != null) backgroundPanel.SetActive(true);
            else backgroundDisplay.gameObject.SetActive(true);
        }
        else if (backgroundDisplay != null)
        {
            // Fallback if no photo is provided
            // backgroundDisplay.sprite = defaultSprite; 
        }
    }
    public void StartInteraction(GameInteractionData data)
    {
        // --- PRIORITY LOCK ---
        if (isPostGameDialogue) return; 

        currentData = data;
        isInteractionComplete = false;
        SetBackground(data.backgroundPhoto);
        
        // Check if already completed
        if (GlobalGameState.completedGames.Contains(data.gameName))
        {
            dialoguePanel.SetActive(true);
            speakerNameText.text = "Dungeon Master";
            string completedText = "Protocol already active. Move along.";
            
            if(typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeSentence(completedText));
            
            isInteractionComplete = true; 
            return; 
        }

        // Start Intro
        dialoguePanel.SetActive(true);
        speakerNameText.text = "Dungeon Master"; 
        if(questionPanel) questionPanel.SetActive(false);
        
        dialogueIndex = 0;
        if(typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        string introText = (currentData.introSentences.Length > 0) ? currentData.introSentences[0] : "Enter credentials.";
        typingCoroutine = StartCoroutine(TypeSentence(introText));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        currentFullSentence = sentence;
        dialogueText.text = "";

        if(audioSource && typingSound) 
        {
            audioSource.clip = typingSound;
            audioSource.loop = true; 
            audioSource.Play();      
        }

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        if(audioSource) audioSource.Stop();
        isTyping = false;
    }

    public void OnDialogueBoxClick()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentFullSentence;
            if(audioSource) audioSource.Stop();
            isTyping = false;
        }
        else
        {
            if (isInteractionComplete)
            {
                CloseInteraction();
            }
            else
            {
                NextSentence();
            }
        }
    }

    void NextSentence()
    {
        if (currentData == null) { CloseInteraction(); return; }

        dialogueIndex++;
        if (dialogueIndex < currentData.introSentences.Length)
        {
            string nextLine = currentData.introSentences[dialogueIndex];
            if(typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeSentence(nextLine));
        }
        else
        {
            // Intro done -> Fetch Question (Silently) -> Show Input
            PrepareInputTerminal();
        }
    }

    void PrepareInputTerminal()
    {
        dialoguePanel.SetActive(false);
        if(loadingSpinner != null) loadingSpinner.SetActive(true);
        
        // We still need to CALL the API to get the Question ID, 
        // even if we don't show the text to the user.
        int gameId = currentData.apiGameId; 
        APIManager.Instance.GetQuestion(gameId, OnTerminalReady, OnError);
    }

    void OnTerminalReady(QuestionResponse data)
    {
        currentApiQuestion = data; 
        if(loadingSpinner != null) loadingSpinner.SetActive(false);
        
        // Show the Input Panel
        if(questionPanel != null) questionPanel.SetActive(true);

        // Reset Input Field
        if (answerInput != null)
        {
            answerInput.text = "";
            // Set Placeholder text to look cool
            TextMeshProUGUI placeholder = answerInput.placeholder.GetComponent<TextMeshProUGUI>();
            if(placeholder) placeholder.text = "INPUT CODE...";
            answerInput.ActivateInputField(); // Auto-focus
        }
    }

    void OnError(string error)
    {
        if(loadingSpinner != null) loadingSpinner.SetActive(false);
        dialoguePanel.SetActive(true);
        dialogueText.text = "TERMINAL ERROR: " + error;
        isInteractionComplete = true; // Let them click to exit
    }

    public void SubmitToBackend()
    {
        if (answerInput == null || APIManager.Instance == null || currentApiQuestion == null) return;

        string playerAnswer = answerInput.text;
        if (string.IsNullOrEmpty(playerAnswer)) return;

        // 1. UI: Lock Input and Show "Verifying"
        if(submitButton) submitButton.interactable = false;
        if(loadingSpinner) loadingSpinner.SetActive(true);
        
        // --- USE FEEDBACK TEXT HERE ---
        if(feedbackText) 
        {
            feedbackText.text = "> VERIFYING CREDENTIALS...";
            feedbackText.color = Color.yellow; 
        }

        // 2. Call API
        APIManager.Instance.SubmitAnswer(currentApiQuestion.questionId, playerAnswer, (isSuccess) => 
        {
            if(submitButton) submitButton.interactable = true;
            if(loadingSpinner) loadingSpinner.SetActive(false);
            
            if(isSuccess)
            {
                // --- SUCCESS FEEDBACK ---
                if(feedbackText) 
                {
                    feedbackText.text = "ACCESS GRANTED. LOADING PROTOCOL...";
                    feedbackText.color = Color.green;
                }
                
                // Wait a tiny bit so they can read "Access Granted" before scene change
                StartCoroutine(DelayAndStartGame());
            }
            else
            {
                // --- FAILURE FEEDBACK ---
                answerInput.text = ""; // Clear the wrong answer
                answerInput.ActivateInputField(); // Keep focus so they can type again immediately

                if(feedbackText) 
                {
                    feedbackText.text = "Incorrect";
                    feedbackText.color = Color.red;
                }
            }
        });
    }

    // Helper to add a small delay for dramatic effect
    System.Collections.IEnumerator DelayAndStartGame()
    {
        yield return new WaitForSeconds(1.0f); // 1 second delay
        StartMiniGame();
    }

    void StartMiniGame()
    {
        questionPanel.SetActive(false);
        GlobalGameState.activeGameData = currentData;
        
        if (loadingSpinner != null) loadingSpinner.SetActive(true);
        StartCoroutine(LoadMiniGameSceneAsync());
    }

    System.Collections.IEnumerator LoadMiniGameSceneAsync()
    {
        yield return new WaitForSeconds(0.5f);
        if (currentData != null && !string.IsNullOrEmpty(currentData.sceneName))
        {
            SceneManager.LoadScene(currentData.sceneName);
        }
    }

    public void PlayPostGameDialogue()
    {
        GameInteractionData dataToUse = GlobalGameState.activeGameData;

        if (dataToUse != null)
        {
            isPostGameDialogue = true; 
            currentData = dataToUse; 
            SetBackground(dataToUse.backgroundPhoto);

            if (loadingSpinner != null) loadingSpinner.SetActive(false);
            if (questionPanel != null) questionPanel.SetActive(false);

            dialoguePanel.SetActive(true);
            speakerNameText.text = "Dungeon Master";

            if (!GlobalGameState.completedGames.Contains(dataToUse.gameName))
            {
                GlobalGameState.completedGames.Add(dataToUse.gameName);
            }

            string winText = (dataToUse.winSentences.Length > 0) 
                             ? dataToUse.winSentences[0] 
                             : "Data secured. Returning to Hub.";

            isInteractionComplete = true; 

            if(typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeSentence(winText));
        }
    }

    void CloseInteraction()
    {
        dialoguePanel.SetActive(false);
        if(backgroundPanel != null) backgroundPanel.SetActive(false);
        else if(backgroundDisplay != null) backgroundDisplay.gameObject.SetActive(false);
        isInteractionComplete = false;

        if (isPostGameDialogue)
        {
            Debug.Log("Returning to Main Hub...");
            isPostGameDialogue = false; 
            GlobalGameState.activeGameData = null; 
            SceneManager.LoadScene("MainMap"); 
        }
    }
    public void AbortToMap()
    {
        Debug.Log("🔙 Aborting interaction. Returning to MainMap.");

        // 1. Reset Global Locks (Important!)
        // If we don't clear this, the game might think you are still standing 
        // in the trigger and refuse to let you interact again.
        GlobalGameState.lastTriggeredLocation = "";
        
        // 2. Clear current game data
        GlobalGameState.activeGameData = null;

        // 3. Load the Map
        SceneManager.LoadScene("MainMap");
    }
}