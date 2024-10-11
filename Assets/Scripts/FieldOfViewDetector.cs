using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class FieldOfViewDetector: MonoBehaviour
{
    [Header("Variables")]
    [Range(0, 360)] public float viewAngle = 90f;        // The angle of the cone
    [SerializeField] private float viewDistance = 5f;    // The distance of the FOV

    public event Action OnPlayerEnterFOV;
    public event Action OnPlayerExitFOV;

    private Mesh mesh;
    private MeshCollider meshCollider;

    private Transform playerTransform;
    private bool playerInRange = false;

    void Start()
    {
        // Just used to draw the line from the enemy to the player can be removed later
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            playerTransform = player.transform;
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
        CreateViewMesh();
    }

    void Update()
    {
        Debug.DrawLine(transform.position, playerTransform.position, Color.red);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 directionToPlayer = other.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle < viewAngle / 2)
            {
                if (!playerInRange)
                {
                    playerInRange = true;
                    //Debug.Log("Player in FOV");
                    OnPlayerEnterFOV?.Invoke();
                }
            }
            else if (playerInRange)
            {
                //Debug.Log("Player not in FOV");
                playerInRange = false;
                OnPlayerExitFOV?.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerInRange)
            {
                //Debug.Log("Player not in FOV");
                playerInRange = false;
                OnPlayerExitFOV?.Invoke();
            }
        }
    }

    void CreateViewMesh()
    {
        Vector3[] vertices = new Vector3[3];
        int[] triangles = new int[3];

        // Calculate the angle of the left and right bounds of the cone
        float angleLeft = -viewAngle / 2;
        float angleRight = viewAngle / 2;

        vertices[0] = Vector3.zero;  // The origin point (at the enemy)
        vertices[1] = DirectionFromAngle(angleLeft) * viewDistance;
        vertices[2] = DirectionFromAngle(angleRight) * viewDistance;

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Assign the mesh to the mesh collider
        meshCollider.sharedMesh = mesh;
    }

    Vector3 DirectionFromAngle(float angleInDegrees)
    {
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // Used for showing the field of view in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        float scaleFactor = transform.lossyScale.x; // Getting the scale from the parent object

        // Calculate updated bounds based on the object's rotation
        Vector3 leftBound = transform.position + transform.TransformDirection(DirectionFromAngle(-viewAngle / 2)) * viewDistance * scaleFactor;
        Vector3 rightBound = transform.position + transform.TransformDirection(DirectionFromAngle(viewAngle / 2)) * viewDistance * scaleFactor;

        // Draw the lines from the current position
        Gizmos.DrawLine(transform.position, leftBound);
        Gizmos.DrawLine(transform.position, rightBound);
        Gizmos.DrawLine(leftBound, rightBound);
    }

}
