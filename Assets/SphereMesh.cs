using System;
using System.Collections.Generic;
using UnityEngine;

public class SphereMesh : MonoBehaviour
{
    #region Static Variables

    private readonly List<Vector3> verts = new();
    private readonly List<int[]> faces = new(); 
    private readonly List<int> visibleEdges = new();

    #endregion

    [SerializeField] private int segments = 8; 
    [SerializeField] private int slices = 12; 

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
        BuildSphere();
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

    private void BuildSphere()
    {
        verts.Clear();
        faces.Clear();

        verts.Add(new Vector3(0, 1, 0));
        verts.Add(new Vector3(0, -1, 0));

        for (int i = 1; i < segments; i++)
        {
            float theta = Mathf.PI * i / segments;
            for (int j = 0; j < slices; j++)
            {
                float phi = 2f * Mathf.PI * j / slices;
                verts.Add(new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi),
                    Mathf.Cos(theta),
                    Mathf.Sin(theta) * Mathf.Sin(phi)));
            }
        }

        int Ring(int ring, int j) => 2 + ring * slices + (j % slices);

        // top 
        for (int j = 0; j < slices; j++)
            faces.Add(new[] { 0, Ring(0, j), Ring(0, j + 1) });

        // middle 
        for (int r = 0; r < segments - 2; r++)
        for (int j = 0; j < slices; j++)
            faces.Add(new[] { Ring(r, j), Ring(r + 1, j), Ring(r + 1, j + 1), Ring(r, j + 1) });

        // bottom 
        int last = segments - 2;
        for (int j = 0; j < slices; j++)
            faces.Add(new[] { 1, Ring(last, j + 1), Ring(last, j) });
    }

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