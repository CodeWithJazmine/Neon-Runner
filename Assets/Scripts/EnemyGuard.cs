using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGuard : MonoBehaviour, IChasePlayer
{
    private Coroutine chaseCoroutine;
    private FieldOfViewDetector fieldOfView;

    //Mesh renderer colors
    private Color originalColor = new(0.0f,0.0f,0.0f,0.0f);
    private Color detectedColor = new(0.5f, 0.0f, 0.0f, 0.5f);

    public event Action OnGuardCaughtPlayer;

    [Header("Guard Variables")]
    [SerializeField] private float detectionTime = 3.0f;
    private float playerVisibleTimer;

    [Header("Pathing Variables")]
    private Vector3[] waypoints;
    [SerializeField] private Transform pathHolder;
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float waitTime = 3.0f;
    [SerializeField] private float turnSpeed = 90.0f;


    void Start()
    {
        // Get the FieldOfViewDetector component and subscribe to its events
        fieldOfView = GetComponentInChildren<FieldOfViewDetector>();
        if (fieldOfView != null)
        {
            fieldOfView.OnPlayerEnterFOV += ChasePlayer;
            fieldOfView.OnPlayerExitFOV += StopChasingPlayer;
        }

        // Get the waypoints from the pathHolder
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        // Start following path
        StartCoroutine(FollowPath(waypoints));
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

    public void ChasePlayer()
    {
       if (chaseCoroutine == null)
       {
           chaseCoroutine = StartCoroutine(Chase());
       }
        Debug.Log("Chasing Player");

    }
    public void StopChasingPlayer()
    {
        if (chaseCoroutine != null)
        {
            StopCoroutine(chaseCoroutine);
            chaseCoroutine = null;
            StartCoroutine(StopChase());
        }
        Debug.Log("Stopped Chasing Player");
       
    }

    IEnumerator Chase()
    {
        while(playerVisibleTimer < detectionTime)
        {
            
            playerVisibleTimer += Time.deltaTime;
            playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, detectionTime);

            // Interpolate the mesh renderer's original color to detected color based on player detection
            fieldOfView.GetComponent<MeshRenderer>().materials[0].color = Color.Lerp(originalColor, detectedColor, playerVisibleTimer / detectionTime);

            if (playerVisibleTimer >= detectionTime)
            {
                // perhaps have an event to specifically handle OnGuardCaughtPlayer instead of ending the game
                GameManager.instance.YouLose();
                yield break;
                //OnGuardCaughtPlayer?.Invoke();
            }
            yield return null;
        }
    }

    IEnumerator StopChase()
    {
        while (playerVisibleTimer > 0)
        {

            playerVisibleTimer -= Time.deltaTime;
            playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, detectionTime);

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
    }
}
