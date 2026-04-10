using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileScript : MonoBehaviour
{
    private BattleshipGameManager gameManager;
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<BattleshipGameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        gameManager.CheckHit(collision.gameObject);
        Destroy(gameObject);
    }
}
