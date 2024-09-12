using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    //private Coroutine detectedOverlayCoroutine;
    private Coroutine detectingPlayerCoroutine;
    private Coroutine flashingLightCoroutine;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered enemy sights");
            GameManager.instance.playerSeen = true;

            if (flashingLightCoroutine == null)
            {
                flashingLightCoroutine = StartCoroutine(FlashingLight());
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

            if (flashingLightCoroutine != null)
            {
                StopCoroutine(flashingLightCoroutine);
                flashingLightCoroutine = null;
                this.GetComponentInChildren<Light>().intensity = 4.0f; // Make sure the light is off if it was on
            }

            if (detectingPlayerCoroutine != null)
            {
                StopCoroutine(detectingPlayerCoroutine);
                detectingPlayerCoroutine = null;
            }
        }
    }

    IEnumerator FlashingLight()
    {
        while (true)
        { 
            this.GetComponentInChildren<Light>().intensity = 20.0f;
            yield return new WaitForSeconds(0.5f);
            this.GetComponentInChildren<Light>().intensity = 4.0f;
            yield return new WaitForSeconds(0.5f);
        }    
    }
    IEnumerator DetectingPlayer(float countdown) {

        yield return new WaitForSeconds(countdown);
        GameManager.instance.YouLose();
    }

}
