using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // 
    private bool playerDetected = false;

    private Coroutine detectedOverlayCoroutine;
    private Coroutine detectingPlayerCoroutine;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered enemy sights");
            GameManager.instance.playerSeen = true;

            if (detectedOverlayCoroutine == null)
            {
                detectedOverlayCoroutine = StartCoroutine(GameManager.instance.DetectedOverlayFlash());
            }

            if (detectingPlayerCoroutine == null)
            {
                detectingPlayerCoroutine = StartCoroutine(DetectingPlayer(3.0f));
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left enemy sights");
            GameManager.instance.playerSeen = false;

            if (detectedOverlayCoroutine != null)
            {
                StopCoroutine(detectedOverlayCoroutine);
                detectedOverlayCoroutine = null;
                GameManager.instance.DetectedOverlay.SetActive(false); // Make sure the overlay is hidden if it was active
            }

            if (detectingPlayerCoroutine != null)
            {
                StopCoroutine(detectingPlayerCoroutine);
                detectingPlayerCoroutine = null;
            }
        }
    }

    IEnumerator DetectingPlayer(float countdown) {

        yield return new WaitForSeconds(countdown);
        playerDetected = true;
        GameManager.instance.YouLose();
    }

}
