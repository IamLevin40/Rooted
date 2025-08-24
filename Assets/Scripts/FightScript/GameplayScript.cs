using UnityEngine;
using UnityEngine.UI;

public class GameplayScript : MonoBehaviour
{
    #region References
    public PlayerScript player;
    public EnemyScript enemy;

    [Header("Countdown UI")]
    public GameObject CountdownDisplay;
    public Image dot1;
    public Image dot2;
    public Text countdownText;

    [HideInInspector] public bool gameActive = false;
    [HideInInspector] public bool gameEnded = false;
    
    private float countdownTimer = 3f;
    private int lastCountdownInt = 4;
    private bool fightDisplayed = false;
    private float fightDisplayTimer = 0f;
    #endregion

    #region Unity Methods
    private void Start()
    {
        InitializeGame();
        CountdownDisplay?.SetActive(true);
        UpdateCountdownUI(3);
        Debug.Log("Countdown starting...");
    }

    private void InitializeGame()
    {
        gameActive = gameEnded = fightDisplayed = false;
        countdownTimer = 3f;
        lastCountdownInt = 4;
        fightDisplayTimer = 0f;
    }

    private void Update()
    {
        if (!gameActive && !gameEnded) HandleCountdown();
        if (fightDisplayed && !gameEnded) HandleFightDisplay();
        if (gameEnded) return;

        CheckGameEndConditions();
    }

    private void HandleFightDisplay()
    {
        fightDisplayTimer += Time.deltaTime;
        if (fightDisplayTimer >= 1f)
        {
            CountdownDisplay?.SetActive(false);
            fightDisplayed = false;
            gameActive = true;
        }
    }

    private void CheckGameEndConditions()
    {
        if (player.currentHealth <= 0) { Defeat(); return; }
        if (enemy.currentHealth <= 0) { Victory(); return; }
    }
    #endregion

    #region Countdown Methods
    private void HandleCountdown()
    {
        if (gameEnded) return;

        countdownTimer -= Time.deltaTime;
        int countdownInt = Mathf.CeilToInt(countdownTimer);
        
        if (countdownInt != lastCountdownInt && countdownInt >= 0)
        {
            UpdateCountdownUI(countdownInt);
            lastCountdownInt = countdownInt;
        }
        
        if (countdownTimer <= 0 && !fightDisplayed)
        {
            UpdateCountdownUI(0);
            Debug.Log("Fight!");
            fightDisplayed = true;
            fightDisplayTimer = 0f;
        }
    }

    private void UpdateCountdownUI(int state)
    {
        if (CountdownDisplay == null) return;
        CountdownDisplay.SetActive(true);

        var colors = new[] { Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green };
        var texts = new[] { "READY.", "READY..", "READY...", "FIGHT!" };
        
        int index = 3 - state;
        if (index >= 0 && index < colors.Length)
        {
            if (dot1 != null) dot1.color = colors[index];
            if (dot2 != null) dot2.color = colors[index];
            if (countdownText != null) countdownText.text = texts[index];
        }
    }
    #endregion

    #region End State Methods
    private void Defeat()
    {
        if (gameEnded) return;
        gameEnded = true;
        Debug.Log("Defeat");
    }

    private void Victory()
    {
        if (gameEnded) return;
        gameEnded = true;
        Debug.Log("Victory");
    }
    #endregion
}
