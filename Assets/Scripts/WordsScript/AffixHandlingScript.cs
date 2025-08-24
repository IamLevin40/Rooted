using UnityEngine;
using System.Collections.Generic;

public class AffixHandlingScript : MonoBehaviour
{
    [Header("Settings")]
    private List<string> predefinedAffixes;
    private List<string> customAffixes;
    
    private const string CUSTOM_AFFIXES_PLAYERPREFS_KEY = "CustomAffixes";
    private const char AFFIX_SEPARATOR = '|';

    #region Initialization
    private void Awake()
    {
        InitializeAffixLists();
    }

    private void InitializeAffixLists()
    {
        predefinedAffixes = new List<string>();
        customAffixes = new List<string>();
        
        LoadPredefinedAffixes();
        LoadCustomAffixes();
    }
    #endregion

    #region Loading Methods
    private void LoadPredefinedAffixes()
    {
        predefinedAffixes.Clear();

        // Load from Resources/ExternalFiles/affixes_list.txt
        TextAsset txt = Resources.Load<TextAsset>("ExternalFiles/affixes_list");
        if (txt != null)
        {
            using (System.IO.StringReader reader = new System.IO.StringReader(txt.text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        predefinedAffixes.Add(line.Trim());
                }
            }
            Debug.Log($"Loaded {predefinedAffixes.Count} predefined affixes from file");
        }
        else
        {
            Debug.LogWarning("Could not load affixes_list.txt from Resources/ExternalFiles/");
        }
    }

    private void LoadCustomAffixes()
    {
        customAffixes.Clear();

        // Load custom affixes from PlayerPrefs
        string customAffixesString = PlayerPrefs.GetString(CUSTOM_AFFIXES_PLAYERPREFS_KEY, "");
        
        if (!string.IsNullOrEmpty(customAffixesString))
        {
            string[] customAffixesArray = customAffixesString.Split(AFFIX_SEPARATOR);
            foreach (string customAffix in customAffixesArray)
            {
                if (!string.IsNullOrWhiteSpace(customAffix))
                {
                    customAffixes.Add(customAffix.Trim());
                }
            }
        }

        Debug.Log($"Loaded {customAffixes.Count} custom affixes from PlayerPrefs");
    }
    #endregion

    #region Saving Methods
    private void SaveCustomAffixes()
    {
        // Save custom affixes to PlayerPrefs
        string customAffixesString = string.Join(AFFIX_SEPARATOR.ToString(), customAffixes.ToArray());
        PlayerPrefs.SetString(CUSTOM_AFFIXES_PLAYERPREFS_KEY, customAffixesString);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved {customAffixes.Count} custom affixes to PlayerPrefs");
    }
    #endregion

    #region Public Interface
    public List<string> GetAllAffixes()
    {
        List<string> allAffixes = new List<string>();
        allAffixes.AddRange(predefinedAffixes);
        allAffixes.AddRange(customAffixes);
        return allAffixes;
    }

    public List<string> GetPredefinedAffixes()
    {
        return new List<string>(predefinedAffixes);
    }

    public List<string> GetCustomAffixes()
    {
        return new List<string>(customAffixes);
    }

    public bool AddCustomAffix(string newAffix)
    {
        if (string.IsNullOrWhiteSpace(newAffix))
        {
            Debug.LogWarning("Cannot add empty or whitespace custom affix");
            return false;
        }

        // Convert to uppercase for consistency
        string normalizedAffix = newAffix.Trim().ToUpper();

        // Check for duplicates
        if (IsAffixDuplicate(normalizedAffix))
        {
            Debug.LogWarning($"Affix '{normalizedAffix}' already exists. Cannot add duplicate.");
            return false;
        }

        // Add the new custom affix
        customAffixes.Add(normalizedAffix);
        
        // Save to PlayerPrefs
        SaveCustomAffixes();
        
        Debug.Log($"Added new custom affix: '{normalizedAffix}'. Total custom affixes: {customAffixes.Count}");
        return true;
    }

    public bool RemoveCustomAffix(string affixToRemove)
    {
        if (string.IsNullOrWhiteSpace(affixToRemove))
            return false;

        string normalizedAffix = affixToRemove.Trim().ToUpper();
        bool removed = customAffixes.Remove(normalizedAffix);
        
        if (removed)
        {
            SaveCustomAffixes();
            Debug.Log($"Removed custom affix: '{normalizedAffix}'. Total custom affixes: {customAffixes.Count}");
        }
        else
        {
            Debug.LogWarning($"Custom affix '{normalizedAffix}' not found for removal");
        }

        return removed;
    }

    public bool IsAffixDuplicate(string affix)
    {
        if (string.IsNullOrWhiteSpace(affix))
            return false;

        string normalizedAffix = affix.Trim().ToUpper();

        // Check predefined affixes
        foreach (string existingAffix in predefinedAffixes)
        {
            if (existingAffix.ToUpper() == normalizedAffix)
            {
                return true;
            }
        }

        // Check custom affixes
        foreach (string existingCustomAffix in customAffixes)
        {
            if (existingCustomAffix.ToUpper() == normalizedAffix)
            {
                return true;
            }
        }

        return false;
    }

    public void ClearAllCustomAffixes()
    {
        customAffixes.Clear();
        SaveCustomAffixes();
        Debug.Log("Cleared all custom affixes");
    }

    public void RefreshPredefinedAffixes()
    {
        LoadPredefinedAffixes();
    }

    public int GetTotalAffixCount()
    {
        return predefinedAffixes.Count + customAffixes.Count;
    }

    public string GetAffixStatistics()
    {
        return $"Predefined: {predefinedAffixes.Count}, Custom: {customAffixes.Count}, Total: {GetTotalAffixCount()}";
    }
    #endregion
}
