using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyDrone : MonoBehaviour, IChasePlayer
{
    private Coroutine detectingPlayerCoroutine;
    private Coroutine flashingConeCoroutine;
    private Transform playerTransform;
    private FieldOfViewDetector fieldOfView;

    [Header("Variables")]
    [SerializeField]private float detectionTime = 3.0f;

    void Start()
    {
        // Get the FieldOfViewDetector component and subscribe to its events
        fieldOfView = GetComponentInChildren<FieldOfViewDetector>();
        if (fieldOfView != null)
        {
            fieldOfView.OnPlayerEnterFOV += ChasePlayer;
            fieldOfView.OnPlayerExitFOV += StopChasingPlayer;
        }
    }

    // Implementing the IChasePlayer interface
    public void ChasePlayer()
    {
        if (flashingConeCoroutine == null)
        {
            flashingConeCoroutine = StartCoroutine(FlashingCone());
        }

        if (detectingPlayerCoroutine == null)
        {
            detectingPlayerCoroutine = StartCoroutine(DetectingPlayer(detectionTime));
        }
    }

    public void StopChasingPlayer()
    {

        if (flashingConeCoroutine != null)
        {
            StopCoroutine(flashingConeCoroutine);
            flashingConeCoroutine = null;
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = new Color(0.0f, 0.0f, 0.0f, 0.0f); // Make sure the cone returns to orignal color
        }

        if (detectingPlayerCoroutine != null)
        {
            StopCoroutine(detectingPlayerCoroutine);
            detectingPlayerCoroutine = null;
        }
    }

    // Custom Drone Coroutines
    IEnumerator FlashingCone()
    {
        while (true)
        {
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = new Color(0.5f,0.0f,0.0f,0.5f);
            yield return new WaitForSeconds(0.5f);
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator DetectingPlayer(float countdown)
    {
        yield return new WaitForSeconds(countdown);
        GameManager.instance.YouLose();
    }

}