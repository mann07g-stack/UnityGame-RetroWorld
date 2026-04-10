using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro;
using System.Collections;

public class WinSceneController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerNameText;
    
    // Assign your big background Image here
    public Image backgroundDisplay; 

    private bool isTyping = false;
    private string fullText = "";

    private void Start()
    {
        // 1. CLEAR OLD IMAGE FIRST
        // This ensures we never see the "default" inspector image by mistake.
        if (backgroundDisplay != null)
        {
            backgroundDisplay.sprite = null; 
            backgroundDisplay.gameObject.SetActive(false); // Hide until we confirm data
        }

        // 2. GET DATA
        GameInteractionData data = GlobalGameState.activeGameData;
        string winText = "Protocol ended. Returning to Hub.";

        if (data != null)
        {
            Debug.Log($"📂 Loaded Data File: {data.gameName}");

            // 3. FORCE UPDATE PHOTO
            if (backgroundDisplay != null)
            {
                if (data.backgroundPhoto != null)
                {
                    Debug.Log($"✅ Found Photo: {data.backgroundPhoto.name}. Applying now.");
                    backgroundDisplay.sprite = data.backgroundPhoto;
                    backgroundDisplay.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError($"❌ ERROR: The .asset file for '{data.gameName}' has NO PHOTO assigned!");
                    // Optional: keep it black or show a default "No Signal" image
                }
            }

            // 4. SET TEXT
            if (!GlobalGameState.completedGames.Contains(data.gameName))
            {
                if (data.winSentences.Length > 0) winText = data.winSentences[0];
                GlobalGameState.completedGames.Add(data.gameName);
            }
            else
            {
                winText = "Protocol already completed. Access restricted.";
            }
        }
        else
        {
            Debug.LogError("🚨 CRITICAL: No Active Game Data found! (Did you start from the Map?)");
        }

        if(dialoguePanel) dialoguePanel.SetActive(true);
        if(speakerNameText) speakerNameText.text = "System";
        
        StartCoroutine(TypeRoutine(winText));
    }

    IEnumerator TypeRoutine(string text)
    {
        isTyping = true;
        fullText = text;
        dialogueText.text = "";
        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.04f);
        }
        isTyping = false;
    }

    public void OnClick()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = fullText;
            isTyping = false;
        }
        else
        {
            GlobalGameState.activeGameData = null;
            GlobalGameState.isReturningFromGame = false;
            SceneManager.LoadScene("MainMap");
        }
    }
}