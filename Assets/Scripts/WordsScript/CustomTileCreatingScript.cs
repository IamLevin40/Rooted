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

    private string currentCustomAffix = "";
    private List<GameObject> letterButtons = new List<GameObject>();

    private void Start()
    {
        backspaceButton?.onClick.AddListener(OnBackspaceClicked);
        submitButton?.onClick.AddListener(OnSubmitClicked);
    }

    public void InitializeCustomTileCreation()
    {
        ClearLetterButtons();
        ClearCustomAffix();
        CreateLetterButtons();
    }

    private void ClearLetterButtons()
    {
        letterButtons.ForEach(button => { if (button != null) Destroy(button); });
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

        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            CreateSingleLetterButton(letter);
        }
    }

    private void CreateSingleLetterButton(char letter)
    {
        GameObject letterButton = Instantiate(letterButtonPrefab, letterButtonsParent.transform);
        letterButtons.Add(letterButton);

        SetLetterButtonText(letterButton, letter);
        SetLetterButtonListener(letterButton, letter);
    }

    private void SetLetterButtonText(GameObject letterButton, char letter)
    {
        var letterText = letterButton.GetComponentInChildren<Text>();
        if (letterText != null) letterText.text = letter.ToString();
    }

    private void SetLetterButtonListener(GameObject letterButton, char letter)
    {
        var button = letterButton.GetComponent<Button>();
        button?.onClick.AddListener(() => OnLetterClicked(letter));
    }

    private void OnLetterClicked(char letter)
    {
        currentCustomAffix += letter;
        UpdateCustomAffixDisplay();
    }

    private void OnBackspaceClicked()
    {
        if (currentCustomAffix.Length > 0)
        {
            currentCustomAffix = currentCustomAffix[..^1];
            UpdateCustomAffixDisplay();
        }
    }

    private void OnSubmitClicked()
    {
        if (string.IsNullOrEmpty(currentCustomAffix))
        {
            Debug.Log("Cannot submit empty custom tile");
            return;
        }

        Debug.Log($"Custom tile submitted: {currentCustomAffix}");
        wordBuildingScript?.affixHandler.AddCustomAffix(currentCustomAffix);
        ClearCustomAffix();
        wordBuildingScript?.OnBackButtonClicked();
    }

    private void UpdateCustomAffixDisplay()
    {
        if (customAffixText != null) customAffixText.text = currentCustomAffix;
    }
}
