using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GripDestruction : MonoBehaviour
{
    [Header("Fragments")]
    [SerializeField] private GameObject grip;

    [SerializeField] private GameObject[] fragmentsPrefabs;
    [SerializeField] private float destructionDelay = 2f;

    [SerializeField] private AudioClip[] breakingSounds;
    [SerializeField] private AudioClip[] fallingSounds;

    private Coroutine destructionCoroutine;

    public void StartDestructionCountdown(SelectEnterEventArgs args)
    {
        if (destructionCoroutine != null)
            StopCoroutine(destructionCoroutine);
        destructionCoroutine = StartCoroutine(DestructionRoutine(args));
    }

    private IEnumerator DestructionRoutine(SelectEnterEventArgs args)
    {
        yield return new WaitForSeconds(destructionDelay);

        grip.SetActive(false);

        AudioManager.Instance.PlaySFXClimb(breakingSounds[Random.Range(0, breakingSounds.Length)], grip.transform.position, 0.3f);

        foreach (var fragPrefab in fragmentsPrefabs)
        {
            fragPrefab.SetActive(true);

            var rb = fragPrefab.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                Vector3 randomDir = (fragPrefab.transform.position - grip.transform.position).normalized + Random.insideUnitSphere * 0.3f;
                float force = Random.Range(2f, 5f);
                rb.AddForce(randomDir.normalized * force, ForceMode.Impulse);
            }
        }

        AudioManager.Instance.PlaySFXClimb(fallingSounds[Random.Range(0, fallingSounds.Length)], grip.transform.position, 0.3f);

        // Deselect the grip
        args.manager.SelectExit(
            args.interactorObject as IXRSelectInteractor,
            args.interactableObject as IXRSelectInteractable);

        yield return new WaitForSeconds(1f);

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
