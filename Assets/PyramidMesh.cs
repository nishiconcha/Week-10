using System;
using System.Collections.Generic;
using UnityEngine;

public class PyramidMesh : MonoBehaviour
{
    #region Static Variables

    private Vector3[] verts =
    {
        new(0, 1, 0),
        new(-1, -1, -1), new(1, -1, -1),
        new(1, -1, 1), new(-1, -1, 1)
    };

    private int[] edges =
    {
        1, 2, 2, 3, 3, 4, 4, 1,
        0, 1, 0, 2, 0, 3, 0, 4
    };

    private int[][] faces =
    {
        new[] { 0, 1, 2 }, new[] { 0, 2, 3 }, new[] { 0, 3, 4 }, new[] { 0, 4, 1 },
        new[] { 1, 4, 3, 2 }
    };

    private readonly List<int> visibleEdges = new();

    #endregion

    [SerializeField] private Material lineMat;
    [SerializeField] private Vector3 position = Vector3.zero;
    [SerializeField] private float scale = 1;
    [SerializeField] private float xRotation = 0;
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
            for (int i = 0; i < f.Length; i++)
            {
                visibleEdges.Add(f[i]);
                visibleEdges.Add(f[(i + 1) % f.Length]);
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
        
        var yRotX = point.y * Mathf.Cos(xRotation * Mathf.Deg2Rad) - point.z * Mathf.Sin(xRotation * Mathf.Deg2Rad);
        var zRotX = point.y * Mathf.Sin(xRotation * Mathf.Deg2Rad) + point.z * Mathf.Cos(xRotation * Mathf.Deg2Rad);
        point = new Vector3(point.x, yRotX, zRotX);
        
        var xRot = point.x * Mathf.Cos(yRotation * Mathf.Deg2Rad) - point.z * Mathf.Sin(yRotation * Mathf.Deg2Rad);
        var zRot = point.x * Mathf.Sin(yRotation * Mathf.Deg2Rad) + point.z * Mathf.Cos(yRotation * Mathf.Deg2Rad);

        point = new Vector3(xRot, point.y, zRot);
        point = new Vector3(point.x + position.x, point.y + position.y, point.z + position.z);
        
        var p = focalLength / (focalLength + point.z);

        return new Vector2(point.x, point.y) * scale * p;
    }

    private bool FacesCamera(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b - a).x * (c - a).y - (b - a).y * (c - a).x > 0;
    }

    #endregion
}