using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LoreBook : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> loreTexts;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLoreLinesChanged += UpdateLoreDisplay;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLoreLinesChanged -= UpdateLoreDisplay;
    }

    private void UpdateLoreDisplay(List<string> lines)
    {
        if (lines == null || loreTexts.Count < 2) return;

        loreTexts[0].text = "";
        loreTexts[1].text = "";

        float maxHeightLeft = ((RectTransform)loreTexts[0].transform).rect.height;
        float maxHeightRight = ((RectTransform)loreTexts[1].transform).rect.height;

        string leftText = "";
        string rightText = "";
        bool fillingLeft = true;

        foreach (var line in lines)
        {
            if (fillingLeft)
            {
                string test = leftText.Length > 0 ? leftText + "\n" + line : line;
                var size = loreTexts[0].GetPreferredValues(test);
                if (size.y > maxHeightLeft)
                {
                    fillingLeft = false;
                    rightText = line;
                }
                else
                {
                    leftText = test;
                }
            }
            else
            {
                rightText = rightText.Length > 0 ? rightText + "\n" + line : line;
            }
        }

        loreTexts[0].text = leftText;
        loreTexts[1].text = rightText;
    }
}
