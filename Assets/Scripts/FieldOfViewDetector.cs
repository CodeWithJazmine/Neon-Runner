using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This script just renders the field of view mesh an object 

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfViewDetector: MonoBehaviour
{
    [Header("Variables")]
    [Range(0, 360)] public float viewAngle = 90f;        // The angle of the cone
    [SerializeField] public float viewDistance = 5f;    // The distance of the FOV

    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateViewMesh();
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
