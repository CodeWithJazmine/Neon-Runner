using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyGuard : MonoBehaviour
{
    private Coroutine chaseCoroutine;

    //Mesh renderer colors
    private Color originalColor = new(0.0f,0.0f,0.0f,0.0f);
    private Color detectedColor = new(0.5f, 0.0f, 0.0f, 0.5f);

    //public event Action OnGuardCaughtPlayer;

    [Header("Guard Variables")]
    [SerializeField] private float detectionTime = 3.0f;
    private float originalDetectionTime;
    private float playerVisibleTimer;
    [SerializeField] GameObject alertUIObject;
    [SerializeField] GameObject suspiciousUIObject;

    
    [Header("Pathing Variables")]
    private Vector3[] waypoints;
    [SerializeField] private Transform pathHolder;
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float waitTime = 3.0f;
    [SerializeField] private float turnSpeed = 90.0f;

    private GameObject player;
    private Transform playerTransform;
    private Vector3 directionToPlayer;
    private float angleToPlayer;
    private bool playerDetected = false;

    [SerializeField] private FieldOfViewDetector fieldOfView;
    private float distanceToPlayer;
    [SerializeField] private float mainFOVAngle;
    [SerializeField] private float peripheralFOVAngle = 120.0f;
    [SerializeField] private float detectionRadius = 5.0f;
    [SerializeField] private float shoulderDetectionRadius = 2.0f;


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

        // Save the original detection time
        originalDetectionTime = detectionTime;

        // Get the waypoints from the pathHolder
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        // Set UI objects to false
        alertUIObject.SetActive(false);
        suspiciousUIObject.SetActive(false);

        // Start following path
        StartCoroutine(FollowPath(waypoints));
    }

    private void CheckPlayerVisibility()
    {
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
                        Debug.Log("Player in Main FOV");

                        detectionTime = originalDetectionTime;
                        StartChaseCoroutine(detectionTime);
                        playerDetected = true;

                        alertUIObject.SetActive(true);
                        suspiciousUIObject.SetActive(false);
                    }
                    // If player is within peripheral view
                    else if (angleToPlayer <= peripheralFOVAngle / 2.0f)
                    {
                        Debug.Log("Player in Peripheral FOV");

                        detectionTime = originalDetectionTime + 1.0f;
                        StartChaseCoroutine(detectionTime);
                        playerDetected = true;

                        suspiciousUIObject.SetActive(true);
                        alertUIObject.SetActive(false);
                    }
                    // If player is behind the enemy (over the shoulder) and closer than the detection radius
                    else if (distanceToPlayer <= shoulderDetectionRadius)
                    {
                        Debug.Log("Player in Shoulder FOV");

                        detectionTime = originalDetectionTime / 2.0f;
                        StartChaseCoroutine(detectionTime);
                        playerDetected = true;

                        suspiciousUIObject.SetActive(true);
                        alertUIObject.SetActive(false);
                    }
                }
            }
        }

        if (!playerDetected)
        {
            StopChaseCoroutine(detectionTime);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckPlayerVisibility();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited detection radius");

            StopChaseCoroutine(detectionTime);
        }
    }

    private void StartChaseCoroutine(float detectionTime)
    {
        if (chaseCoroutine == null)
        {
            chaseCoroutine = StartCoroutine(Chase(detectionTime));
        }
    }

    private void StopChaseCoroutine(float detectionTime)
    {
        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            chaseCoroutine = null;
            StartCoroutine(StopChase(detectionTime));
        }
    }
    IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];
        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);

        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if(transform.position ==  targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 direction = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        while(Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    IEnumerator Chase(float detectionTime)
    {
        while (playerVisibleTimer < detectionTime)
        {
            playerVisibleTimer += Time.deltaTime;
            playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, detectionTime);

            if(alertUIObject.activeSelf)
            {
                alertUIObject.transform.LookAt(Camera.main.transform);
            }
            if (suspiciousUIObject.activeSelf)
            {
                suspiciousUIObject.transform.LookAt(Camera.main.transform);
            }

            // Interpolate the mesh renderer's original color to detected color based on player detection
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = Color.Lerp(originalColor, detectedColor, playerVisibleTimer / detectionTime);

            if (playerVisibleTimer >= detectionTime)
            {
                // TODO: perhaps have an event to specifically handle OnGuardCaughtPlayer instead of ending the game
                GameManager.instance.YouLose();
                yield break;
                //OnGuardCaughtPlayer?.Invoke();
            }
            yield return null;
        }
    }

    IEnumerator StopChase(float detectionTime)
    {
        while (playerVisibleTimer > 0)
        {
            playerVisibleTimer -= Time.deltaTime;
            playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, detectionTime);

            alertUIObject.SetActive(false);
            suspiciousUIObject.SetActive(false);

            // Interpolate the mesh renderer's detected color back to original color based on player detection
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = Color.Lerp(detectedColor, originalColor, 1 - (playerVisibleTimer / detectionTime));
        }
        yield return null;
    }


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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius * transform.lossyScale.x); // Draw the detection radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shoulderDetectionRadius * transform.lossyScale.x ); // Draw the shoulder detection radius
    }
}

