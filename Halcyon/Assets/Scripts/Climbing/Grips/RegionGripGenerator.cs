using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RegionGripGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform mountainSurface;
    public List<GripRegion> regions = new List<GripRegion>();
    public float maxGripReach = 3f;
    public int minimumPaths = 1;

    [Header("Final Region Settings")]
    public float finalRegionTolerance = 2f;

    private Dictionary<GameObject, List<GameObject>> gripGraph = new Dictionary<GameObject, List<GameObject>>();

    void Start()
    {
        StartCoroutine(GenerateGripsAsync());
    }

    // Main coroutine to generate grips
    IEnumerator GenerateGripsAsync()
    {
        bool pathFound = false;

        while (!pathFound)
        {
            yield return ClearAllGrips();
            yield return GenerateGripsInRegionsAdaptive();
            AdjustFinalRegionGrips();
            BuildGripGraph();
            pathFound = ValidateMultiplePaths();
        }

        Debug.Log($"Generation succeeded with at least {minimumPaths} valid path(s).");
    }

    // Clears all existing grips
    IEnumerator ClearAllGrips()
    {
        foreach (var region in regions)
        {
            region.ClearGeneratedGrips();
            yield return null;
        }
    }

    // Adaptive grip generation using a grid-like approach
    IEnumerator GenerateGripsInRegionsAdaptive()
    {
        foreach (var region in regions)
        {
            var collider = region.GetComponent<Collider>();
            float regionWidth = collider.bounds.size.x;
            float regionHeight = collider.bounds.size.y;
            float regionDepth = collider.bounds.size.z;

            int gridSize = Mathf.CeilToInt(Mathf.Pow(region.numberOfGrips, 1f / 3f));
            gridSize = Mathf.Max(gridSize, 1);

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    for (int k = 0; k < gridSize; k++)
                    {
                        Vector3 offset = new Vector3(
                            (i + Random.Range(0f, 1f)) * (regionWidth / gridSize),
                            (j + Random.Range(0f, 1f)) * (regionHeight / gridSize),
                            (k + Random.Range(0f, 1f)) * (regionDepth / gridSize)
                        );

                        Vector3 randomPoint = collider.bounds.min + offset;

                        GameObject newGrip = region.InstantiateGrip(randomPoint);
                        if (newGrip == null || !AlignGripToSurfaceAdaptive(newGrip))
                        {
                            if (newGrip != null)
                                region.generatedGrips.Remove(newGrip);
                        }

                        yield return null;
                    }
                }
            }
        }
    }


    // Adaptive alignment with multiple attempts
    bool AlignGripToSurfaceAdaptive(GameObject grip, int attempts = 5)
    {
        for (int i = 0; i < attempts; i++)
        {
            if (Physics.Raycast(grip.transform.position, -mountainSurface.transform.up, out RaycastHit hit, 10f) && hit.transform == mountainSurface)
            {
                grip.transform.position = hit.point;
                grip.transform.rotation = Quaternion.LookRotation(hit.normal);
                return true;
            }
            else
            {
                grip.transform.position += Random.insideUnitSphere * 0.5f; // Small random reposition
            }
        }
        Destroy(grip);
        return false;
    }

    // Adjusts grips in the final region to be closer to the mountain top
    void AdjustFinalRegionGrips()
    {
        float topHeight = GetMountainTopHeight();
        foreach (var region in regions)
        {
            if (region.isFinal)
            {
                foreach (var grip in region.generatedGrips)
                {
                    Vector3 gripPosition = grip.transform.position;
                    if (Mathf.Abs(gripPosition.y - topHeight) > finalRegionTolerance)
                    {
                        gripPosition.y = topHeight - Random.Range(0f, finalRegionTolerance);
                        grip.transform.position = gripPosition;
                    }
                }
            }
        }
    }

    // Retrieves the mountain's top height
    float GetMountainTopHeight()
    {
        Collider mountainCollider = mountainSurface.GetComponent<Collider>();
        return mountainCollider.bounds.max.y;
    }

    // Constructs the graph of grips connections
    void BuildGripGraph()
    {
        gripGraph.Clear();

        foreach (var region in regions)
        {
            foreach (var grip in region.generatedGrips)
            {
                gripGraph[grip] = new List<GameObject>();

                foreach (var otherRegion in regions)
                {
                    foreach (var otherGrip in otherRegion.generatedGrips)
                    {
                        if (grip == otherGrip)
                            continue; // skip self

                        float distance = Vector3.Distance(grip.transform.position, otherGrip.transform.position);

                        if (distance <= maxGripReach)
                        {
                            gripGraph[grip].Add(otherGrip);
                        }
                    }
                }
            }
        }
    }

    // Validates if multiple paths exist from the final region
    bool ValidateMultiplePaths()
    {
        int pathsFound = 0;
        foreach (var region in regions)
        {
            if (region.isFinal)
            {
                foreach (var grip in region.generatedGrips)
                {
                    if (PathExists(grip))
                    {
                        pathsFound++;
                        if (pathsFound >= minimumPaths) return true;
                    }
                }
            }
        }
        return false;
    }

    // Checks if a path exists from the starting grip
    bool PathExists(GameObject start)
    {
        HashSet<GameObject> visited = new HashSet<GameObject>();
        Queue<GameObject> queue = new Queue<GameObject>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Contains(current)) continue;
            visited.Add(current);

            if (IsEndRegion(current)) return true;

            foreach (var neighbor in gripGraph[current])
            {
                if (!visited.Contains(neighbor)) queue.Enqueue(neighbor);
            }
        }

        return false;
    }

    // Checks if a grip is part of a final region
    bool IsEndRegion(GameObject grip)
    {
        foreach (var region in regions)
        {
            if (region.isFinal && region.generatedGrips.Contains(grip))
                return true;
        }
        return false;
    }
}
