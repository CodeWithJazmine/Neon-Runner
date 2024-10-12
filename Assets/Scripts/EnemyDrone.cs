using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyDrone : MonoBehaviour
{
    private Coroutine detectingPlayerCoroutine;
    private Coroutine flashingConeCoroutine;
    private FieldOfViewDetector fieldOfView;

    //Mesh renderer colors
    private Color originalColor = new(0.0f, 0.0f, 0.0f, 0.0f);
    private Color detectedColor = new(0.5f, 0.0f, 0.0f, 0.5f);

    [Header("Drone Variables")]
    [SerializeField] private float detectionTime = 3.0f;
    [SerializeField] private float oscillationSpeed = 0.35f;
    private Vector3 oscillationStart;
    private Vector3 oscillationEnd;

    void Start()
    {
        // Get the FieldOfViewDetector component and subscribe to its events
        //fieldOfView = GetComponentInChildren<FieldOfViewDetector>();
        //if (fieldOfView != null)
        //{
        //    fieldOfView.OnPlayerEnterFOV += ChasePlayer;
        //    fieldOfView.OnPlayerExitFOV += StopChasingPlayer;
        //}

        oscillationStart = transform.eulerAngles + new Vector3(0f, fieldOfView.viewAngle / 2.0f, 0f);
        oscillationEnd = transform.eulerAngles + new Vector3(0f, -(fieldOfView.viewAngle / 2.0f), 0f);
    }

    void Update()
    {
        Oscillate();
    }

    private void Oscillate()
    {
        float oscillation = Mathf.PingPong(Time.time * oscillationSpeed, 1.0f);
        transform.eulerAngles = Vector3.Lerp(oscillationStart, oscillationEnd, oscillation);
    }

    // Implementing the IChasePlayer interface
    public void ChasePlayer(Collider other)
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

    public void StopChasingPlayer(Collider other)
    {

        if (flashingConeCoroutine != null)
        {
            StopCoroutine(flashingConeCoroutine);
            flashingConeCoroutine = null;
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = originalColor; // Make sure the cone returns to orignal color
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
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = detectedColor;
            yield return new WaitForSeconds(0.5f);
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator DetectingPlayer(float countdown)
    {
        yield return new WaitForSeconds(countdown);
        GameManager.instance.YouLose();
    }

}