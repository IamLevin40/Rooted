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
    public AffixHandlingScript affixHandler;

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
    private GameObject currentDraggedTile;
    private Canvas canvas;
    private float currentTime;
    private bool timerRunning = false;
    #endregion

    private void Start()
    {
        InitializeComponents();
        SetupUIListeners();
        InitializeGameState();
    }

    private void InitializeComponents()
    {
        canvas = FindFirstObjectByType<Canvas>();
        SetTileComponentsEnabled(prefixTile, false);
        SetTileComponentsEnabled(suffixTile, false);
        WordBuildingDisplay.SetActive(false);
    }

    private void SetupUIListeners()
    {
        wordSubmitButton?.onClick.AddListener(OnWordSubmit);
        createCustomTileButton?.onClick.AddListener(OnCreateCustomTileClicked);
        backButton?.onClick.AddListener(OnBackButtonClicked);
    }

    private void InitializeGameState()
    {
        SetPanelStates(true);
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
        WordBuildingDisplay?.SetActive(true);
        ResetWordState(rootWord);
        UpdateAllDisplays();
        StartTimer();
        Debug.Log($"Word building started with root word: {rootWord}");
    }

    private void ResetWordState(string rootWord)
    {
        currentRootWord = rootWord;
        currentPrefix = currentSuffix = currentDefinition = "";
        if (rootWordText != null) rootWordText.text = currentRootWord;
    }

    private void UpdateAllDisplays()
    {
        UpdateWordDisplay();
        UpdateSubmitButtonState();
        UpdatePlayerWord();
    }

    private void UpdateWordDisplay()
    {
        if (prefixText != null) prefixText.text = currentPrefix;
        if (suffixText != null) suffixText.text = currentSuffix;
        if (definitionText != null)
            definitionText.text = string.IsNullOrEmpty(currentDefinition) ? "No definition available for this word." : currentDefinition;
    }

    private void UpdateSubmitButtonState()
    {
        if (wordSubmitButton != null)
        {
            bool canSubmit = !string.IsNullOrEmpty(currentPrefix) || !string.IsNullOrEmpty(currentSuffix);
            wordSubmitButton.interactable = canSubmit;
        }
    }

    public void OnWordSubmit()
    {
        string combinedWord = currentPrefix + currentRootWord + currentSuffix;
        bool isValidWord = validWordDetector.IsValidWord(combinedWord);

        if (isValidWord)
        {
            ProcessValidWord();
        }
        else
        {
            Debug.Log($"Invalid word: {combinedWord} - No projectile spawned");
            return;
        }

        ResetAfterSubmission();
    }

    private void ProcessValidWord()
    {
        WordBuildingDisplay?.SetActive(false);
        if (player != null) 
            player.SpawnPlayerProjectile(player.queueDamage, player.queueScore);
    }

    private void ResetAfterSubmission()
    {
        StopTimer();
        SetTileComponentsEnabled(prefixTile, false);
        SetTileComponentsEnabled(suffixTile, false);
    }

    public void SetPrefix(string prefix) => SetAffix(ref currentPrefix, prefix, prefixTile, prefixText);
    public void SetSuffix(string suffix) => SetAffix(ref currentSuffix, suffix, suffixTile, suffixText);

    private void SetAffix(ref string currentAffix, string newAffix, GameObject tile, Text displayText)
    {
        currentAffix = newAffix ?? "";
        UpdateAffixDisplay(tile, displayText, currentAffix);
        UpdateSubmitButtonState();
    }

    private void UpdateAffixDisplay(GameObject tile, Text displayText, string affixValue)
    {
        bool hasAffix = !string.IsNullOrEmpty(affixValue);
        
        if (displayText != null) displayText.text = affixValue;
            
        if (tile != null)
        {
            SetTileComponentsEnabled(tile, hasAffix);
            var tileText = tile.GetComponentInChildren<Text>(true);
            if (tileText != null) tileText.text = affixValue;
        }

        UpdatePlayerWord();
    }

    private void UpdatePlayerWord()
    {
        if (player == null) return;

        ResetPlayerQueue();

        if (IsOnlyRootWord())
        {
            SetDefinition(validWordDetector.GetWordDefinition(currentRootWord));
            Debug.Log("Only root word displayed.");
            return;
        }

        string playWord = currentPrefix + currentRootWord + currentSuffix;
        if (!validWordDetector.IsValidWord(playWord))
        {
            Debug.Log($"Invalid word: {playWord}");
            if (definitionText != null)
                definitionText.text = "No word found on the dictionary. Let's try it again!";
            return;
        }

        ProcessValidPlayerWord(playWord);
    }

    private void ResetPlayerQueue()
    {
        player.queueDamage = 0;
        player.queueScore = 0;
        player.isEnvironmentalWord = false;
    }

    private bool IsOnlyRootWord() => string.IsNullOrEmpty(currentPrefix) && string.IsNullOrEmpty(currentSuffix);

    private void ProcessValidPlayerWord(string playWord)
    {
        player.playWord = playWord;
        CalculateAndUpdateQueueValues(playWord);
        Debug.Log($"Player word updated: {playWord}");
        Debug.Log($"Queue Damage: {player.queueDamage}, Queue Score: {player.queueScore} for word: {playWord}; Environmental: {player.isEnvironmentalWord}");
    }

    private void CalculateAndUpdateQueueValues(string playWord)
    {
        SetDefinition(validWordDetector.GetWordDefinition(playWord));
        
        bool isEnvironmentalWord = validWordDetector.IsEnvironmentalWord(playWord);
        int multiplier = isEnvironmentalWord ? 2 : 1;

        player.queueDamage = WordScoringScript.CalculateDamageToEnemy(playWord) * multiplier;
        player.queueScore = WordScoringScript.CalculateWordScore(playWord) * multiplier;
        player.isEnvironmentalWord = isEnvironmentalWord;
    }

    private void SetTileComponentsEnabled(GameObject tile, bool enabled)
    {
        if (tile == null) return;
        
        SetComponentAlpha(tile.GetComponent<Image>(), enabled);
        SetButtonInteractable(tile.GetComponent<Button>(), enabled);
        SetComponentAlpha(tile.GetComponentInChildren<Text>(true), enabled);
    }

    private void SetComponentAlpha(Graphic component, bool enabled)
    {
        if (component != null)
        {
            Color c = component.color;
            c.a = enabled ? 1f : 0f;
            component.color = c;
        }
    }

    private void SetButtonInteractable(Button button, bool enabled)
    {
        if (button != null) button.interactable = enabled;
    }

    public void SetDefinition(string definition)
    {
        currentDefinition = definition;
        UpdateWordDisplay();
    }

    public void CreateAffixTiles()
    {
        if (affixContent == null || affixTilePrefab == null || affixHandler == null) return;

        // Clear existing tiles first
        ClearAffixTiles();

        // Get all affixes from the handler
        List<string> allAffixes = affixHandler.GetAllAffixes();

        // Create tiles for all affixes
        foreach (string affix in allAffixes)
        {
            CreateSingleAffixTile(affix);
        }

        Debug.Log($"Created {allAffixes.Count} affix tiles. {affixHandler.GetAffixStatistics()}");
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
        AddDragFunctionality(prefixTile, "", true);
        AddDragFunctionality(suffixTile, "", true);
    }

    private void AddDragFunctionality(GameObject tile, string affixText, bool isDropZone)
    {
        if (tile == null) return;

        // Add or get drag handler
        var dragHandler = tile.GetComponent<DragHandler>() ?? tile.AddComponent<DragHandler>();
        dragHandler.Initialize(this, affixText, isDropZone);

        // Add drop zone functionality if needed
        if (isDropZone)
        {
            var dropZone = tile.GetComponent<DropZone>() ?? tile.AddComponent<DropZone>();
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
            SetTileComponentsEnabled(draggedTile, false);
    }

    private void CreateDraggedTileClone(GameObject originalTile, string affixText, Vector2 startPosition)
    {
        currentDraggedTile = Instantiate(originalTile, canvas.transform);
        currentDraggedTile.name = $"DraggedTile_{affixText}";

        SetupDraggedTileTransform(originalTile, startPosition);
        CleanupDraggedTileComponents();
        ConfigureDraggedTileAppearance(affixText);
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
        DestroyComponents<DragHandler>(currentDraggedTile);
        DestroyComponents<DropZone>(currentDraggedTile);
    }

    private void DestroyComponents<T>(GameObject target) where T : Component
    {
        var components = target.GetComponents<T>();
        foreach (var component in components)
            DestroyImmediate(component);
    }

    private void ConfigureDraggedTileAppearance(string affixText)
    {
        var button = currentDraggedTile.GetComponent<Button>();
        var text = currentDraggedTile.GetComponentInChildren<Text>();
        
        if (button != null) button.interactable = false;
        if (text != null) text.text = affixText;

        currentDraggedTile.SetActive(true);
        SetTileComponentsEnabled(currentDraggedTile, true);
    }

    public void UpdateDrag(Vector2 position)
    {
        if (currentDraggedTile?.GetComponent<RectTransform>() is RectTransform rectTransform)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, position, canvas.worldCamera, out Vector2 localPos))
            {
                rectTransform.localPosition = localPos;
            }
        }
    }

    public void EndDrag(bool wasDropped, bool isPrefix = false, string droppedAffix = "", bool wasFromDropZone = false, bool wasOriginallyPrefix = false)
    {
        if (currentDraggedTile != null)
        {
            if (wasDropped)
            {
                ProcessDroppedTile(isPrefix, droppedAffix, wasFromDropZone, wasOriginallyPrefix);
            }

            Destroy(currentDraggedTile);
            currentDraggedTile = null;
        }
    }

    private void ProcessDroppedTile(bool isPrefix, string droppedAffix, bool wasFromDropZone, bool wasOriginallyPrefix)
    {
        // Set the new position
        if (isPrefix) SetPrefix(droppedAffix);
        else SetSuffix(droppedAffix);
        
        // Handle zone-to-zone movement
        if (wasFromDropZone && wasOriginallyPrefix != isPrefix)
        {
            if (wasOriginallyPrefix) SetPrefix("");
            else SetSuffix("");
        }
    }

    public void RemoveAffix(GameObject affixTile)
    {
        if (affixTile == prefixTile) SetPrefix("");
        else if (affixTile == suffixTile) SetSuffix("");
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
        if (timerBar != null) timerBar.value = currentTime / maxTime;
        if (timerText != null) timerText.text = Mathf.Ceil(currentTime).ToString("F0");
    }

    private void OnTimerExpired()
    {
        Debug.Log("Timer expired! Player takes damage based on root word.");
        
        StopTimer();
        ApplyTimerDamage();
        CleanupAfterTimer();
    }

    private void ApplyTimerDamage()
    {
        int damageToPlayer = WordScoringScript.CalculateDamageToPlayer(currentRootWord);
        if (player != null)
        {
            player.SubtractHealth(damageToPlayer);
            Debug.Log($"Player takes {damageToPlayer} damage for not submitting word in time (Root word: {currentRootWord})");
        }
    }

    private void CleanupAfterTimer()
    {
        WordBuildingDisplay?.SetActive(false);
        SetTileComponentsEnabled(prefixTile, false);
        SetTileComponentsEnabled(suffixTile, false);
        player?.EndWordBuilding();
    }
    #endregion

    #region Panel Management
    private void SetPanelStates(bool showYourTiles)
    {
        yourTiles?.SetActive(showYourTiles);
        createCustomTiles?.SetActive(!showYourTiles);
    }

    private void OnCreateCustomTileClicked()
    {
        SetPanelStates(false);
        customTileCreating?.InitializeCustomTileCreation();
    }

    public void OnBackButtonClicked()
    {
        SetPanelStates(true);
        CreateAffixTiles();
    }
    #endregion
}
