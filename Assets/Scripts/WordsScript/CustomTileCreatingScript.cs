using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CustomTileCreatingScript : MonoBehaviour
{
    [Header("UI References")]
    public GameObject letterButtonPrefab;
    public GameObject letterButtonsParent;
    public Text customAffixText;
    public Button backspaceButton;
    public Button submitButton;
    public WordBuildingScript wordBuildingScript;

    [Header("Settings")]
    private string currentCustomAffix = "";
    private List<GameObject> letterButtons = new List<GameObject>();

    private void Start()
    {
        // Setup button listeners
        if (backspaceButton != null)
        {
            backspaceButton.onClick.AddListener(OnBackspaceClicked);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
    }

    public void InitializeCustomTileCreation()
    {
        // Clear any existing content
        ClearLetterButtons();
        ClearCustomAffix();

        // Create letter buttons A-Z
        CreateLetterButtons();
    }

    private void ClearLetterButtons()
    {
        foreach (GameObject button in letterButtons)
        {
            if (button != null)
                Destroy(button);
        }
        letterButtons.Clear();
    }

    private void ClearCustomAffix()
    {
        currentCustomAffix = "";
        UpdateCustomAffixDisplay();
    }

    private void CreateLetterButtons()
    {
        if (letterButtonPrefab == null || letterButtonsParent == null) return;

        // Create buttons for A-Z
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            GameObject letterButton = Instantiate(letterButtonPrefab, letterButtonsParent.transform);
            letterButtons.Add(letterButton);

            // Set the letter text
            Text letterText = letterButton.GetComponentInChildren<Text>();
            if (letterText != null)
            {
                letterText.text = letter.ToString();
            }

            // Add click listener
            Button button = letterButton.GetComponent<Button>();
            if (button != null)
            {
                char currentLetter = letter; // Capture for closure
                button.onClick.AddListener(() => OnLetterClicked(currentLetter));
            }
        }
    }

    private void OnLetterClicked(char letter)
    {
        currentCustomAffix += letter.ToString();
        UpdateCustomAffixDisplay();
        Debug.Log($"Letter clicked: {letter}, Current affix: {currentCustomAffix}");
    }

    private void OnBackspaceClicked()
    {
        if (currentCustomAffix.Length > 0)
        {
            currentCustomAffix = currentCustomAffix.Substring(0, currentCustomAffix.Length - 1);
            UpdateCustomAffixDisplay();
            Debug.Log($"Backspace clicked, Current affix: {currentCustomAffix}");
        }
    }

    private void OnSubmitClicked()
    {
        if (!string.IsNullOrEmpty(currentCustomAffix))
        {
            Debug.Log($"Custom tile submitted: {currentCustomAffix}");
            
            // Add the custom affix to the word building script
            if (wordBuildingScript != null)
            {
                wordBuildingScript.AddCustomAffix(currentCustomAffix);
            }
            
            // Clear the custom affix
            ClearCustomAffix();
            
            // Go back to yourTiles panel
            if (wordBuildingScript != null)
            {
                wordBuildingScript.OnBackButtonClicked();
            }
        }
        else
        {
            Debug.Log("Cannot submit empty custom tile");
        }
    }

    private void UpdateCustomAffixDisplay()
    {
        if (customAffixText != null)
        {
            customAffixText.text = currentCustomAffix;
        }
    }
}
