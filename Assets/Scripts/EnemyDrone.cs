using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyDrone : MonoBehaviour
{
    private Coroutine detectingPlayerCoroutine;
    private Coroutine flashingConeCoroutine;
    [SerializeField] private FieldOfViewDetector fieldOfView;
    [SerializeField] private HackingModule hackingModule;
    [SerializeField] private GameObject pressE;
    private bool canPressE;
    private bool isAudioOverrideActive;
    //Mesh renderer colors
    private Color originalColor = new(0.0f, 0.0f, 0.0f, 0.0f);
    private Color detectedColor = new(0.5f, 0.0f, 0.0f, 0.5f);

    [Header("Drone Variables")]
    [SerializeField] private float detectionTime = 3.0f;
    [SerializeField] private float oscillationSpeed = 0.35f;
    private Vector3 oscillationStart;
    private Vector3 oscillationEnd;

    private GameObject player;
    private Transform playerTransform;
    private Vector3 directionToPlayer;
    private float angleToPlayer;
    private bool playerDetected = false;
    private float mainFOVAngle;
    private float detectionRadius;
    private float distanceToPlayer;

    void Start()
    {
        // Get player object and transform
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        //Get the FieldOfViewDetector component and subscribe to its events
       fieldOfView = GetComponentInChildren<FieldOfViewDetector>();
        if(fieldOfView != null)
        {
            mainFOVAngle = fieldOfView.viewAngle;
            detectionRadius = fieldOfView.viewDistance;
        }

        // Set the detection radius of the sphere collider
        transform.GetComponent<SphereCollider>().radius = detectionRadius;
        transform.GetComponent<SphereCollider>().isTrigger = true;

        oscillationStart = transform.eulerAngles + new Vector3(0f, fieldOfView.viewAngle / 2.0f, 0f);
        oscillationEnd = transform.eulerAngles + new Vector3(0f, -(fieldOfView.viewAngle / 2.0f), 0f);

        // subscribe to the events
        hackingModule = GetComponentInChildren<HackingModule>();
        if (hackingModule != null)
        {
            hackingModule.PlayerInRange += CanPressE;
        }

    }

    void Update()
    {
        Oscillate();

        if(canPressE && Input.GetKeyDown(KeyCode.E))
        {
            AudioOverride();
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (isAudioOverrideActive)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            CheckPlayerVisibility();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopDetectingPlayer();
        }
    }

    private void Oscillate()
    {
        float oscillation = Mathf.PingPong(Time.time * oscillationSpeed, 1.0f);
        transform.eulerAngles = Vector3.Lerp(oscillationStart, oscillationEnd, oscillation);
    }

    private void CheckPlayerVisibility()
    {
        if (isAudioOverrideActive) 
        {
            return;
        }
        // Calculate the direction, distance, and angle to the player
        directionToPlayer = (playerTransform.position - transform.position);
        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Reset player detection
        playerDetected = false;

        // Check if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            // If player is in line of sight
            RaycastHit hit;

            Debug.DrawRay(transform.position, directionToPlayer, Color.red);

            if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRadius))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    // If player is within the main field of view
                    if (angleToPlayer <= mainFOVAngle / 2.0f)
                    {
                        DetectPlayer();
                        playerDetected = true;
                    }
                }
            }
        }
        if(!playerDetected)
        {
            StopDetectingPlayer();
        }
    }


    // Implementing the IChasePlayer interface
    public void DetectPlayer()
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

    public void StopDetectingPlayer()
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

    private void CanPressE()
    {
        canPressE = true;
    }

    private void AudioOverride()
    {
        Debug.Log("Audio Override!");
        pressE.SetActive(false);
        canPressE = false;
        isAudioOverrideActive = true;
        hackingModule.GetComponent<BoxCollider>().enabled = false;

        // Color the drone green
        GetComponent<MeshRenderer>().materials[0].color = new Color(0.0f, 0.5f, 0.0f, 0.5f);

        // Color the field of view green
        fieldOfView.GetComponent<MeshRenderer>().materials[0].color = new Color(0.0f, 0.5f, 0.0f, 0.5f);

        // Color the light green
        GetComponentInChildren<Light>().color = new Color(0.0f, 0.5f, 0.0f, 0.5f);

    }

}