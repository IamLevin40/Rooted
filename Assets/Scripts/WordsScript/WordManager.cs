using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class WordInfo
{
    public string Definition { get; set; }
    public bool IsEnviromental { get; set; }

    public WordInfo() { }

    public WordInfo(string definition)
    {
        Definition = definition;
        IsEnviromental = false;
    }

    public WordInfo(bool isEnviromental)
    {
        Definition = null;
        IsEnviromental = isEnviromental;
    }

    public WordInfo(string definition, bool isEnviromental)
    {
        Definition = definition;
        IsEnviromental = isEnviromental;
    }
}

[System.Serializable]
public class GlossaryEntry
{
    public string word;
    public string definition;
}

[System.Serializable]
public class GlossaryWrapper
{
    public List<GlossaryEntry> entries;
}


public class WordManager : MonoBehaviour
{
    public static WordManager Instance { get; private set; }
    public Dictionary<string, WordInfo> wordMap = new();

    [SerializeField] private string FileValidWords;
    [SerializeField] private string FileEnvironmentalWords;
    [SerializeField] private string FileGlossary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        LoadValidWords();
        LoadGlossaryWords();
        LoadEnvironmentalWords();

        Debug.Log($"Total words stored: {wordMap.Keys.Count}");
    }

    private void LoadValidWords()
    {
        string filePath = "ExternalFiles/" + FileValidWords;
        TextAsset file = Resources.Load<TextAsset>(filePath);

        if (file == null)
        {
            Debug.LogError($"File not found in Resources: {filePath}");
            return;
        }

        string[] rawLines = file.text.Split("\n");
        foreach (string line in rawLines)
        {
            string word = line.Trim();

            if (string.IsNullOrEmpty(word) || wordMap.ContainsKey(word)) continue;
            wordMap.Add(word, new WordInfo());
        }
    }

    private void LoadEnvironmentalWords()
    {
        string filePath = "ExternalFiles/" + FileEnvironmentalWords;
        TextAsset file = Resources.Load<TextAsset>(filePath);

        if (file == null)
        {
            Debug.LogError($"File not found in Resources: {filePath}");
            return;
        }

        string[] rawLines = file.text.Split("\n");
        foreach (string line in rawLines)
        {
            string word = line.Trim();

            if (string.IsNullOrEmpty(word)) continue;

            if (wordMap.ContainsKey(word))
            {
                wordMap[word].IsEnviromental = true;
            }
            else
            {
                wordMap.Add(word, new WordInfo(true));
            }
        }
    }

    private void LoadGlossaryWords()
    {
        string filePath = "ExternalFiles/" + FileGlossary;
        TextAsset file = Resources.Load<TextAsset>(filePath);

        if (file == null)
        {
            Debug.LogError($"File not found in Resources: {filePath}");
            return;
        }

        GlossaryWrapper wrapper = JsonUtility.FromJson<GlossaryWrapper>(file.text);
        foreach (var entry in wrapper.entries)
        {
            if (wordMap.ContainsKey(entry.word))
            {
                wordMap[entry.word].Definition = entry.definition;
            }
            else
            {
                wordMap.Add(entry.word, new WordInfo(entry.definition));
            }
        }
    }
}
