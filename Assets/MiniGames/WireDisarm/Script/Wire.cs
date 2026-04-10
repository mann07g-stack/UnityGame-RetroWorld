using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Wire : MonoBehaviour
{
    [Tooltip("0 for Top wire, 1 for Second, etc.")]
    public int wireIndex;

    private WireManager manager;
    private Button myButton;
    private TMP_Text cutText;

    void Awake()
    {
        myButton = GetComponent<Button>();
        
        // Use safer Find function
        manager = FindObjectOfType<WireManager>();

        if (transform.childCount > 0)
            cutText = transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public void OnWireClicked()
    {
        if (manager == null) 
        {
            // Try finding it again if it was null
            manager = FindFirstObjectByType<WireManager>();
            if(manager == null) return;
        }

        if (myButton != null && !myButton.interactable) return;

        manager.ProcessWireCut(wireIndex);

        if (myButton != null)
            myButton.interactable = false;

        if (cutText != null)
            cutText.text = "X";
    }
}