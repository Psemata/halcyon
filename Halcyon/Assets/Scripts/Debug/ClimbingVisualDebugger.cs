using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class ClimbingDebugVisualizer : MonoBehaviour
{
    public MeshFilter[] targetMeshFilter;
    public Color climbableColor = new Color(1, 0, 0, 0.2f);
    public float gizmoSphereRadius = 0.1f;

    [Header("Empty Zones")]
    public Transform[] startZone;
    public Transform[] goalZone;
    public Transform[] hintZones;

    [Header("Grips")]
    public List<GripData> climbingGripsData = new List<GripData>();
    public Color holdColor = Color.yellow;
    public Color startHoldColor = Color.blue;
    public Color goalHoldColor = Color.green;

    [Header("Path debug (optionnel)")]
    public List<List<Vector3>> paths;
    public List<(Vector3, Vector3)> gripLinks = new List<(Vector3, Vector3)>();

    public void SetClimbingGrips(List<GripData> grips)
    {
        climbingGripsData.AddRange(grips);
    }

    public void SetGripLinks(List<(Vector3, Vector3)> links)
    {
        gripLinks = links;
    }

    private void OnDrawGizmos()
    {
        if (targetMeshFilter.Length < 0)
            return;

        foreach (var meshFilter in targetMeshFilter)
        {
            if (!meshFilter || !meshFilter.sharedMesh)
                continue;

            Mesh mesh = meshFilter.sharedMesh;
            Color[] colors = mesh.colors;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            if (colors.Length != vertices.Length)
                return;

            Transform t = meshFilter.transform;

            // --- Dessiner triangles grimpables
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                Color c0 = colors[i0];
                Color c1 = colors[i1];
                Color c2 = colors[i2];
                float r = (c0.r + c1.r + c2.r) / 3f;

                if (r > 0.5f)
                {
                    Vector3 v0 = t.TransformPoint(vertices[i0]);
                    Vector3 v1 = t.TransformPoint(vertices[i1]);
                    Vector3 v2 = t.TransformPoint(vertices[i2]);

                    Gizmos.color = climbableColor;
                    Gizmos.DrawLine(v0, v1);
                    Gizmos.DrawLine(v1, v2);
                    Gizmos.DrawLine(v2, v0);
                }
            }
        }

        // --- Dessiner zones Start / Goal
        if (startZone != null && startZone.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach (var start in startZone)
            {
                if (start)
                    Gizmos.DrawWireSphere(start.position, gizmoSphereRadius);
            }
        }

        if (goalZone != null && goalZone.Length > 0)
        {
            Gizmos.color = Color.green;
            foreach (var goal in goalZone)
            {
                if (goal)
                    Gizmos.DrawWireSphere(goal.position, gizmoSphereRadius);
            }
        }

        if (hintZones != null && hintZones.Length > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (var hint in hintZones)
            {
                if (hint)
                    Gizmos.DrawWireCube(hint.position, Vector3.one * gizmoSphereRadius);
            }
        }

        // --- Dessiner les prises
        if (climbingGripsData != null && climbingGripsData.Count > 0)
        {
            Gizmos.color = holdColor;
            foreach (var gripData in climbingGripsData)
            {
                Gizmos.DrawSphere(gripData.position, gizmoSphereRadius);
            }
        }

        // --- Dessiner les liens du graphe
        if (gripLinks != null && gripLinks.Count > 0)
        {
            Gizmos.color = Color.blue;
            foreach (var link in gripLinks)
            {
                Gizmos.DrawLine(link.Item1, link.Item2);
            }
        }

        // --- Dessiner le chemin
        if (paths != null && paths.Count > 0)
        {
            Gizmos.color = Color.magenta;
            foreach (var path in paths)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    if (path[i] != Vector3.zero && path[i + 1] != Vector3.zero)
                        Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }
}
