using System.Collections.Generic;
using System.IO;
using UnityEngine;  


[System.Serializable]
public class WordInfo
{
    public string Word { get; set; }
    public string Definition { get; set; }
    public bool IsEnviromental { get; set; }

    public WordInfo(string word, string definition, bool isEnviromental)
    {
        Word = word;
        Definition = definition;
        IsEnviromental = isEnviromental;
    }
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

    private void LoadValidWords()
    {
        // pass
    }
}