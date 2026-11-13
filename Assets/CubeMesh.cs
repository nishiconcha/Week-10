using System;
using System.Collections.Generic;
using UnityEngine;

public class CubeMesh : MonoBehaviour
{
    #region Static Variables

    private Vector3[] verts =
    {
        new(-1, -1, -1), new(1, -1, -1), // 0, 1
        new(1, 1, -1), new(-1, 1, -1), // 2, 3
        new(-1, -1, 1), new(1, -1, 1), // 4, 5
        new(1, 1, 1), new(-1, 1, 1)
    }; // 6, 7

    private int[] edges =
    {
        0, 1, 1, 2, 2, 3, 3, 0, // front ring
        4, 5, 5, 6, 6, 7, 7, 4, // back ring
        0, 4, 1, 5, 2, 6, 3, 7
    }; // connectors

    private int[][] faces =
    {
        new[] { 0, 1, 2, 3 }, new[] { 5, 4, 7, 6 }, new[] { 4, 0, 3, 7 }, // counter-clockwise
        new[] { 1, 5, 6, 2 }, new[] { 3, 2, 6, 7 }, new[] { 0, 4, 5, 1 }
    }; // from outside

    private readonly List<int> visibleEdges = new();

    #endregion

    [SerializeField] private Material lineMat;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private float scale = 1;
    [SerializeField] private float yRotation = 0;

    [Range(1, 500)] [SerializeField] private float focalLength = 20f;

    private Mesh mesh;
    private readonly List<Vector3> actualPosition = new();

    #region Unity Functions

    private void Awake()
    {
        mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = lineMat;
    }

    void LateUpdate()
    {
        actualPosition.Clear();
        foreach (var point in verts)
        {
            actualPosition.Add(Project(point));
        }

        mesh.vertices = actualPosition.ToArray();
        visibleEdges.Clear();
        
        
        foreach (var f in faces)
        {
            if (!FacesCamera(actualPosition[f[0]], actualPosition[f[1]], actualPosition[f[2]])) continue;
            for (int i = 0; i < 4; i++)
            {
                visibleEdges.Add(f[i]);
                visibleEdges.Add(f[(i + 1) % 4]);
            }
            
        }
        
        mesh.SetIndices(visibleEdges, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
    }

    #endregion

    #region Helper

    private Vector2 Project(Vector3 point)
    {
        point *= .5f;

        //rotate via Y axis
        var xRot = point.x * Mathf.Cos(yRotation * Mathf.Deg2Rad) - point.z * Mathf.Sin(yRotation * Mathf.Deg2Rad);
        var zRot = point.x * Mathf.Sin(yRotation * Mathf.Deg2Rad) + point.z * Mathf.Cos(yRotation * Mathf.Deg2Rad);
        // new rotated point
        point = new Vector3(xRot, point.y, zRot);

        point = new Vector3(point.x + position.x, point.y + position.y, point.z + position.z);



        // get perspective with rotated point
        var p = focalLength / (focalLength + point.z);

        return new Vector2(point.x, point.y) * scale * p;
    }

    private bool FacesCamera(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b - a).x * (c - a).y - (b - a).y * (c - a).x > 0;
    }
    
    

    #endregion


}