using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    private GameObject currentDraggedTile;
    private Canvas canvas;
    #endregion

    private void Start()
    {
        WordBuildingDisplay.SetActive(false);
        
        // Get canvas reference for dragging
        canvas = FindObjectOfType<Canvas>();
        
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
        SetupDropZones();
        
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
            
            // Add drag functionality to affix tile
            AddDragFunctionality(affixTile, affix, false);
        }
    }
    
    private void SetupDropZones()
    {
        // Add drag functionality to prefix and suffix tiles
        if (prefixTile != null)
        {
            AddDragFunctionality(prefixTile, "", true);
        }
        if (suffixTile != null)
        {
            AddDragFunctionality(suffixTile, "", true);
        }
    }
    
    private void AddDragFunctionality(GameObject tile, string affixText, bool isDropZone)
    {
        // Add drag handler
        DragHandler dragHandler = tile.GetComponent<DragHandler>();
        if (dragHandler == null)
        {
            dragHandler = tile.AddComponent<DragHandler>();
        }
        dragHandler.Initialize(this, affixText, isDropZone);
        
        // Add drop zone functionality if it's a prefix/suffix tile
        if (isDropZone)
        {
            DropZone dropZone = tile.GetComponent<DropZone>();
            if (dropZone == null)
            {
                dropZone = tile.AddComponent<DropZone>();
            }
            dropZone.Initialize(this, tile == prefixTile);
        }
    }
    
    public void StartDrag(GameObject draggedTile, string affixText, Vector2 startPosition)
    {
        // Create a clone for dragging
        currentDraggedTile = Instantiate(draggedTile, canvas.transform);
        currentDraggedTile.name = "DraggedTile_" + affixText;
        
        // Store original size before any modifications
        RectTransform originalRect = draggedTile.GetComponent<RectTransform>();
        RectTransform cloneRect = currentDraggedTile.GetComponent<RectTransform>();
        
        if (originalRect != null && cloneRect != null)
        {
            // Preserve the original size
            cloneRect.sizeDelta = originalRect.sizeDelta;
            cloneRect.anchorMin = originalRect.anchorMin;
            cloneRect.anchorMax = originalRect.anchorMax;
            cloneRect.pivot = originalRect.pivot;
        }
        
        // Remove any existing drag handlers from the clone to prevent conflicts
        DragHandler[] dragHandlers = currentDraggedTile.GetComponents<DragHandler>();
        for (int i = 0; i < dragHandlers.Length; i++)
        {
            DestroyImmediate(dragHandlers[i]);
        }
        
        DropZone[] dropZones = currentDraggedTile.GetComponents<DropZone>();
        for (int i = 0; i < dropZones.Length; i++)
        {
            DestroyImmediate(dropZones[i]);
        }
        
        // Make it non-interactable while dragging
        Button button = currentDraggedTile.GetComponent<Button>();
        if (button != null) button.interactable = false;
        
        // Set the text
        Text text = currentDraggedTile.GetComponentInChildren<Text>();
        if (text != null) 
        {
            text.text = affixText;
        }
        
        // Ensure the tile is active and visible
        currentDraggedTile.SetActive(true);
        
        // Make it follow the mouse and ensure it's visible
        currentDraggedTile.transform.SetAsLastSibling();
        
        // Position the cloned tile
        if (cloneRect != null)
        {
            Vector3 worldPos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform, 
                startPosition, 
                canvas.worldCamera, 
                out worldPos))
            {
                currentDraggedTile.transform.position = worldPos;
            }
            else
            {
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, 
                    startPosition, 
                    canvas.worldCamera, 
                    out localPos);
                cloneRect.localPosition = localPos;
            }
        }
        
        // Make it slightly transparent to show it's being dragged
        CanvasGroup canvasGroup = currentDraggedTile.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = currentDraggedTile.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false;
    }
    
    public void UpdateDrag(Vector2 position)
    {
        if (currentDraggedTile != null)
        {
            RectTransform rectTransform = currentDraggedTile.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, 
                    position, 
                    canvas.worldCamera, 
                    out localPos);
                rectTransform.localPosition = localPos;
            }
        }
    }
    
    public void EndDrag(bool wasDropped, bool isPrefix = false, string droppedAffix = "")
    {
        if (currentDraggedTile != null)
        {
            if (wasDropped)
            {
                // Set the prefix or suffix
                if (isPrefix)
                {
                    SetPrefix(droppedAffix);
                }
                else
                {
                    SetSuffix(droppedAffix);
                }
            }
            
            // Destroy the dragged tile
            Destroy(currentDraggedTile);
            currentDraggedTile = null;
        }
    }
    
    public void RemovePrefix()
    {
        SetPrefix("");
    }
    
    public void RemoveSuffix()
    {
        SetSuffix("");
    }
    // ...existing code...
}
