using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoutHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button logoutButton;
    [SerializeField] private GameObject loadingSpinner; // Optional

    [Header("Scene Settings")]
    [SerializeField] private string loginSceneName = "LoginScene"; // Name of your Login Scene

    private void Start()
    {
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
        
        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }

    public void OnLogoutClicked()
    {
        if (loadingSpinner != null) loadingSpinner.SetActive(true);
        if (logoutButton != null) logoutButton.interactable = false;

        if (APIManager.Instance != null)
        {
            APIManager.Instance.Logout((success) => 
            {
                PerformLocalLogout();
            });
        }
        else
        {
            // Fallback if API Manager is missing
            PerformLocalLogout();
        }
    }

    private void PerformLocalLogout()
    {
        // 1. Clear Global State
        GlobalGameState.playerToken = "";
        GlobalGameState.teamId = "";
        GlobalGameState.completedGames.Clear();
        GlobalGameState.activeGameData = null;
        GlobalGameState.currentScannedLocation = "";
        GlobalGameState.lastTriggeredLocation = "";

        // 2. Destroy Persistent Singletons (Optional but recommended)
        // This ensures a fresh start when they log back in
        if (APIManager.Instance != null) Destroy(APIManager.Instance.gameObject);
        if (PlayerTraveler.Instance != null) Destroy(PlayerTraveler.Instance.gameObject);

        // 3. Load Login Scene
        SceneManager.LoadScene(loginSceneName);
    }
}