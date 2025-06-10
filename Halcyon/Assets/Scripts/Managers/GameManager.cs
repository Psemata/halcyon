using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject character;

    [SerializeField] private AudioClip[] windClips;
    [SerializeField] private AudioClip[] birdsClips;
    [SerializeField] private AudioClip[] instrumentsClips;
    [SerializeField] private AudioClip[] instrumentsHighAltClips;
    [SerializeField] private AudioClip[] breathingClips;
    [SerializeField] private AudioClip[] breathingColdClips;

    private List<string> loreLines;

    public event Action<List<string>> OnLoreLinesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        this.loreLines = new List<string>();
        StartCoroutine(PlayRandomWindLoop());
        StartCoroutine(PlayRandomBirdsLoop());
        StartCoroutine(PlayRandomInstruments());
        StartCoroutine(PlayRandomBreathing());
    }

    public void AddLoreLine(string line)
    {
        if (!this.loreLines.Contains(line))
        {
            this.loreLines.Add(line);
            OnLoreLinesChanged?.Invoke(this.loreLines);
        }
    }

    private IEnumerator PlayRandomWindLoop()
    {
        while (true)
        {
            if (windClips != null && windClips.Length > 0)
            {
                int idx = UnityEngine.Random.Range(0, windClips.Length);
                AudioManager.Instance.PlayMusic(windClips[idx], 0.5f);
                yield return new WaitForSeconds(windClips[idx].length);
            }
            else
            {
                yield break;
            }
        }
    }

    private IEnumerator PlayRandomBirdsLoop()
    {
        while (true)
        {
            if (birdsClips != null && birdsClips.Length > 0)
            {
                int idx = UnityEngine.Random.Range(0, birdsClips.Length);
                AudioManager.Instance.PlayAmbianceBird(birdsClips[idx], 0.6f);
                yield return new WaitForSeconds(birdsClips[idx].length);
                yield return new WaitForSeconds(UnityEngine.Random.Range(30f, 80f));
            }
            else
            {
                yield break;
            }
        }
    }

    private IEnumerator PlayRandomInstruments()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(120f, 150f));
            if (this.character.transform.position.y > 250f)
            {
                if (instrumentsHighAltClips != null && instrumentsHighAltClips.Length > 0)
                {
                    int idx = UnityEngine.Random.Range(0, instrumentsHighAltClips.Length);
                    AudioManager.Instance.PlayAmbianceInstrument(instrumentsHighAltClips[idx], 0.3f);
                    yield return new WaitForSeconds(instrumentsHighAltClips[idx].length);
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                if (instrumentsClips != null && instrumentsClips.Length > 0)
                {
                    int idx = UnityEngine.Random.Range(0, instrumentsClips.Length);
                    AudioManager.Instance.PlayAmbianceInstrument(instrumentsClips[idx], 0.3f);
                    yield return new WaitForSeconds(instrumentsClips[idx].length);
                }
                else
                {
                    yield break;
                }
            }
        }
    }

    private IEnumerator PlayRandomBreathing()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(120f, 150f));
            if (this.character.transform.position.y > 250f)
            {
                if (breathingColdClips != null && breathingColdClips.Length > 0)
                {
                    int idx = UnityEngine.Random.Range(0, breathingColdClips.Length);
                    AudioManager.Instance.PlaySFXBreathing(breathingColdClips[idx], null, 0.3f);
                    yield return new WaitForSeconds(breathingColdClips[idx].length);
                }
                else
                {
                    yield break;
                }
            }
            else
            {
                if (breathingClips != null && breathingClips.Length > 0)
                {
                    int idx = UnityEngine.Random.Range(0, breathingClips.Length);
                    AudioManager.Instance.PlaySFXBreathing(breathingClips[idx], null, 0.3f);
                    yield return new WaitForSeconds(breathingClips[idx].length);
                }
                else
                {
                    yield break;
                }
            }
        }
    }
}
