using UnityEngine;
using System.Collections.Generic;

public class GripRegion : MonoBehaviour
{
    [Header("Region Settings")]
    public int numberOfGrips = 10;
    public GameObject[] gripPrefabs;
    public bool isFinal = false;

    [HideInInspector]
    public List<GameObject> generatedGrips = new List<GameObject>();

    private Collider regionCollider;

    private void Awake()
    {
        regionCollider = GetComponent<Collider>();
        if (regionCollider == null)
        {
            Debug.LogError("GripRegion requires a Collider component.");
        }
    }

    // Instantiates a grip at a given position, linking it to the appropriate parent
    public GameObject InstantiateGrip(Vector3 position)
    {
        if (gripPrefabs.Length == 0)
        {
            Debug.LogWarning($"Region {name} lacks grip prefabs!");
            return null;
        }

        GameObject gripPrefab = gripPrefabs[Random.Range(0, gripPrefabs.Length)];
        GameObject newGrip = Instantiate(gripPrefab, position, Quaternion.identity, transform);
        generatedGrips.Add(newGrip);
        return newGrip;
    }

    // Clears all generated grips in this region
    public void ClearGeneratedGrips()
    {
        foreach (GameObject grip in generatedGrips)
        {
            if (grip != null) Destroy(grip);
        }
        generatedGrips.Clear();
    }

    // Gizmos for visualization of the region and grips
    private void OnDrawGizmos()
    {
        if (regionCollider == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(regionCollider.bounds.center, regionCollider.bounds.size);

        Gizmos.color = Color.red;
        foreach (var grip in generatedGrips)
        {
            if (grip != null)
                Gizmos.DrawSphere(grip.transform.position, 0.1f);
        }
    }
}
