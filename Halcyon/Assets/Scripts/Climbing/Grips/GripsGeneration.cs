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
    private float minGripDistance = 0.5f;
    [SerializeField]
    private float maxGripDistance = 1f;
    [SerializeField]
    private float maxDistanceFromPath = 1.5f;
    [SerializeField, Range(0f, 1f)]
    private float keepFarGripPercentage = 0.05f;

    [Header("Pathfinding")]
    [SerializeField]
    private Transform[] startingGrips;
    [SerializeField]
    private Transform[] endingGrips;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject[] standardGripPrefabs;

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
            int nbPaths = random.Next(1, 4);
            var usedGoals = new HashSet<int>();

            for (int i = 0; i < nbPaths && usedGoals.Count < endPositions.Count; i++)
            {
                int goalIdx;
                do
                {
                    goalIdx = random.Next(endPositions.Count);
                } while (usedGoals.Contains(goalIdx) && usedGoals.Count < endPositions.Count);

                usedGoals.Add(goalIdx);
                var goal = endPositions[goalIdx];

                var path = FindPathAStar(graph, start, goal);
                if (path != null && path.Count > 1)
                    debugger.paths.Add(path.Select(n => n.data.position).ToList());
            }
        }

        var filteredGrips = FilterGripsByPathProximity(
            grips,
            debugger.paths,
            startPositions,
            endPositions,
            maxDistanceFromPath,
            keepFarGripPercentage
        );

        debugger.SetClimbingGrips(filteredGrips);
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
        AddingBaseGrips(climbableZone);
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
        return FilterCloseGrips(climbableZone, minGripDistance);
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
            if (filtered.All(other => Vector3.Distance(grip.position, other.position) >= minDistance))
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
        var pathPoints = new HashSet<Vector3>(paths.SelectMany(p => p));
        foreach (var s in startPositions) pathPoints.Add(s);
        foreach (var e in endPositions) pathPoints.Add(e);

        var closeGrips = allGrips.Where(grip =>
            pathPoints.Any(pathPt => Vector3.Distance(grip.position, pathPt) <= maxDistanceFromPath)
        ).ToList();

        var farGrips = allGrips.Except(closeGrips).ToList();
        int nbToKeep = Mathf.CeilToInt(farGrips.Count * keepFarGripPercentage);
        if (nbToKeep > 0)
        {
            var random = new System.Random();
            closeGrips.AddRange(farGrips.OrderBy(_ => random.Next()).Take(nbToKeep));
        }
        return closeGrips;
    }
}
