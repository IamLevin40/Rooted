using UnityEngine;  

public class WordInfo
{
    public string word { get; set; }
    public string definition { get; set; }
    public bool isEnviromental { get; set; }

    public WordInfo(string word, string definition, bool isEnviromental)
    {
        word = word;
        definition = definition;
        isEnviromental = isEnviromental;
    }
}

public static class WordManager : MonoBehaviour
{
    public static void loadWords()
    {

    }
}