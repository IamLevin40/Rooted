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

    public WordInfo(string definition, bool isEnviromental)
    {
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

    private void LoadGlossaryWords()
    {
        string filePath = "ExternalFiles/" + FileGlossary;
        TextAsset file = Resources.Load<TextAsset>(filePath);

        if (file == null)
        {
            Debug.LogError($"File not found in Resources: {filePath}");
            return;
        }

        string jsonContent = file.text;
        Dictionary<string, string> glossaryData = ParseGlossaryJson(jsonContent);

        foreach (var data in glossaryData)
        {
            string word = data.Key.ToLower().Trim();
            string definition = data.Value;

            if (wordMap.ContainsKey(word))
            {
                wordMap[word].Definition = definition;
            }
            else
            {
                wordMap.Add(word, new WordInfo(definition));
            }
        }

    }

    private Dictionary<string, string> ParseGlossaryJson(string jsonContent)
    {
        Dictionary<string, string> result = new();

        try
        {
            jsonContent = jsonContent.Trim().TrimStart('{').TrimEnd('}');

            bool inQuotes = false;
            bool inValue = false;
            int braceCount = 0;
            string currentKey = "";
            string currentValue = "";
            string currentPart = "";

            for (int i = 0; i < jsonContent.Length; i++)
            {
                char character = jsonContent[i];

                if (character == '"' && (i == 0 || jsonContent[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                    if (!inQuotes && !inValue)
                    {
                        currentKey = currentPart.Trim();
                        currentPart = "";
                        continue;
                    }
                    else if (!inQuotes && inValue)
                    {
                        currentValue = currentPart.Trim();
                        result[currentKey] = currentValue;
                        currentKey = "";
                        currentValue = "";
                        currentPart = "";
                        inValue = false;
                        continue;
                    }
                }

                if (!inQuotes)
                {
                    if (character == ':' && !inValue)
                    {
                        inValue = true;
                        continue;
                    }
                    else if (character == ',' && !inValue && braceCount == 0)
                    {
                        continue;
                    }
                    else if (character == ' ' || character == '\t' || character == '\n' || character == '\r')
                    {
                        if (currentPart.Length > 0)
                            currentPart += character;
                        continue;
                    }
                }

                if (inQuotes || (!char.IsWhiteSpace(character) && character != ':' && character != ','))
                {
                    currentPart += character;
                }
            }

            if (!string.IsNullOrEmpty(currentKey) && !string.IsNullOrEmpty(currentValue))
            {
                result[currentKey] = currentValue;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing JSON: {e.Message}");

            try
            {
                result = ParseGlossaryJsonSimple(jsonContent);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Fallback parsing also failed: {ex.Message}");
            }
        }

        return result;
    }

    private Dictionary<string, string> ParseGlossaryJsonSimple(string jsonContent)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        string[] lines = jsonContent.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line == "{" || line == "}") continue;

            int colonIndex = FindColonIndex(line);
            if (colonIndex == -1) continue;

            string keyPart = line.Substring(0, colonIndex).Trim();
            string valuePart = line.Substring(colonIndex + 1).Trim();

            if (keyPart.StartsWith("\"") && keyPart.EndsWith("\""))
            {
                keyPart = keyPart.Substring(1, keyPart.Length - 2);
            }

            if (valuePart.StartsWith("\""))
            {
                valuePart = valuePart.Substring(1);
            }
            if (valuePart.EndsWith("\","))
            {
                valuePart = valuePart.Substring(0, valuePart.Length - 2);
            }
            else if (valuePart.EndsWith("\""))
            {
                valuePart = valuePart.Substring(0, valuePart.Length - 1);
            }

            if (!string.IsNullOrEmpty(keyPart))
            {
                result[keyPart] = valuePart;
            }
        }

        return result;
    }

    private int FindColonIndex(string line)
    {
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"' && (i == 0 || line[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ':' && !inQuotes)
            {
                return i;
            }
        }
        return -1;
    }
}