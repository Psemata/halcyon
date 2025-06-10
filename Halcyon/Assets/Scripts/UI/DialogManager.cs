using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private Canvas dialogCanvas;
    [SerializeField] private float animDuration = 1f;
    [SerializeField] private TMPro.TextMeshProUGUI dialogText;
    [SerializeField] private float charDisplaySpeed = 0.03f;
    [SerializeField] private bool isLore = true;
    [SerializeField] private AudioClip[] voiceSounds;

    private CanvasGroup canvasGroup;
    private LoreEntry selectedStory;

    void Awake()
    {
        if (dialogCanvas != null)
        {
            dialogCanvas.gameObject.SetActive(false);
            canvasGroup = dialogCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = dialogCanvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }
    }

    void Start()
    {
        selectedStory = LoreEntryConst.Entries[UnityEngine.Random.Range(0, LoreEntryConst.Entries.Length)];
        while (selectedStory.type != isLore)
        {
            selectedStory = LoreEntryConst.Entries[UnityEngine.Random.Range(0, LoreEntryConst.Entries.Length)];
        }
    }

    public void ShowDialog(SelectEnterEventArgs args)
    {
        if (dialogCanvas == null || dialogText == null) return;
        if (!dialogCanvas.gameObject.activeSelf)
        {
            dialogText.text = "";
            dialogCanvas.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(FadeInDialog());
        }
    }

    private IEnumerator FadeInDialog()
    {
        string story = selectedStory.story;
        float elapsed = 0f;
        canvasGroup.alpha = 0f;

        while (elapsed < animDuration)
        {
            float t = elapsed / animDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return StartCoroutine(DisplayText(story));
    }

    private IEnumerator FadeOutDialog()
    {
        float elapsed = 0f;
        canvasGroup.alpha = 1f;

        while (elapsed < animDuration)
        {
            float t = elapsed / animDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;

        yield return new WaitForSeconds(1f);
        dialogCanvas.gameObject.SetActive(false);
    }

    private IEnumerator DisplayText(string story)
    {
        string[] phrases = story.Split('|');
        int idx = Random.Range(0, voiceSounds.Length);
        AudioManager.Instance.PlaySFXLore(voiceSounds[idx], this.transform.position, 0.5f);
        for (int p = 0; p < phrases.Length; p++)
        {
            dialogText.text = "";
            string phrase = phrases[p].Trim();
            for (int i = 0; i < phrase.Length; i++)
            {
                dialogText.text = phrase.Substring(0, i + 1);
                yield return new WaitForSeconds(charDisplaySpeed);
            }

            if (p < phrases.Length - 1)
            {
                yield return new WaitForSeconds(1f);
                dialogText.text = "";
            }
        }

        GameManager.Instance.AddLoreLine(selectedStory.story);
        yield return new WaitForSeconds(5f); // Pause finale avant de cacher la boxe
        StartCoroutine(FadeOutDialog());
    }

}
