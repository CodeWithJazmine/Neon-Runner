using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Variables")]
    private bool gameIsOver = false;

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

    private void Start()
    {
        InitializeGame();
    }
    private void Update()
    {
        if (gameIsOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    private void InitializeGame()
    {
        hackedEnemyDrones = 0;
        gameIsOver = false;

        totalEnemyDrones = GameObject.FindGameObjectsWithTag("EnemyDrone").Length;
        EnemyDrone[] enemyDrones = FindObjectsOfType<EnemyDrone>();
        foreach (EnemyDrone drone in enemyDrones)
        {
            if (drone != null)
            {
                drone.OnDroneHacked += DroneHacked;
            }
        }

        LoseOverlay.SetActive(false);
        WinOverlay.SetActive(false);
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
        gameIsOver = true;
    }

    public void YouWin()
    {
        WinOverlay.SetActive(true);

        StopAllCoroutines();
        Time.timeScale = 0;
        gameIsOver = true;
       
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }
}
