using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxClimbSource;
    [SerializeField] private AudioSource sfxBreathingSource;
    [SerializeField] private AudioSource sfxRotationSource;
    [SerializeField] private AudioSource sfxLoreSource;
    [SerializeField] private AudioSource ambianceInstrumentSource;
    [SerializeField] private AudioSource ambianceBirdSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void PlaySFXClimb(AudioClip clip, Vector3? position = null, float volume = 1f)
    {
        if (position == null)
        {
            sfxClimbSource.PlayOneShot(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, position.Value, volume);
        }
    }

    public void PlaySFXBreathing(AudioClip clip, Vector3? position = null, float volume = 1f)
    {
        if (position == null)
        {
            sfxBreathingSource.PlayOneShot(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, position.Value, volume);
        }
    }

    public void PlaySFXRotation(AudioClip clip, Vector3? position = null, float volume = 1f)
    {
        if (position == null)
        {
            sfxRotationSource.PlayOneShot(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, position.Value, volume);
        }
    }

    public void PlaySFXLore(AudioClip clip, Vector3? position = null, float volume = 1f)
    {
        if (position == null)
        {
            sfxLoreSource.PlayOneShot(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, position.Value, volume);
        }
    }

    public void PlayAmbianceInstrument(AudioClip clip, float volume = 1f)
    {
        ambianceInstrumentSource.clip = clip;
        ambianceInstrumentSource.volume = volume;
        ambianceInstrumentSource.Play();
    }

    public void PlayAmbianceBird(AudioClip clip, float volume = 1f)
    {
        ambianceBirdSource.clip = clip;
        ambianceBirdSource.volume = volume;
        ambianceBirdSource.Play();
    }
}
