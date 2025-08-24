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
        // Initialize tiles - prefix and suffix start with components disabled
        if (prefixTile != null) SetTileComponentsEnabled(prefixTile, false);
        if (suffixTile != null) SetTileComponentsEnabled(suffixTile, false);
        WordBuildingDisplay.SetActive(false);

        // Get canvas reference for dragging
        canvas = FindObjectOfType<Canvas>();

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
        if (prefixTile != null) SetTileComponentsEnabled(prefixTile, false);
        if (suffixTile != null) SetTileComponentsEnabled(suffixTile, false);
        OnEndWordBuilding();
    }

    public void SetPrefix(string prefix)
    {
        currentPrefix = prefix ?? "";
        UpdateAffixDisplay(prefixTile, prefixText, currentPrefix);
        UpdateSubmitButtonState();
    }

    public void SetSuffix(string suffix)
    {
        currentSuffix = suffix ?? "";
        UpdateAffixDisplay(suffixTile, suffixText, currentSuffix);
        UpdateSubmitButtonState();
    }

    private void UpdateAffixDisplay(GameObject tile, Text displayText, string affixValue)
    {
        bool hasAffix = !string.IsNullOrEmpty(affixValue);
        
        if (displayText != null)
            displayText.text = affixValue;
            
        if (tile != null)
        {
            SetTileComponentsEnabled(tile, hasAffix);
            var tileText = tile.GetComponentInChildren<Text>(true);
            if (tileText != null)
                tileText.text = affixValue;
        }
    }

    private void SetTileComponentsEnabled(GameObject tile, bool enabled)
    {
        if (tile == null) return;
        var image = tile.GetComponent<Image>();
        if (image != null)
        {
            Color c = image.color;
            c.a = enabled ? 1f : 0f;
            image.color = c;
        }
        var button = tile.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = enabled;
        }
        var text = tile.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            Color c = text.color;
            c.a = enabled ? 1f : 0f;
            text.color = c;
        }
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
        HideOriginalTileIfNeeded(draggedTile);
        CreateDraggedTileClone(draggedTile, affixText, startPosition);
    }

    private void HideOriginalTileIfNeeded(GameObject draggedTile)
    {
        if (draggedTile == prefixTile || draggedTile == suffixTile)
        {
            SetTileComponentsEnabled(draggedTile, false);
        }
    }

    private void CreateDraggedTileClone(GameObject originalTile, string affixText, Vector2 startPosition)
    {
        currentDraggedTile = Instantiate(originalTile, canvas.transform);
        currentDraggedTile.name = "DraggedTile_" + affixText;

        SetupDraggedTileTransform(originalTile, startPosition);
        CleanupDraggedTileComponents();
        ConfigureDraggedTileAppearance(affixText);
        SetupDraggedTileVisibility();
    }

    private void SetupDraggedTileTransform(GameObject originalTile, Vector2 startPosition)
    {
        RectTransform originalRect = originalTile.GetComponent<RectTransform>();
        RectTransform cloneRect = currentDraggedTile.GetComponent<RectTransform>();

        if (originalRect != null && cloneRect != null)
        {
            cloneRect.sizeDelta = originalRect.sizeDelta;
            cloneRect.anchorMin = originalRect.anchorMin;
            cloneRect.anchorMax = originalRect.anchorMax;
            cloneRect.pivot = originalRect.pivot;

            PositionDraggedTile(cloneRect, startPosition);
        }
    }

    private void PositionDraggedTile(RectTransform cloneRect, Vector2 startPosition)
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

    private void CleanupDraggedTileComponents()
    {
        var dragHandlers = currentDraggedTile.GetComponents<DragHandler>();
        var dropZones = currentDraggedTile.GetComponents<DropZone>();

        foreach (var handler in dragHandlers)
            DestroyImmediate(handler);
        foreach (var zone in dropZones)
            DestroyImmediate(zone);
    }

    private void ConfigureDraggedTileAppearance(string affixText)
    {
        var button = currentDraggedTile.GetComponent<Button>();
        if (button != null) button.interactable = false;

        var text = currentDraggedTile.GetComponentInChildren<Text>();
        if (text != null) text.text = affixText;

        currentDraggedTile.SetActive(true);
        SetTileComponentsEnabled(currentDraggedTile, true);
    }

    private void SetupDraggedTileVisibility()
    {
        currentDraggedTile.transform.SetAsLastSibling();

        var canvasGroup = currentDraggedTile.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = currentDraggedTile.AddComponent<CanvasGroup>();
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
                if (isPrefix)
                    SetPrefix(droppedAffix);
                else
                    SetSuffix(droppedAffix);
            }

            Destroy(currentDraggedTile);
            currentDraggedTile = null;
        }
    }

    public void RemoveAffix(GameObject affixTile)
    {
        if (affixTile == prefixTile)
            SetPrefix("");
        else if (affixTile == suffixTile)
            SetSuffix("");
    }
}
