using UnityEngine;

[CreateAssetMenu(fileName = "NewGameData", menuName = "Game Interaction Data")]
public class GameInteractionData : ScriptableObject
{   
    public Sprite backgroundPhoto;
    public int apiGameId;
    public string gameName; // e.g., "Protocol Alpha"
    
    [TextArea(3, 10)]
    public string[] introSentences;

    // --- NEW: SCENE NAME INSTEAD OF PREFAB ---
    public string sceneName; // Type exact scene name here (e.g., "MiniGame_Flappy")
    // -----------------------------------------

    [TextArea(3, 10)]
    public string[] winSentences;
}