using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GripSounds : MonoBehaviour
{

    [SerializeField] private AudioClip[] gripSounds;

    public void GripSound(SelectEnterEventArgs args)
    {
        if (gripSounds != null && gripSounds.Length > 0)
        {
            int idx = Random.Range(0, gripSounds.Length);
            AudioManager.Instance.PlaySFXClimb(gripSounds[idx], this.transform.position, 0.5f);
        }
    }
}
