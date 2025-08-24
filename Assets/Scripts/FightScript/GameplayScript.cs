using UnityEngine;
using UnityEngine.UI;

public class GameplayScript : MonoBehaviour
{
    #region References
    public PlayerScript player;
    public EnemyScript enemy;
    #endregion

    #region Countdown UI Fields
    [Header("Countdown UI")]
    public GameObject CountdownDisplay;
    public Image dot1;
    public Image dot2;
    public Text countdownText;
    #endregion

    #region State Fields
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
        gameActive = false;
        gameEnded = false;
        countdownTimer = 3f;
        lastCountdownInt = 4;
        fightDisplayed = false;
        fightDisplayTimer = 0f;
        if (CountdownDisplay != null)
            CountdownDisplay.SetActive(true);
        UpdateCountdownUI(3);
        Debug.Log("Countdown starting...");
    }

    private void Update()
    {
        if (!gameActive && !gameEnded)
        {
            HandleCountdown();
        }

        if (fightDisplayed && !gameEnded)
        {
            fightDisplayTimer += Time.deltaTime;
            if (fightDisplayTimer >= 1f)
            {
                if (CountdownDisplay != null)
                    CountdownDisplay.SetActive(false);
                fightDisplayed = false;
                gameActive = true;
            }
        }

        if (gameEnded)
            return;

        if (player.currentHealth <= 0 && !gameEnded)
        {
            Defeat();
            return;
        }
        if (enemy.currentHealth <= 0 && !gameEnded)
        {
            Victory();
            return;
        }
        // ...existing code for main gameplay loop...
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
        if (!CountdownDisplay.activeSelf)
            CountdownDisplay.SetActive(true);

        Color red = Color.red;
        Color orange = new Color(1f, 0.5f, 0f);
        Color yellow = Color.yellow;
        Color green = Color.green;

        switch (state)
        {
            case 3:
                if (dot1 != null) dot1.color = red;
                if (dot2 != null) dot2.color = red;
                if (countdownText != null) countdownText.text = "READY.";
                break;
            case 2:
                if (dot1 != null) dot1.color = orange;
                if (dot2 != null) dot2.color = orange;
                if (countdownText != null) countdownText.text = "READY..";
                break;
            case 1:
                if (dot1 != null) dot1.color = yellow;
                if (dot2 != null) dot2.color = yellow;
                if (countdownText != null) countdownText.text = "READY...";
                break;
            case 0:
                if (dot1 != null) dot1.color = green;
                if (dot2 != null) dot2.color = green;
                if (countdownText != null) countdownText.text = "FIGHT!";
                break;
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
