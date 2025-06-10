using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

public class ClimbingTransition : MonoBehaviour
{
    [SerializeField] private GameObject character;
    [SerializeField] private ClimbProvider climbProvider;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform[] nextClimbinPositions;
    [SerializeField] private Collider plateauCollider;

    private bool isArrived = false;

    void Update()
    {
        if (!isArrived && climbProvider.enabled && IsOnMountainGround())
        {
            MoveToNextGrimpable();
        }
    }

    public void MoveToNextGrimpable()
    {
        Transform nextPoint = FindClosestClimbingPoint();
        if (nextPoint != null)
            StartCoroutine(MoveCoroutine(nextPoint));
    }

    IEnumerator MoveCoroutine(Transform target)
    {
        climbProvider.enabled = false;
        Vector3 start = character.transform.position;
        Vector3 end = target.position;

        // On garde la hauteur de départ pour le plan XZ
        Vector3 startXZ = new Vector3(start.x, start.y, start.z);
        Vector3 endXZ = new Vector3(end.x, start.y, end.z);

        float stopDistance = 0.5f;
        Vector3 dirXZ = (endXZ - startXZ).normalized;
        Vector3 finalXZ = endXZ - dirXZ * stopDistance;

        float t = 0f;
        float totalDist = Vector3.Distance(startXZ, finalXZ);

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed / totalDist;
            Vector3 nextXZ = Vector3.Lerp(startXZ, finalXZ, t);

            // Raycast vers le bas pour trouver le sol sous nextXZ
            float raycastHeight = 5f;
            Vector3 rayOrigin = new Vector3(nextXZ.x, nextXZ.y + raycastHeight, nextXZ.z);
            RaycastHit hit;
            int mountainMask = 1 << LayerMask.NameToLayer("MountainGround");
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 10f, mountainMask))
            {
                character.transform.position = new Vector3(nextXZ.x, hit.point.y, nextXZ.z);
            }

            yield return null;
        }

        climbProvider.enabled = true;
        isArrived = true;
    }

    private Transform FindClosestClimbingPoint()
    {
        if (nextClimbinPositions == null || nextClimbinPositions.Length == 0)
            return null;

        Transform closestPoint = null;
        float minDist = float.MaxValue;

        foreach (Transform point in nextClimbinPositions)
        {
            if (point != null)
            {
                float dist = Vector3.Distance(character.transform.position, point.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestPoint = point;
                }
            }
        }
        return closestPoint;
    }


    private bool IsOnMountainGround()
    {
        float raycastHeight = 0.5f;
        Vector3 rayOrigin = character.transform.position + Vector3.up * raycastHeight;
        int mountainMask = 1 << LayerMask.NameToLayer("MountainGround");
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 1f, mountainMask))
        {
            // Vérifie que le collider touché est bien celui de ce plateau
            if (hit.collider == plateauCollider)
            {
                return true;
            }
        }
        return false;
    }
}
