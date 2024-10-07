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
