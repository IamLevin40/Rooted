// Input is the word to be checked, which was made from WordBuildingScript.cs
// Must detect and return the following:
// (a.) Return True if the word is valid, False if not
//      Base this on the text file "valid_words.txt" under Resources/ExternalFiles
// (b.) Return the meaning if the word is valid
//      Base this on the csv file "glossary.csv" under Resources/ExternalFiles
// (c.) Return True if the word is environmentally-related, False if not.
//      Base this on the text file "environmental_words.txt" under Resources/ExternalFiles

using UnityEngine;

public class ValidWordDetectorScript : MonoBehaviour
{
    private WordManager wordManager;

    private void Start()
    {
        // Get reference to the WordManager instance
        wordManager = WordManager.Instance;
        
        if (wordManager == null)
        {
            Debug.LogError("WordManager instance not found! Make sure WordManager is in the scene.");
        }
    }

    /// <summary>
    /// (a.) Return True if the word is valid, False if not
    /// Base this on the text file "valid_words.txt" under Resources/ExternalFiles
    /// </summary>
    /// <param name="word">The word to check for validity</param>
    /// <returns>True if word is valid, False otherwise</returns>
    public bool IsValidWord(string word)
    {
        if (wordManager == null)
        {
            Debug.LogError("WordManager is not available. Cannot check word validity.");
            return false;
        }

        if (string.IsNullOrEmpty(word))
        {
            return false;
        }

        // Normalize the word (convert to lowercase and trim)
        string normalizedWord = word.ToLower().Trim();
        
        // Check if the word exists in the wordMap
        return wordManager.wordMap.ContainsKey(normalizedWord);
    }

    /// <summary>
    /// (b.) Return the meaning if the word is valid
    /// Base this on the glossary.json file under Resources/ExternalFiles
    /// </summary>
    /// <param name="word">The word to get the definition for</param>
    /// <returns>The definition of the word, or empty string if not found</returns>
    public string GetWordDefinition(string word)
    {
        if (wordManager == null)
        {
            Debug.LogError("WordManager is not available. Cannot get word definition.");
            return "";
        }

        if (string.IsNullOrEmpty(word))
        {
            return "";
        }

        // Normalize the word (convert to lowercase and trim)
        string normalizedWord = word.ToLower().Trim();
        
        // Check if the word exists in the wordMap and has a definition
        if (wordManager.wordMap.ContainsKey(normalizedWord))
        {
            WordInfo wordInfo = wordManager.wordMap[normalizedWord];
            return wordInfo.Definition ?? "";
        }

        return "";
    }

    /// <summary>
    /// (c.) Return True if the word is environmentally-related, False if not.
    /// Base this on the text file "environmental_words.txt" under Resources/ExternalFiles
    /// </summary>
    /// <param name="word">The word to check for environmental relevance</param>
    /// <returns>True if word is environmental, False otherwise</returns>
    public bool IsEnvironmentalWord(string word)
    {
        if (wordManager == null)
        {
            Debug.LogError("WordManager is not available. Cannot check if word is environmental.");
            return false;
        }

        if (string.IsNullOrEmpty(word))
        {
            return false;
        }

        // Normalize the word (convert to lowercase and trim)
        string normalizedWord = word.ToLower().Trim();
        
        // Check if the word exists in the wordMap and is marked as environmental
        if (wordManager.wordMap.ContainsKey(normalizedWord))
        {
            WordInfo wordInfo = wordManager.wordMap[normalizedWord];
            return wordInfo.IsEnviromental;
        }

        return false;
    }
}
