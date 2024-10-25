using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script just renders the field of view mesh an object 

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewDetector: MonoBehaviour
{
    [Header("Variables")]
    [Range(0, 360)] public float viewAngle = 90f;        // The angle of the cone
    [SerializeField] public float viewDistance = 5f;    // The distance of the FOV
    [SerializeField] private int coneResolution = 120; // The number of edges in the mesh

    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateViewMesh();
    }

    void CreateViewMesh()
    {
        Vector3[] vertices = new Vector3[coneResolution + 1];
        int[] triangles = new int[(coneResolution - 1) * 3];

        // Calculate the angle of the left and right bounds of the cone
        float currentAngle = -viewAngle / 2;
        float angleStep = viewAngle / coneResolution;

        // Create the vertices for the base of the cone
        for (int i = 0; i < coneResolution; i++)
        {
            float angle = currentAngle + (i * angleStep);
            Vector3 direction = DirectionFromAngle(angle) * viewDistance;
            vertices[i + 1] = direction;
        }

        // Create the triangles for the cone
        for (int i = 0; i < coneResolution - 1; i++)
        {
            triangles[i * 3] = 0; // Apex
            triangles[i * 3 + 1] = i + 1; // Current edge vertex
            triangles[i * 3 + 2] = (i + 2 > coneResolution) ? 1 : i + 2; // Next edge vertex (wrap around)
        }


        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
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
