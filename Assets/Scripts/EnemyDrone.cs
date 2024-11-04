using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class EnemyDrone : MonoBehaviour
{

    [Header("Hacking Module")]
    public Action OnDroneHacked;
    [SerializeField] private HackingModule hackingModule;
    [SerializeField] private GameObject beaconFX;
    [SerializeField] private GameObject pressE;
    [SerializeField] private TextMeshProUGUI overridingText;
    [SerializeField] private Slider overridingSlider;
    private bool canPressE;
    private bool isAudioOverrideActive;

    [Header("Mesh Renderer Colors")]
    private Color originalColor = new(0.01568628f, 0.007843138f, 0.1529411f, 0.5f);
    private Color detectedColor = new(0.5f, 0.0f, 0.0f, 0.5f);

    [Header("Drone Variables")]
    [SerializeField] private float detectionTime = 3.0f;
    [SerializeField] private float oscillationSpeed = 0.35f;
    private Vector3 oscillationStart;
    private Vector3 oscillationEnd;
    private AudioSource moduleAudio;
    private AudioSource droneAudio;

    [Header("Player Detection")]
    private Coroutine detectingPlayerCoroutine;
    private Coroutine flashingConeCoroutine;
    [SerializeField] private FieldOfViewDetector fieldOfView;
    private GameObject player;
    private Transform playerTransform;
    private Vector3 directionToPlayer;
    private float angleToPlayer;
    private bool playerDetected = false;
    private float mainFOVAngle;
    private float detectionRadius;
    private float distanceToPlayer;

    private PlayerController playerController;

    void Start()
    {
        // Get player object and transform
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerController = player.GetComponent<PlayerController>();
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

        moduleAudio = hackingModule.GetComponent<AudioSource>();
        droneAudio = GetComponent<AudioSource>();

    }

    void Update()
    {
        Oscillate();

        if(canPressE)
        {
            if (playerController.GetIsOverriding())
            {
                // Start playing the audio if it's not already playing
                
                if (!moduleAudio.isPlaying)
                {
                    moduleAudio.Play();
                }

                pressE.SetActive(false);
                overridingSlider.gameObject.SetActive(true);
                overridingText.gameObject.SetActive(true);
                overridingSlider.value += Time.deltaTime / 2;

                if (overridingSlider.value >= overridingSlider.maxValue)
                {
                    overridingSlider.value = 0;
                    AudioOverride();
                }
            }
            else
            {
                // Stop the audio when player stops overriding
                moduleAudio.Stop();
                overridingSlider.value -= Time.deltaTime / 2;

                if (overridingSlider.value <= 0)
                {
                    overridingSlider.value = 0;
                    pressE.SetActive(true);
                    overridingSlider.gameObject.SetActive(false);
                    overridingText.gameObject.SetActive(false);
                }
            }
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
        // Have to add 1.0f to the player's position to make sure the drone is looking at the player's center instead of feet/root
        directionToPlayer = ((playerTransform.position + new Vector3(0, 1.0f, 0)) - transform.position);
        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Reset player detection
        playerDetected = false;

        // Check if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            // If player is in line of sight

            Debug.DrawRay(transform.position, directionToPlayer, Color.red);

            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRadius))
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
        moduleAudio.Stop();
        droneAudio.Stop();
        pressE.SetActive(false);
        canPressE = false;
        isAudioOverrideActive = true;
        hackingModule.GetComponent<BoxCollider>().enabled = false;
        beaconFX.SetActive(false);
        overridingSlider.gameObject.SetActive(false);
        overridingText.gameObject.SetActive(false);
        

        OnDroneHacked?.Invoke();

        // Color the field of view green
        fieldOfView.GetComponent<MeshRenderer>().materials[0].color = new Color(0.0f, 0.5f, 0.0f, 0.5f);
        this.GetComponent<Light>().color = new Color(0.0f, 0.5f, 0.0f, 1.0f);
    }

}