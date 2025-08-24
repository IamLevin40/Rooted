using UnityEngine;

public class WordBuildingScript : MonoBehaviour
{
    public GameObject WordBuildingDisplay;
    public PlayerScript player;

    private void Start()
    {
        WordBuildingDisplay.SetActive(false);
    }

    public void OnStartWordBuilding(string rootWord)
    {
        if (WordBuildingDisplay != null)
        {
            WordBuildingDisplay.SetActive(true);
        }

        // TODO: Implement logic to start word building with the given rootWord
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
    // ...existing code...
}
