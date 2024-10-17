using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGuard : MonoBehaviour
{
    #region Coroutines and Flags
    // Coroutines
    private Coroutine detectingCoroutine;
    private Coroutine stopDetectCoroutine;

    // Flags
    private bool playerInSight;
    private bool playerDetected;
    private bool isDetectingPlayer;
    private bool isWaiting = false;
    private bool playerVisible;
    private bool playerOutOfSight;

    #endregion

    #region Detection Settings
    [Header("Detection Settings")]
    [SerializeField] private float detectionTime = 3.0f;
    [SerializeField] private FieldOfViewDetector fieldOfView;
    [SerializeField] private float mainFOVAngle;
    [SerializeField] private float peripheralFOVAngle = 180.0f;
    [SerializeField] private float detectionRadius = 5.0f;
    [SerializeField] private float shoulderDetectionRadius = 2.0f;

    private float playerVisibleTimer;
    private Vector3 directionToPlayer;
    private float distanceToPlayer;
    private float angleToPlayer;
    #endregion

    #region UI and Visuals
    [Header("UI and Visuals")]
    // UI elements
    [SerializeField] private GameObject alertUIObject;
    [SerializeField] private GameObject suspiciousUIObject;

    // Mesh renderer colors
    private Color originalColor = new(0.0f, 0.0f, 0.0f, 0.0f);
    private Color detectedColor = new(0.5f, 0.0f, 0.0f, 0.5f);

    #endregion

    #region Movement and Pathing
    [Header("Movement and Pathing")]
    private NavMeshAgent agent;

    // Pathing variables
    [SerializeField] private Transform pathHolder;
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float waitTime = 3.0f;
    [SerializeField] private float turnSpeed = 90.0f;
    private Vector3[] waypoints;
    private Vector3 targetWaypoint;
    private int targetWaypointIndex = 0;
    #endregion


    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float attackDelay = 1.0f;
    [SerializeField] private float stoppingDistance = 2.0f;
    [SerializeField] private float playerFaceSpeed = 6.0f;
    private float projectileTime;
    private bool isShooting = false;
    #endregion

    #region Player Reference
    private GameObject player;
    private Transform playerTransform;
    private Vector3 lastKnownPlayerPos;
    #endregion


    void Start()
    {
        // Get player object and transform
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Get the FieldOfViewDetector component and assign variables
        fieldOfView = GetComponentInChildren<FieldOfViewDetector>();
        if (fieldOfView != null)
        {
            mainFOVAngle = fieldOfView.viewAngle;
            detectionRadius = fieldOfView.viewDistance;
        }

        // Set the detection radius of the sphere collider
        transform.GetComponent<SphereCollider>().radius = detectionRadius;

        // Get the waypoints from the pathHolder
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.angularSpeed = turnSpeed;

        // Set UI objects to false
        alertUIObject.SetActive(false);
        suspiciousUIObject.SetActive(false);

        // Set the first waypoint as the target
        targetWaypoint = waypoints[targetWaypointIndex];
        agent.SetDestination(targetWaypoint);

    }

    void Update()
    {
        CheckPlayerVisibility();

        if (!playerDetected && !playerInSight)
        {
            agent.stoppingDistance = 0.0f;
            FollowPath();
        }
        else if (playerDetected)
        {
            // Keep chasing even if player is not in sight
            if (playerInSight)
            {
                lastKnownPlayerPos = playerTransform.position;

                agent.SetDestination(playerTransform.position);
                playerOutOfSight = false;

                agent.stoppingDistance = stoppingDistance;

                // Face the player if in range
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    FacePlayer();

                    if (!isShooting)
                    {
                        StartCoroutine(Attack());
                    }
                }
            }
            else if (!playerOutOfSight)
            {
                // Go to the last known player position
                agent.SetDestination(lastKnownPlayerPos);
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    StartCoroutine(WaitAtLastKnownPlayerPos());
                }
            }
        }
    }

    private void CheckPlayerVisibility()
    {
        // Calculate the direction, distance, and angle to the player
        // Have to add 1.0f to the player's position to make sure the drone is looking at the player's center instead of feet/root
        directionToPlayer = ((playerTransform.position + new Vector3(0, 1.0f, 0)) - transform.position);
        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Reset player detection
        playerVisible = false;

        // Check if player is within detection radius
        if (distanceToPlayer <= detectionRadius)
        {
            // If player is in line of sight
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRadius))
            {
                Debug.DrawRay(transform.position, directionToPlayer, Color.red);
                if (hit.collider.CompareTag("Player"))
                {
                    // If player is within the main field of view
                    if (angleToPlayer <= mainFOVAngle / 2.0f)
                    {
                        Debug.Log("Player in Main FOV");

                        StartDetecting(detectionTime - 2.0f);
                        playerVisible = true;
                    }
                    // If player is within peripheral view
                    else if (angleToPlayer <= peripheralFOVAngle / 2.0f)
                    {
                        Debug.Log("Player in Peripheral FOV");

                        StartDetecting(detectionTime);
                        playerVisible = true;

                    }
                    // If player is behind the enemy (over the shoulder) and closer than the detection radius
                    else if (distanceToPlayer <= shoulderDetectionRadius)
                    {
                        Debug.Log("Player in Shoulder FOV");
                        StartDetecting(detectionTime / 2.0f);
                        playerVisible = true;
                    }
                }
            }
        }

        if (!playerVisible & playerInSight)
        { 
            StopDetecting(detectionTime);

            playerInSight = false;
            playerDetected = false;
        }
        
    }

    private IEnumerator Attack()
    {
        isShooting = true;
        Instantiate(projectile, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        yield return new WaitForSeconds(attackDelay);
        isShooting = false;
    }

    private void FacePlayer()
    {
        directionToPlayer = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * playerFaceSpeed);
    }

    #region Player Detection Functions
    private void StartDetecting(float detectionTime)
    {
        // Cancel any existing stop detection coroutine

        if (stopDetectCoroutine != null)
        {
            StopCoroutine(stopDetectCoroutine);
            stopDetectCoroutine = null;
        }

        // Start detecting the player if not already detecting
        if (!isDetectingPlayer && detectingCoroutine == null)
        {
            isDetectingPlayer = true;
            detectingCoroutine = StartCoroutine(Detect(detectionTime));
        }
    }

    private void StopDetecting(float detectionTime)
    {
        // Cancel any existing stop detection coroutine
        if (stopDetectCoroutine != null)
        {
            StopCoroutine(stopDetectCoroutine);
            stopDetectCoroutine = null;
        }
        // Stop detecting the player
        if (detectingCoroutine != null)
        {
            StopCoroutine(detectingCoroutine);
            detectingCoroutine = null;
        }

        // Reset detection variables
        playerDetected = false;
        isDetectingPlayer = false;

        // Start the stop detection coroutine
        stopDetectCoroutine = StartCoroutine(StopDetect(detectionTime));

    }

    IEnumerator Detect(float detectionTime)
    {
        Debug.Log("Detecting player");

        while (playerVisibleTimer < detectionTime)
        {
            suspiciousUIObject.SetActive(true);
            suspiciousUIObject.transform.LookAt(Camera.main.transform);
            Debug.Log("UI Object LookAt Camera");

            playerVisibleTimer += Time.deltaTime;
            playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, detectionTime);

            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = Color.Lerp(originalColor, detectedColor, playerVisibleTimer / detectionTime);

            yield return null;

        }
        if (playerVisibleTimer >= detectionTime)
        {
            Debug.Log("Player detected");
            playerDetected = true;
            playerInSight = true;
            alertUIObject.SetActive(true);
            suspiciousUIObject.SetActive(false);
        }

         isDetectingPlayer = false;
    }

    IEnumerator StopDetect(float detectionTime)
    {
        Debug.Log("Stopping detection");

        while (playerVisibleTimer > 0)
        {
            playerVisibleTimer -= Time.deltaTime;
            playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, detectionTime);

            alertUIObject.SetActive(false);
            suspiciousUIObject.SetActive(true);

            // Interpolate the mesh renderer's detected color back to original color based on player detection time
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = Color.Lerp(detectedColor, originalColor, 1 - (playerVisibleTimer / detectionTime));
            yield return null;
        }

        suspiciousUIObject.SetActive(false);
        playerDetected = false;
        stopDetectCoroutine = null;
    }

    IEnumerator WaitAtLastKnownPlayerPos()
    {
        suspiciousUIObject.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        suspiciousUIObject.SetActive(false);
        playerOutOfSight = true;
        playerDetected = false;
    }

    #endregion


    #region Pathing Functions

    void FollowPath()
    {
        // If agent reaches its current waypoint
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                StartCoroutine(WaitAtWaypoint());
            }
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);  // Wait for the defined wait time at the waypoint

        // Move to the next waypoint
        targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
        targetWaypoint = waypoints[targetWaypointIndex];
        agent.SetDestination(targetWaypoint);

        isWaiting = false;
    }

    #endregion


    #region Gizmos
    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, 0.3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, detectionRadius * transform.lossyScale.x); // Draw the detection radius
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(transform.position, shoulderDetectionRadius * transform.lossyScale.x ); // Draw the shoulder detection radius
    }
    #endregion
}

