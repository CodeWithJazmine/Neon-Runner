using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class OptionsMenu : MonoBehaviour
{
    public TMP_Dropdown qualityDropdown;

    private void Start()
    {
        qualityDropdown.value = QualitySettings.GetQualityLevel();
    }
    public void SetMusicVolume(float volume)
    {
        GameManager.instance.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        GameManager.instance.SetSFXVolume(volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void Back()
    {
        GameManager.instance.activeCanvas.SetActive(false);
        if (GameManager.instance.activeCanvas != null)
        {
            GameManager.instance.activeCanvas = GameManager.instance.previousCanvas;
            GameManager.instance.activeCanvas.SetActive(true);
        }
        else
        {
            GameManager.instance.activeCanvas = null;
            GameManager.instance.Unpaused();
        }
    }

}
