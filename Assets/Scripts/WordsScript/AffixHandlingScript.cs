using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AffixHandlingScript : MonoBehaviour
{
    private List<string> predefinedAffixes;
    private List<string> customAffixes;
    
    private const string CUSTOM_AFFIXES_KEY = "CustomAffixes";
    private const char AFFIX_SEPARATOR = '|';

    #region Initialization
    private void Awake() => InitializeAffixLists();

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
        var txt = Resources.Load<TextAsset>("ExternalFiles/affixes_list");
        
        if (txt != null)
        {
            using var reader = new System.IO.StringReader(txt.text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    predefinedAffixes.Add(line.Trim());
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
        string customAffixesString = PlayerPrefs.GetString(CUSTOM_AFFIXES_KEY, "");
        
        if (!string.IsNullOrEmpty(customAffixesString))
        {
            var customAffixesArray = customAffixesString.Split(AFFIX_SEPARATOR);
            foreach (string customAffix in customAffixesArray)
            {
                if (!string.IsNullOrWhiteSpace(customAffix))
                    customAffixes.Add(customAffix.Trim());
            }
        }
        Debug.Log($"Loaded {customAffixes.Count} custom affixes from PlayerPrefs");
    }
    #endregion

    #region Saving Methods
    private void SaveCustomAffixes()
    {
        string customAffixesString = string.Join(AFFIX_SEPARATOR.ToString(), customAffixes.ToArray());
        PlayerPrefs.SetString(CUSTOM_AFFIXES_KEY, customAffixesString);
        PlayerPrefs.Save();
        Debug.Log($"Saved {customAffixes.Count} custom affixes to PlayerPrefs");
    }
    #endregion

    #region Public Interface
    public List<string> GetAllAffixes()
    {
        var allAffixes = new List<string>();
        allAffixes.AddRange(predefinedAffixes);
        allAffixes.AddRange(customAffixes);
        return allAffixes;
    }

    public List<string> GetPredefinedAffixes() => new(predefinedAffixes);
    public List<string> GetCustomAffixes() => new(customAffixes);

    public bool AddCustomAffix(string newAffix)
    {
        if (string.IsNullOrWhiteSpace(newAffix))
        {
            Debug.LogWarning("Cannot add empty or whitespace custom affix");
            return false;
        }

        string normalizedAffix = newAffix.Trim().ToLower();

        if (IsAffixDuplicate(normalizedAffix))
        {
            Debug.LogWarning($"Affix '{normalizedAffix}' already exists. Cannot add duplicate.");
            return false;
        }

        customAffixes.Add(normalizedAffix);
        SaveCustomAffixes();
        Debug.Log($"Added new custom affix: '{normalizedAffix}'. Total custom affixes: {customAffixes.Count}");
        return true;
    }

    public bool RemoveCustomAffix(string affixToRemove)
    {
        if (string.IsNullOrWhiteSpace(affixToRemove)) return false;

        string normalizedAffix = affixToRemove.Trim().ToLower();
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
        if (string.IsNullOrWhiteSpace(affix)) return false;

        string normalizedAffix = affix.Trim().ToLower();
        return predefinedAffixes.Any(existingAffix => existingAffix.ToLower() == normalizedAffix) ||
               customAffixes.Any(existingCustomAffix => existingCustomAffix.ToLower() == normalizedAffix);
    }

    public void ClearAllCustomAffixes()
    {
        customAffixes.Clear();
        SaveCustomAffixes();
        Debug.Log("Cleared all custom affixes");
    }

    public void RefreshPredefinedAffixes() => LoadPredefinedAffixes();
    public int GetTotalAffixCount() => predefinedAffixes.Count + customAffixes.Count;
    public string GetAffixStatistics() => $"Predefined: {predefinedAffixes.Count}, Custom: {customAffixes.Count}, Total: {GetTotalAffixCount()}";
    #endregion
}
