// using UnityEngine;
// using TMPro;

// public class FinalScoreDisplay : MonoBehaviour
// {
//     public TMP_Text collisionText;

//     public void Start()
//     {
//         int collisions = GameStatsManager.Instance.totalCollisions;
//         GameTimer.Instance.StopTimer();
//         float time = GameTimer.Instance.GetElapsedTime();

//         collisionText.text = $"Total Collisions: {collisions}\nTime: {time:F2} sec";

//         string playerName = PlayerDataManager.Instance.currentPlayerName;
//         Debug.Log($"✅ Displaying results for {playerName} - Collisions: {collisions}, Time: {time}");
//     }
// }
