using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WordBuildingScript : MonoBehaviour
{
    #region UI References
    public GameObject WordBuildingDisplay;
    public PlayerScript player;
    
    [Header("Tiles")]
    public GameObject rootWordTile;
    public GameObject prefixTile;
    public GameObject suffixTile;
    
    [Header("Text Components")]
    public Text rootWordText;
    public Text prefixText;
    public Text suffixText;
    public Text meaningText;
    
    [Header("Submit Button")]
    public Button wordSubmitButton;
    
    [Header("Affix Tiles")]
    public GameObject affixContent;
    public GameObject affixTilePrefab;
    #endregion
    
    #region Word Data
    private string currentRootWord = "";
    private string currentPrefix = "";
    private string currentSuffix = "";
    private string currentMeaning = "";
    private List<string> affixesList;
    #endregion

    private void Start()
    {
        WordBuildingDisplay.SetActive(false);
        
        // Initialize tiles - prefix and suffix start inactive
        if (prefixTile != null) prefixTile.SetActive(false);
        if (suffixTile != null) suffixTile.SetActive(false);
        
        // Setup submit button
        if (wordSubmitButton != null)
        {
            wordSubmitButton.onClick.AddListener(OnWordSubmit);
        }
        
        // Load affixes and create tiles
        LoadAffixes();
        CreateAffixTiles();
        
        UpdateSubmitButtonState();
    }

    public void OnStartWordBuilding(string rootWord)
    {
        if (WordBuildingDisplay != null)
        {
            WordBuildingDisplay.SetActive(true);
        }

        // Set the root word
        currentRootWord = rootWord;
        if (rootWordText != null)
        {
            rootWordText.text = currentRootWord;
        }
        
        // Clear prefix and suffix
        currentPrefix = "";
        currentSuffix = "";
        currentMeaning = "";
        
        // Update UI
        UpdateWordDisplay();
        UpdateSubmitButtonState();
        
        Debug.Log($"Word building started with root word: {rootWord}");
    }

    public void OnEndWordBuilding()
    {
        if (WordBuildingDisplay != null)
        {
            WordBuildingDisplay.SetActive(false);
        }
        if (player != null)
        {
            player.EndWordBuilding();
        }
    }
    
    private void UpdateWordDisplay()
    {
        // Update prefix display
        if (prefixText != null)
        {
            prefixText.text = currentPrefix;
        }
        
        // Update suffix display
        if (suffixText != null)
        {
            suffixText.text = currentSuffix;
        }
        
        // Update meaning display
        if (meaningText != null)
        {
            meaningText.text = currentMeaning;
        }
    }
    
    private void UpdateSubmitButtonState()
    {
        if (wordSubmitButton != null)
        {
            // Can only submit if we have prefix and/or suffix (not just root word alone)
            bool canSubmit = !string.IsNullOrEmpty(currentPrefix) || !string.IsNullOrEmpty(currentSuffix);
            wordSubmitButton.interactable = canSubmit;
        }
    }
    
    public void OnWordSubmit()
    {
        // Combine prefix + root + suffix
        string combinedWord = currentPrefix + currentRootWord + currentSuffix;
        
        // For testing purposes, print the combined word
        Debug.Log($"Word submitted: {combinedWord} (Prefix: '{currentPrefix}', Root: '{currentRootWord}', Suffix: '{currentSuffix}')");
        
        // TODO: Add logic for word validation and scoring
        
        // End word building phase
        OnEndWordBuilding();
    }
    
    // Helper methods for adding prefix/suffix (to be called by UI or other systems)
    public void SetPrefix(string prefix)
    {
        currentPrefix = prefix;
        if (prefixTile != null) prefixTile.SetActive(!string.IsNullOrEmpty(prefix));
        UpdateWordDisplay();
        UpdateSubmitButtonState();
    }
    
    public void SetSuffix(string suffix)
    {
        currentSuffix = suffix;
        if (suffixTile != null) suffixTile.SetActive(!string.IsNullOrEmpty(suffix));
        UpdateWordDisplay();
        UpdateSubmitButtonState();
    }
    
    public void SetMeaning(string meaning)
    {
        currentMeaning = meaning;
        UpdateWordDisplay();
    }
    
    private void LoadAffixes()
    {
        affixesList = new List<string>();
        
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
                        affixesList.Add(line.Trim());
                }
            }
        }
        else
        {
            Debug.LogWarning("Could not load affixes_list.txt from Resources/ExternalFiles/");
        }
    }
    
    private void CreateAffixTiles()
    {
        if (affixContent == null || affixTilePrefab == null || affixesList == null) return;
        
        foreach (string affix in affixesList)
        {
            // Instantiate the affix tile prefab
            GameObject affixTile = Instantiate(affixTilePrefab, affixContent.transform);
            
            // Get the text component from the child
            Text affixText = affixTile.GetComponentInChildren<Text>();
            if (affixText != null)
            {
                affixText.text = affix;
            }
            
            // TODO: Add click functionality to affix tiles for selecting prefix/suffix
        }
    }
    // ...existing code...
}
