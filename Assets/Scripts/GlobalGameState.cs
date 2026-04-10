using UnityEngine;
using System.Collections.Generic;

public static class GlobalGameState
{
    public static string playerToken = "";
    public static string teamId = "";
    public static HashSet<string> completedGames = new HashSet<string>();
    public static string deviceId = "";
    public static GameInteractionData activeGameData; 
    public static Vector3 playerReturnPosition;
    public static bool isReturningFromGame = false;
    public static string lastTriggeredLocation = ""; 
    public static string returnSceneName = "MainMap";

    // --- NEW: Store the ID of the location we just scanned ---
    public static string currentScannedLocation = ""; 
}