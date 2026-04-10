using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerAccountHandler : MonoBehaviour
{
    [Header("New Player Fields")]
    public TMP_InputField nameInputField;
    public TMP_InputField appNoInputField;
    public GameObject registerButton;
    public TextMeshProUGUI duplicateNameWarning;
    public TextMeshProUGUI duplicateAppNumberWarning;

    public void OnRegisterButtonClick()
    {
        string playerName = nameInputField.text.Trim();
        string appNo = appNoInputField.text.Trim();

        // Hide warnings initially
        duplicateAppNumberWarning.gameObject.SetActive(false);
        duplicateNameWarning.gameObject.SetActive(false);

        // Empty field check (optional but recommended)
        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(appNo))
        {
            Debug.LogWarning("Name or Application Number is empty.");
            return;
        }

        // Check for duplicates
        bool nameTaken = PlayerDataManager.Instance.IsNameTaken(playerName);
        bool appTaken = PlayerDataManager.Instance.IsAppNumberTaken(appNo);

        // Show warnings
        if (nameTaken) duplicateNameWarning.gameObject.SetActive(true);
        if (appTaken) duplicateAppNumberWarning.gameObject.SetActive(true);

        if (nameTaken || appTaken)
        {
            Debug.LogWarning("Duplicate Name or Application Number.");
            return;
        }

        // Save player info
        PlayerDataManager.Instance.SetCurrentPlayer(playerName, appNo);
        GameStatsManager.Instance.totalCollisions = 0;

        // Load the first level
        SceneManager.LoadScene("LEVEL01");
    }
}
