using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GripsGeneration : MonoBehaviour
{
    [Header("Grips Generation")]
    [SerializeField, Range(1, 10)]
    private int gripDensity = 2;
    [SerializeField, Range(0f, 1f)]
    private float gripPercentage = 0.1f;
    [SerializeField]
    private float minGripDistance = 1f;
    [SerializeField]
    private float maxGripDistance = 1f;
    [SerializeField]
    private float maxDistanceFromPath = 1.5f;
    [SerializeField, Range(0f, 1f)]
    private float keepFarGripPercentage = 0.05f;

    [Header("Miscellaneous")]
    [SerializeField]
    private bool isSpawningTent = false;

    [Header("Pathfinding")]
    [SerializeField]
    private Transform[] startingGrips;
    [SerializeField]
    private Transform[] endingGrips;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject[] standardGripPrefabs;
    [SerializeField]
    private GameObject[] technicalGripPrefabs;
    [SerializeField]
    private GameObject[] temporaryGripPrefabs;
    [SerializeField]
    private GameObject gripParent;
    [SerializeField]
    private GameObject[] miscellaneousPrefabs;

    [Header("Debug")]
    [SerializeField]
    private ClimbingDebugVisualizer debugger;

    private Mesh mesh;

    private void Awake()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("No MeshFilter found on this GameObject.");
            return;
        }
        mesh = meshFilter.sharedMesh;

        var grips = ClimbableZone();

        var graph = BuildGripGraph(grips, maxGripDistance);

        var startPositions = startingGrips.Where(g => g != null).Select(g => g.position).ToList();
        var endPositions = endingGrips.Where(g => g != null).Select(g => g.position).ToList();

        var random = new System.Random();
        debugger.paths = new List<List<Vector3>>();

        foreach (var start in startPositions)
        {
            for (int goalIdx = 0; goalIdx < endPositions.Count; goalIdx++)
            {
                var goal = endPositions[goalIdx];
                var path = FindPathAStar(graph, start, goal);
                if (path != null && path.Count > 0)
                    debugger.paths.Add(path.Select(n => n.data.position).ToList());
            }
        }

        Debug.Log(debugger.paths.Count + " paths found.");
        var filteredGrips = FilterGripsByPathProximity(
            grips,
            debugger.paths,
            startPositions,
            endPositions,
            maxDistanceFromPath,
            keepFarGripPercentage
        );

        debugger.SetClimbingGrips(filteredGrips);

        foreach (var grip in filteredGrips)
        {
            if (grip.NearPath && Random.value < 0.001f && miscellaneousPrefabs.Length > 0 && isSpawningTent)
            {
                var misc = miscellaneousPrefabs[Random.Range(0, miscellaneousPrefabs.Length)];
                Instantiate(misc, grip.position, misc.transform.rotation, this.transform);
                continue;
            }

            float rand = Random.value;
            GameObject prefab;

            if (rand < 0.6f && standardGripPrefabs.Length > 0)
            {
                prefab = standardGripPrefabs[Random.Range(0, standardGripPrefabs.Length)];
            }
            else if (rand < 0.9f && technicalGripPrefabs.Length > 0)
            {
                prefab = technicalGripPrefabs[Random.Range(0, technicalGripPrefabs.Length)];
            }
            else if (temporaryGripPrefabs.Length > 0)
            {
                prefab = temporaryGripPrefabs[Random.Range(0, temporaryGripPrefabs.Length)];
            }
            else
            {
                continue;
            }

            var position = grip.position;
            Quaternion rotation = Quaternion.LookRotation(grip.normal) * Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );
            Instantiate(prefab, position, rotation, gripParent.transform);
        }
    }

    private List<GripNode> FindPathAStar(List<GripNode> nodes, Vector3 startPosition, Vector3 goalPosition)
    {
        var start = nodes.OrderBy(n => Vector3.Distance(n.data.position, startPosition)).First();
        var goal = nodes.OrderBy(n => Vector3.Distance(n.data.position, goalPosition)).First();

        var openSet = new SortedSet<(float, GripNode)>(Comparer<(float, GripNode)>.Create((a, b) =>
            a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : a.Item2.index.CompareTo(b.Item2.index)));
        var cameFrom = new Dictionary<GripNode, GripNode>();
        var gScore = nodes.ToDictionary(n => n, n => float.PositiveInfinity);
        var fScore = nodes.ToDictionary(n => n, n => float.PositiveInfinity);

        gScore[start] = 0;
        fScore[start] = Vector3.Distance(start.data.position, goal.data.position);
        openSet.Add((fScore[start], start));

        while (openSet.Count > 0)
        {
            var current = openSet.Min.Item2;
            openSet.Remove(openSet.Min);

            if (current == goal)
            {
                var path = new List<GripNode> { current };
                while (cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    path.Add(current);
                }
                path.Reverse();
                return path;
            }

            foreach (var neighbor in current.neighbors)
            {
                float tentativeG = gScore[current] + Vector3.Distance(current.data.position, neighbor.data.position);
                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Vector3.Distance(neighbor.data.position, goal.data.position);
                    if (!openSet.Any(e => e.Item2 == neighbor))
                        openSet.Add((fScore[neighbor], neighbor));
                }
            }
        }
        return null;
    }

    private List<GripNode> BuildGripGraph(List<GripData> grips, float maxDistance)
    {
        var nodes = grips.Select((g, i) => new GripNode(g, i)).ToList();

        for (int i = 0; i < nodes.Count; i++)
        {
            var from = nodes[i].data;
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                var to = nodes[j].data;
                if (Vector3.Distance(from.position, to.position) <= maxDistance)
                    nodes[i].neighbors.Add(nodes[j]);
            }
        }
        return nodes;
    }

    private List<GripData> ClimbableZone()
    {
        var climbableZone = new List<GripData>();
        var vertices = mesh.vertices;

        foreach (Vector3 triangle in ClimbableTriangles())
        {
            int i0 = (int)triangle.x, i1 = (int)triangle.y, i2 = (int)triangle.z;
            Vector3 v0 = vertices[i0], v1 = vertices[i1], v2 = vertices[i2];
            Vector3 normal = transform.TransformDirection(Vector3.Cross(v1 - v0, v2 - v0).normalized);

            for (int i = 0; i < gripDensity; i++)
            {
                if (Random.value >= gripPercentage)
                    continue;
                Vector3 baryPos = GetRandomBarycentricPosition(v0, v1, v2);
                climbableZone.Add(new GripData(transform.TransformPoint(baryPos), normal));
            }
        }
        AddingBaseGrips(climbableZone);

        // Light filtering
        climbableZone = FilterCloseGrips(climbableZone, minGripDistance);

        return climbableZone;
    }

    private List<Vector3> ClimbableTriangles()
    {
        var climbableTriangles = new List<Vector3>();
        var colors = mesh.colors;
        var triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i], i1 = triangles[i + 1], i2 = triangles[i + 2];
            float rAvg = (colors[i0].r + colors[i1].r + colors[i2].r) / 3f;
            if (rAvg > 0.5f)
                climbableTriangles.Add(new Vector3(i0, i1, i2));
        }
        return climbableTriangles;
    }

    private Vector3 GetRandomBarycentricPosition(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        float a = Random.value, b = Random.value;
        if (a + b > 1f) { a = 1f - a; b = 1f - b; }
        float u = 1f - a - b, v = a, w = b;
        return u * v0 + v * v1 + w * v2;
    }

    private void AddingBaseGrips(List<GripData> climbableZone)
    {
        if (startingGrips != null)
        {
            foreach (var startGrip in startingGrips)
                if (startGrip != null)
                    climbableZone.Add(new GripData(startGrip.position, startGrip.forward));
        }
        if (endingGrips != null)
        {
            foreach (var endGrip in endingGrips)
                if (endGrip != null)
                    climbableZone.Add(new GripData(endGrip.position, endGrip.forward));
        }
    }

    private List<GripData> FilterCloseGrips(List<GripData> grips, float minDistance)
    {
        var filtered = new List<GripData>();
        foreach (var grip in grips)
        {
            bool isStartOrEnd = startingGrips.Any(g => g != null && Vector3.Distance(g.position, grip.position) < 0.01f) || endingGrips.Any(g => g != null && Vector3.Distance(g.position, grip.position) < 0.01f);

            if (isStartOrEnd || filtered.All(other => Vector3.Distance(grip.position, other.position) >= minDistance))
                filtered.Add(grip);
        }
        return filtered;
    }

    private List<GripData> FilterGripsByPathProximity(
    List<GripData> allGrips,
    List<List<Vector3>> paths,
    List<Vector3> startPositions,
    List<Vector3> endPositions,
    float maxDistanceFromPath = 1.5f,
    float keepFarGripPercentage = 0.1f)
    {
        var pathPoints = new List<Vector3>();
        foreach (var path in paths)
            pathPoints.AddRange(path);
        pathPoints.AddRange(startPositions);
        pathPoints.AddRange(endPositions);

        var closeGrips = new List<GripData>();
        var farGrips = new List<GripData>();

        foreach (var originalGrip in allGrips)
        {
            var grip = originalGrip;
            bool near = false;
            for (int i = 0; i < pathPoints.Count; i++)
            {
                if (Vector3.Distance(grip.position, pathPoints[i]) <= maxDistanceFromPath)
                {
                    near = true;
                    break;
                }
            }
            grip.NearPath = near;
            if (near)
                closeGrips.Add(grip);
            else
                farGrips.Add(grip);
        }

        int nbToKeep = Mathf.CeilToInt(farGrips.Count * keepFarGripPercentage);
        if (nbToKeep > 0)
        {
            var random = new System.Random();
            closeGrips.AddRange(farGrips.OrderBy(_ => random.Next()).Take(nbToKeep));
        }

        return closeGrips;
    }
}
