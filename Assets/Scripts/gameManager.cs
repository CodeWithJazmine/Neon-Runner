using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Variables")]
    private bool gameIsOver = false;

    [Header("UI Objects")]
    public GameObject LoseOverlay;
    public GameObject WinOverlay;
    public GameObject ObjectiveUI;
    public TextMeshProUGUI DronesOverridenText;
    public TextMeshProUGUI DronesHackedText;
    public TextMeshProUGUI DronesToBeHackedText;

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

        DronesOverridenText.gameObject.SetActive(false);
        DronesHackedText.text = hackedEnemyDrones.ToString();
        DronesToBeHackedText.text = " / " + totalEnemyDrones.ToString();
        DronesHackedText.gameObject.SetActive(false);
        DronesToBeHackedText.gameObject.SetActive(false);


        StartCoroutine(ShowObjective());
    }

    private IEnumerator ShowObjective()
    {
        ObjectiveUI.SetActive(true);
        yield return new WaitForSeconds(3);
        ObjectiveUI.SetActive(false);

        DronesOverridenText.gameObject.SetActive(true);
        DronesHackedText.gameObject.SetActive(true);
        DronesToBeHackedText.gameObject.SetActive(true);
    }
    private void DroneHacked()
    {
        hackedEnemyDrones++;
        DronesHackedText.text = hackedEnemyDrones.ToString();
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
