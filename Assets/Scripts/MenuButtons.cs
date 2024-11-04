using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    public void Resume()
    {
        GameManager.instance.activeCanvas.SetActive(false);
        GameManager.instance.Unpaused();
        GameManager.instance.inGameMusic.UnPause();
    }

    public void Restart()
    {
        GameManager.instance.RestartGame();
    }
    public void Options()
    {
        GameManager.instance.OptionsMenu();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
