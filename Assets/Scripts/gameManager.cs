using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Variables")]
    public bool playerSeen = false;

    [Header("UI Objects")]
    public GameObject DetectedOverlay;
    public GameObject LoseOverlay;

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

    public IEnumerator DetectedOverlayFlash() {

        while (playerSeen)
        {
            DetectedOverlay.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            DetectedOverlay.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void YouLose()
    {
        LoseOverlay.SetActive(true);
     
        StopAllCoroutines();
        Time.timeScale = 0;
    }
}
