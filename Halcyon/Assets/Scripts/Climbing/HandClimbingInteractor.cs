using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;

public class HandClimbingInteractor : MonoBehaviour
{
    [SerializeField] private XRBaseInteractor nearFarInteractor;
    [SerializeField] private ClimbProvider climbProvider;
    [SerializeField] private float maxEnergy = 100, energyConsumptionRate = 5;
    [SerializeField] private float minEnergyToClimb = 5f;

    private float m_CurrentEnergy;
    private bool m_IsClimbing;
    private ClimbInteractable m_CurrentClimbInteractable;
    private float m_ClimbingDifficulty;

    private void Awake()
    {
        OnValidate();
        nearFarInteractor.selectEntered.AddListener(TryStartClimbing);
        nearFarInteractor.selectExited.AddListener(StopClimbing);
    }

    private void Update()
    {
        // if (m_IsClimbing)
        // {
        //     ConsumeEnergy();
        //     if (!hasEnoughEnergy(energyConsumptionRate))
        //         ReleaseGrip();
        // }
    }

    private void TryStartClimbing(SelectEnterEventArgs arg0)
    {
        if (!arg0.interactableObject.transform.TryGetComponent(out ClimbInteractable climbInteractable)) return;
        if (!climbInteractable.transform.TryGetComponent(out ClimbAttributes climbAttributes)) return;

        // if (!hasEnoughEnergy(minEnergyToClimb))
        // {
        //     ReleaseGrip();
        //     return;
        // }

        m_IsClimbing = true;
        m_CurrentClimbInteractable = climbInteractable;
        m_ClimbingDifficulty = climbAttributes.ClimbingDifficulty;
    }

    private void StopClimbing(SelectExitEventArgs arg0)
    {
        if (!arg0.interactableObject.transform.TryGetComponent(out ClimbInteractable climbInteractable)) return;
        if (!climbInteractable.transform.TryGetComponent(out ClimbAttributes climbAttributes)) return;

        ReleaseGrip();
    }

    private void ReleaseGrip()
    {
        // if (nearFarInteractor.hasSelection && nearFarInteractor.firstInteractableSelected as ClimbInteractable == m_CurrentClimbInteractable)
        // {
        //     nearFarInteractor.interactionManager.SelectExit(nearFarInteractor as IXRSelectInteractor, m_CurrentClimbInteractable);
        // }

        m_IsClimbing = false;
        m_CurrentClimbInteractable = null;
    }

    private bool hasEnoughEnergy(float threshold) => m_CurrentEnergy > threshold;

    private void ConsumeEnergy()
    {
        var energyDrain = energyConsumptionRate * m_ClimbingDifficulty * Time.deltaTime;
        m_CurrentEnergy = Mathf.Max(0f, m_CurrentEnergy - energyDrain);
    }

    private void OnValidate()
    {
        if (!nearFarInteractor)
        {
            TryGetComponent(out nearFarInteractor);
        }
    }
}
