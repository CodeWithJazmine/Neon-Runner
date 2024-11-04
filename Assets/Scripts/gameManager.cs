using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;
using Unity.VisualScripting;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Variables")]
    private bool gameIsOver = false;

    [Header("UI Objects")]
    public GameObject UIObject;
    public GameObject LoseOverlay;
    public GameObject WinOverlay;
    public GameObject ObjectiveUI;
    public TextMeshProUGUI OverrideDroneText;
    public TextMeshProUGUI DronesOverridenText;
    public TextMeshProUGUI DronesHackedText;
    public TextMeshProUGUI DronesToBeHackedText;

    [Header("Game Objectives")]
    public int hackedEnemyDrones = 0;
    public int totalEnemyDrones = 0;

    [Header("UI Menus")]
    public GameObject activeMenu;
    public GameObject activeCanvas, previousCanvas;
    public GameObject pauseMenuCanvas, optionsMenuCanvas, winMenuCanvas, loseMenuCanvas;
    public AudioSource inGameMusic;
    public AudioMixer audioMixer;
    private bool isPaused;
    private float musicVolume = 0;
    private float sfxVolume = 0;

    private void Awake()
    {
        // Ensure the GameManager is a singleton
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate GameManager
        }
    }

    private void Start()
    {
        pauseMenuCanvas.SetActive(false);
        optionsMenuCanvas.SetActive(false);
        loseMenuCanvas.SetActive(false);

        InitializeGame();

    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reinitialize the game every time the scene is loaded
        InitializeGame();
    }

    private void Update()
    {
        if (gameIsOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }

        // Open pause menu
        if ((Input.GetButtonDown("Cancel") || (Input.GetKeyDown(KeyCode.P))) && activeMenu == null && activeCanvas == null)
        {
            pauseMenuCanvas.SetActive(true);
            activeCanvas = pauseMenuCanvas;
            previousCanvas = activeCanvas;
            Paused();
        }
        // Close pause menu
        else if ((Input.GetButtonDown("Cancel") || (Input.GetKeyDown(KeyCode.P))) && activeCanvas == pauseMenuCanvas)
        {
            pauseMenuCanvas.SetActive(false);
            activeCanvas = null;
            Unpaused();
        }
        // Close options menu
        else if ((Input.GetButtonDown("Cancel") || (Input.GetKeyDown(KeyCode.P))) && activeCanvas == optionsMenuCanvas)
        {
            optionsMenuCanvas.SetActive(false);
            activeCanvas = previousCanvas;
            activeCanvas.SetActive(true);
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
                drone.OnDroneHacked -= DroneHacked;
                drone.OnDroneHacked += DroneHacked;
            }
        }

        LoseOverlay.SetActive(false);
        WinOverlay.SetActive(false);

        OverrideDroneText.gameObject.SetActive(false);
        DronesHackedText.text = hackedEnemyDrones.ToString();
        DronesToBeHackedText.text = " / " + totalEnemyDrones.ToString();
        DronesOverridenText.gameObject.SetActive(false);
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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        inGameMusic.UnPause();
        audioMixer.SetFloat("musicVolume", musicVolume);
        audioMixer.SetFloat("sfxVolume", sfxVolume);
        Time.timeScale = 1;
    }

    public void Paused()
    {
        //pauses game
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        // pause IN GAME sound effects and IN GAME music
        if (activeCanvas != null)
        {
            inGameMusic.Pause();
        }

        isPaused = !isPaused;
    }

    public void Unpaused()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // unpause IN GAME sound effects and IN GAME music
        if (activeCanvas == null)
        {
            inGameMusic.UnPause();
            audioMixer.SetFloat("sfxVolume", sfxVolume);
            audioMixer.SetFloat("musicVolume", musicVolume);
        }

        isPaused = !isPaused;
        if (activeMenu != null)
        {
            activeMenu.SetActive(false);
        }

        activeMenu = null;
        activeCanvas = null;
        previousCanvas = null;
    }

    public void OptionsMenu()
    {
        previousCanvas = activeCanvas;

        optionsMenuCanvas.SetActive(true);
        activeCanvas = optionsMenuCanvas;

        if (previousCanvas != null)
        {
            previousCanvas.SetActive(false);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        audioMixer.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        audioMixer.SetFloat("sfxVolume", volume);
    }

}