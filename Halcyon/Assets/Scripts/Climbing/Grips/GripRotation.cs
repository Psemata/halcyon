using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GripRotation : MonoBehaviour
{
    [Header("Mountain Rotation")]
    [SerializeField] private GameObject mountainLevel;
    [SerializeField] private GameObject button;
    [SerializeField] private GameObject currentPosition;
    [SerializeField] private GameObject nextPosition;

    [SerializeField] private AudioClip rotationClip;

    private Coroutine moveCoroutine;
    private bool isAnimatingPressed = false;
    private bool isAnimatingReleased = false;
    private bool isRotating = false;

    public void ButtonPressed(SelectEnterEventArgs args)
    {
        if (isAnimatingPressed) return;
        isAnimatingPressed = true;
        isAnimatingReleased = false;
        StartMoveAnimation(button.transform, new Vector3(0, -0.1f, 0));
        RotateMountain();
    }

    public void ButtonReleased(SelectExitEventArgs args)
    {
        if (isAnimatingReleased) return;
        isAnimatingReleased = true;
        isAnimatingPressed = false;
        StartMoveAnimation(button.transform, new Vector3(0, 0.1f, 0));
    }

    private void StartMoveAnimation(Transform target, Vector3 direction)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimateMove(target, direction));
    }

    private IEnumerator AnimateMove(Transform target, Vector3 direction)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startPos = target.localPosition;
        Vector3 endPos = startPos + direction;

        while (elapsed < duration)
        {
            target.localPosition = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localPosition = endPos;
    }

    public void RotateMountain()
    {
        if (isRotating) return;
        isRotating = true;

        Vector3 center = Vector3.zero;
        Vector3 flattenedCurrent = new Vector3(currentPosition.transform.position.x, 0, currentPosition.transform.position.z);
        Vector3 flattenedNext = new Vector3(nextPosition.transform.position.x, 0, nextPosition.transform.position.z);
        Vector3 from = (flattenedCurrent - center).normalized;
        Vector3 to = (flattenedNext - center).normalized;

        float angle = Vector3.Angle(from, to);

        if (button.name.Contains("Left"))
        {
            angle = -angle; // Invert angle for left button
        }

        StartCoroutine(RotateMountainCoroutine(angle, 10f));
    }

    private IEnumerator RotateMountainCoroutine(float angle, float duration)
    {
        Quaternion startRot = mountainLevel.transform.localRotation;
        Quaternion endRot = startRot * Quaternion.AngleAxis(angle, Vector3.forward);

        AudioManager.Instance.PlaySFXRotation(rotationClip, null, 0.7f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            mountainLevel.transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mountainLevel.transform.localRotation = endRot;
        isRotating = false;
    }
}
