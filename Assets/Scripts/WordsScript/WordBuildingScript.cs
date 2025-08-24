using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WordBuildingScript : MonoBehaviour
{
    #region UI References
    public GameObject WordBuildingDisplay;
    public PlayerScript player;
    public EnemyScript enemy;
    public ValidWordDetectorScript validWordDetector;

    [Header("Tiles")]
    public GameObject rootWordTile;
    public GameObject prefixTile;
    public GameObject suffixTile;

    [Header("Text Components")]
    public Text rootWordText;
    public Text prefixText;
    public Text suffixText;
    public Text definitionText;

    [Header("Submit Button")]
    public Button wordSubmitButton;

    [Header("Timer")]
    public Slider timerBar;
    public Text timerText;
    public float maxTime = 20f;

    [Header("Affix Tiles")]
    public GameObject affixContent;
    public GameObject affixTilePrefab;

    [Header("Panel Management")]
    public GameObject yourTiles;
    public GameObject createCustomTiles;
    public Button createCustomTileButton;
    public Button backButton;
    public CustomTileCreatingScript customTileCreating;
    #endregion

    #region Word Data
    private string currentRootWord = "";
    private string currentPrefix = "";
    private string currentSuffix = "";
    private string currentDefinition = "";
    private List<string> affixesList;
    private List<string> customAffixesList; // Store custom affixes separately
    private GameObject currentDraggedTile;
    private Canvas canvas;
    private float currentTime;
    private bool timerRunning = false;
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

        // Setup panel management buttons
        if (createCustomTileButton != null)
        {
            createCustomTileButton.onClick.AddListener(OnCreateCustomTileClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // Set default panel states
        SetPanelStates(true); // Show yourTiles by default

        // Load affixes and create tiles
        LoadAffixes();
        CreateAffixTiles();
        SetupDropZones();

        UpdateSubmitButtonState();
    }

    private void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTime <= 0f)
            {
                OnTimerExpired();
            }
        }
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
        currentDefinition = "";

        // Update UI
        UpdateWordDisplay();
        UpdateSubmitButtonState();
        UpdatePlayerWord();

        // Start timer
        StartTimer();

        Debug.Log($"Word building started with root word: {rootWord}");
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

        // Update definition display
        if (definitionText != null)
        {
            definitionText.text = currentDefinition;
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
        string combinedWord = currentPrefix + currentRootWord + currentSuffix;

        // Check if the word is valid first
        bool isValidWord = validWordDetector.IsValidWord(combinedWord);

        if (isValidWord)
        {
            if (WordBuildingDisplay != null) WordBuildingDisplay.SetActive(false);

            // Use the queue values instead of recalculating
            float finalDamage = player.queueDamage;
            int finalScore = player.queueScore;

            // Spawn projectile instead of immediately applying damage/score
            if (player != null) player.SpawnPlayerProjectile(finalDamage, finalScore);
        }
        else
        {
            Debug.Log($"Invalid word: {combinedWord} - No projectile spawned");
            return;
        }

        // Stop the timer
        StopTimer();

        // Reset tiles (but don't end word building yet for valid words - projectile will handle that)
        if (prefixTile != null) SetTileComponentsEnabled(prefixTile, false);
        if (suffixTile != null) SetTileComponentsEnabled(suffixTile, false);
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

        UpdatePlayerWord();
    }

    private void UpdatePlayerWord()
    {
        if (player != null)
        {
            player.queueDamage = 0;
            player.queueScore = 0;

            if (string.IsNullOrEmpty(currentPrefix) && string.IsNullOrEmpty(currentSuffix))
            {
                Debug.Log("Only root word displayed.");
                return;
            }

            string playWord = currentPrefix + currentRootWord + currentSuffix;
            bool isValidWord = validWordDetector.IsValidWord(playWord);

            if (!isValidWord)
            {
                Debug.Log($"Invalid word: {playWord}");
                return;
            }

            player.playWord = playWord;
            Debug.Log($"Player word updated: {playWord}");

            CalculateAndUpdateQueueValues(playWord);
            Debug.Log($"Queue Damage: {player.queueDamage}, Queue Score: {player.queueScore} for word: {playWord}; Environmental: {player.isEnvironmentalWord}");
        }
    }

    private void CalculateAndUpdateQueueValues(string playWord)
    {
        string wordDefinition = validWordDetector.GetWordDefinition(playWord);
        SetDefinition(wordDefinition);

        bool isEnvironmentalWord = validWordDetector.IsEnvironmentalWord(playWord);

        // Calculate base damage and score
        int baseDamage = WordScoringScript.CalculateDamageToEnemy(playWord);
        int baseScore = WordScoringScript.CalculateWordScore(playWord);

        // Apply multiplier for environmental words
        int finalDamage = isEnvironmentalWord ? baseDamage * 2 : baseDamage;
        int finalScore = isEnvironmentalWord ? baseScore * 2 : baseScore;

        player.queueDamage = finalDamage;
        player.queueScore = finalScore;
        player.isEnvironmentalWord = isEnvironmentalWord;
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

    public void SetDefinition(string definition)
    {
        currentDefinition = definition;
        UpdateWordDisplay();
    }

    private void LoadAffixes()
    {
        affixesList = new List<string>();
        customAffixesList = new List<string>();

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

        // Load custom affixes from PlayerPrefs
        LoadCustomAffixes();
    }

    private void CreateAffixTiles()
    {
        if (affixContent == null || affixTilePrefab == null) return;

        // Clear existing tiles first
        ClearAffixTiles();

        // Create tiles for predefined affixes
        if (affixesList != null)
        {
            foreach (string affix in affixesList)
            {
                CreateSingleAffixTile(affix);
            }
        }

        // Create tiles for custom affixes
        if (customAffixesList != null)
        {
            foreach (string customAffix in customAffixesList)
            {
                CreateSingleAffixTile(customAffix);
            }
        }
    }

    private void ClearAffixTiles()
    {
        // Clear all existing affix tiles
        for (int i = affixContent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = affixContent.transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
    }

    private void CreateSingleAffixTile(string affix)
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

    #region Timer Methods
    private void StartTimer()
    {
        currentTime = maxTime;
        timerRunning = true;
        UpdateTimerUI();
        Debug.Log($"Timer started: {maxTime} seconds");
    }

    private void StopTimer()
    {
        timerRunning = false;
        Debug.Log("Timer stopped");
    }

    private void UpdateTimerUI()
    {
        if (timerBar != null)
        {
            timerBar.value = currentTime / maxTime;
        }

        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(currentTime).ToString("F0");
        }
    }

    private void OnTimerExpired()
    {
        Debug.Log("Timer expired! Player takes damage based on root word.");
        
        // Stop the timer
        StopTimer();

        // Calculate damage to player based on root word only
        int damageToPlayer = WordScoringScript.CalculateDamageToPlayer(currentRootWord);
        
        // Apply damage to player
        if (player != null)
        {
            player.SubtractHealth(damageToPlayer);
            Debug.Log($"Player takes {damageToPlayer} damage for not submitting word in time (Root word: {currentRootWord})");
        }

        // Hide word building display
        if (WordBuildingDisplay != null)
        {
            WordBuildingDisplay.SetActive(false);
        }

        // Reset tiles
        if (prefixTile != null) SetTileComponentsEnabled(prefixTile, false);
        if (suffixTile != null) SetTileComponentsEnabled(suffixTile, false);

        // End word building phase
        if (player != null)
        {
            player.EndWordBuilding();
        }
    }
    #endregion

    #region Panel Management
    private void SetPanelStates(bool showYourTiles)
    {
        if (yourTiles != null)
            yourTiles.SetActive(showYourTiles);
        
        if (createCustomTiles != null)
            createCustomTiles.SetActive(!showYourTiles);
    }

    private void OnCreateCustomTileClicked()
    {
        SetPanelStates(false); // Hide yourTiles, show createCustomTiles
        
        // Initialize custom tile creation
        if (customTileCreating != null)
        {
            customTileCreating.InitializeCustomTileCreation();
        }
    }

    public void OnBackButtonClicked()
    {
        SetPanelStates(true); // Show yourTiles, hide createCustomTiles
    }
    #endregion

    #region Custom Affix Management
    private void LoadCustomAffixes()
    {
        // Load custom affixes from PlayerPrefs
        string customAffixesString = PlayerPrefs.GetString("CustomAffixes", "");
        
        if (!string.IsNullOrEmpty(customAffixesString))
        {
            string[] customAffixesArray = customAffixesString.Split('|');
            foreach (string customAffix in customAffixesArray)
            {
                if (!string.IsNullOrWhiteSpace(customAffix))
                {
                    customAffixesList.Add(customAffix.Trim());
                }
            }
        }

        Debug.Log($"Loaded {customAffixesList.Count} custom affixes from PlayerPrefs");
    }

    private void SaveCustomAffixes()
    {
        // Save custom affixes to PlayerPrefs
        string customAffixesString = string.Join("|", customAffixesList.ToArray());
        PlayerPrefs.SetString("CustomAffixes", customAffixesString);
        PlayerPrefs.Save();
        
        Debug.Log($"Saved {customAffixesList.Count} custom affixes to PlayerPrefs");
    }

    public void AddCustomAffix(string newAffix)
    {
        if (string.IsNullOrWhiteSpace(newAffix))
        {
            Debug.LogWarning("Cannot add empty or whitespace custom affix");
            return;
        }

        // Convert to uppercase for consistency
        string normalizedAffix = newAffix.Trim().ToUpper();

        // Check for duplicates in both predefined and custom affixes
        bool isDuplicate = false;

        // Check predefined affixes
        foreach (string existingAffix in affixesList)
        {
            if (existingAffix.ToUpper() == normalizedAffix)
            {
                isDuplicate = true;
                break;
            }
        }

        // Check custom affixes if not already a duplicate
        if (!isDuplicate)
        {
            foreach (string existingCustomAffix in customAffixesList)
            {
                if (existingCustomAffix.ToUpper() == normalizedAffix)
                {
                    isDuplicate = true;
                    break;
                }
            }
        }

        if (isDuplicate)
        {
            Debug.LogWarning($"Affix '{normalizedAffix}' already exists. Cannot add duplicate.");
            return;
        }

        // Add the new custom affix
        customAffixesList.Add(normalizedAffix);
        
        // Save to PlayerPrefs
        SaveCustomAffixes();
        
        // Recreate affix tiles to include the new one
        CreateAffixTiles();
        
        Debug.Log($"Added new custom affix: '{normalizedAffix}'. Total custom affixes: {customAffixesList.Count}");
    }
    #endregion
}
