using System;
using System.Collections.Generic;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance { get; private set; }

    [SerializeField] private Transform leftPage;
    [SerializeField] private Transform rightPage;

    [SerializeField] private GameObject textLinePrefab;

    private List<LoreEntry> loreLines;

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
        this.loreLines = new List<LoreEntry>();
    }

    public void HandleNewLoreLine(LoreEntry line)
    {
        if (loreLines.Contains(line))
            return;

        loreLines.Add(line);

        Transform parent = line.type ? leftPage : rightPage;
        GameObject instance = Instantiate(textLinePrefab, parent);
        instance.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = line.story;
    }
}
