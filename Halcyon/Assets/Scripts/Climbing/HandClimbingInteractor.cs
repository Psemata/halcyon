using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

[RequireComponent(typeof(XRBaseInteractor))]
public class HandClimbingInteractor : MonoBehaviour
{
    [SerializeField] private XRBaseInteractor nearFarInteractor;
    [SerializeField] private AudioClip climbClip;

    private ClimbInteractable m_CurrentClimbInteractable;

    private void Awake()
    {
        if (!nearFarInteractor)
            nearFarInteractor = GetComponent<XRBaseInteractor>();

        nearFarInteractor.selectEntered.AddListener(TryStartClimbing);
        nearFarInteractor.selectExited.AddListener(StopClimbing);
    }

    private void OnDestroy()
    {
        if (nearFarInteractor != null)
        {
            nearFarInteractor.selectEntered.RemoveListener(TryStartClimbing);
            nearFarInteractor.selectExited.RemoveListener(StopClimbing);
        }
    }

    private void TryStartClimbing(SelectEnterEventArgs args)
    {
        if (!TryGetClimbInteractable(args, out var climbInteractable, out _)) return;

        m_CurrentClimbInteractable = climbInteractable;

        if (climbClip && AudioManager.Instance)
            AudioManager.Instance.PlaySFX(climbClip, transform.position);
    }

    private void StopClimbing(SelectExitEventArgs args)
    {
        if (!TryGetClimbInteractable(args, out var climbInteractable, out _)) return;

        if (nearFarInteractor.hasSelection &&
            nearFarInteractor.firstInteractableSelected as ClimbInteractable == m_CurrentClimbInteractable)
        {
            nearFarInteractor.interactionManager.SelectExit(
                nearFarInteractor as IXRSelectInteractor, m_CurrentClimbInteractable);
        }

        m_CurrentClimbInteractable = null;
    }

    private bool TryGetClimbInteractable(BaseInteractionEventArgs args, out ClimbInteractable interactable, out ClimbAttributes attributes)
    {
        interactable = null;
        attributes = null;
        if (!args.interactableObject.transform.TryGetComponent(out ClimbInteractable climbInteractable)) return false;
        if (!climbInteractable.transform.TryGetComponent(out ClimbAttributes climbAttributes)) return false;
        interactable = climbInteractable;
        attributes = climbAttributes;
        return true;
    }
}
