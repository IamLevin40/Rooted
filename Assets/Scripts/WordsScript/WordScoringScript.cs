using UnityEngine;
using System.Collections.Generic;

public class WordScoringScript : MonoBehaviour
{
    private static Dictionary<char, int> letterScores = new Dictionary<char, int>
    {
        {'A', 1}, {'B', 3}, {'C', 3}, {'D', 2}, {'E', 1}, {'F', 4}, {'G', 2},
        {'H', 4}, {'I', 1}, {'J', 8}, {'K', 5}, {'L', 1}, {'M', 3}, {'N', 1},
        {'O', 1}, {'P', 3}, {'Q', 10}, {'R', 1}, {'S', 1}, {'T', 1}, {'U', 1},
        {'V', 4}, {'W', 4}, {'X', 8}, {'Y', 4}, {'Z', 10}
    };

    public static int CalculateWordScore(string word)
    {
        if (string.IsNullOrEmpty(word)) return 0;
        
        int totalScore = 0;
        foreach (char letter in word.ToUpper())
        {
            if (letterScores.ContainsKey(letter))
            {
                totalScore += letterScores[letter];
            }
        }
        return totalScore;
    }

    public static int CalculateDamage(string word)
    {
        if (string.IsNullOrEmpty(word)) return 0;
        return word.Length;
    }

    public static bool IsValidWord(string word)
    {
        // For now, assume all words are valid
        // This will be replaced with actual word validation logic later
        return !string.IsNullOrEmpty(word) && word.Length > 1;
    }

    public static bool IsEnvironmentalWord(string word)
    {
        // For now, assume all words are environmental-related
        // This will be replaced with actual validation logic later
        return true;
    }
}
