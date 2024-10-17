using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Variables")]
    //public bool playerSeen = false;

    [Header("UI Objects")]
    public GameObject LoseOverlay;
    public GameObject WinOverlay;

    [Header("Game Objectives")]
    public int hackedEnemyDrones = 0;
    public int totalEnemyDrones = 0;
    private EnemyDrone EnemyDrone;


    private void Awake() {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this);
    }

    public void Start()
    {
        totalEnemyDrones = GameObject.FindGameObjectsWithTag("EnemyDrone").Length;
        EnemyDrone[] enemyDrones = FindObjectsOfType<EnemyDrone>();
        foreach (EnemyDrone drone in enemyDrones)
        {
            if (drone != null)
            {
                drone.OnDroneHacked += DroneHacked;
            }
        }

    }

    private void DroneHacked()
    {
        hackedEnemyDrones++;
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        // Can add more win conditions here

        if (hackedEnemyDrones >= totalEnemyDrones)
        {
            YouWin();
        }
    }

    public void YouLose()
    {
        LoseOverlay.SetActive(true);
     
        StopAllCoroutines();
        Time.timeScale = 0;
    }

    public void YouWin()
    {
        WinOverlay.SetActive(true);

        StopAllCoroutines();
        Time.timeScale = 0;
    }
}
