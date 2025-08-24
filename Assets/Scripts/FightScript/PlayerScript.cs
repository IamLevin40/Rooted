using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBar;
    public Text healthText;
    public float healthLerpSpeed = 10f;
    public float maxHealthLerpSpeed = 1000f;
    public float healthLerpEase = 5f;
    private float displayedHealth;
    private float currentHealthLerpSpeed = 10f;

    [Header("Score Settings")]
    public int score = 0;
    public Text scoreText;
    public float scoreLerpSpeed = 10f;
    public float maxScoreLerpSpeed = 1000f;
    public float lerpSpeedEase = 5f;
    private int displayedScore = 0;
    private float currentLerpSpeed = 10f;

    public string rootWord = "";
    public GameplayScript gameplay;

    private void Start()
    {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        currentHealthLerpSpeed = healthLerpSpeed;
        UpdateHealthUI();
        displayedScore = score;
        currentLerpSpeed = scoreLerpSpeed;
        UpdateScoreUI();
    }

    private void Update()
    {
        AnimateHealth();
        AnimateScore();
    }

    private bool CanUpdate()
    {
        return gameplay != null && gameplay.gameActive && !gameplay.gameEnded;
    }

    private void AnimateHealth()
    {
        if (Mathf.Abs(displayedHealth - currentHealth) > 0.01f)
        {
            float gap = Mathf.Abs(currentHealth - displayedHealth);
            float targetLerpSpeed = Mathf.Lerp(healthLerpSpeed, maxHealthLerpSpeed, Mathf.Clamp01(gap / 10f));
            currentHealthLerpSpeed = Mathf.Lerp(currentHealthLerpSpeed, targetLerpSpeed, healthLerpEase * Time.deltaTime);
            displayedHealth = Mathf.MoveTowards(displayedHealth, currentHealth, currentHealthLerpSpeed * Time.deltaTime);
            UpdateHealthUI();
        }
        else
        {
            currentHealthLerpSpeed = Mathf.Lerp(currentHealthLerpSpeed, healthLerpSpeed, healthLerpEase * Time.deltaTime);
        }
    }

    private void AnimateScore()
    {
        if (displayedScore != score)
        {
            int gap = Mathf.Abs(score - displayedScore);
            float targetLerpSpeed = Mathf.Lerp(scoreLerpSpeed, maxScoreLerpSpeed, Mathf.Clamp01(gap / 100f));
            currentLerpSpeed = Mathf.Lerp(currentLerpSpeed, targetLerpSpeed, lerpSpeedEase * Time.deltaTime);
            displayedScore = (int)Mathf.MoveTowards(displayedScore, score, Mathf.Ceil(currentLerpSpeed * Time.deltaTime));
            UpdateScoreUI();
        }
        else
        {
            currentLerpSpeed = Mathf.Lerp(currentLerpSpeed, scoreLerpSpeed, lerpSpeedEase * Time.deltaTime);
        }
    }

    public void AddHealth(float amount)
    {
        if (!CanUpdate()) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthUI();
    }

    public void SubtractHealth(float amount)
    {
        if (!CanUpdate()) return;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
            healthBar.value = displayedHealth / maxHealth;
        if (healthText != null)
            healthText.text = displayedHealth.ToString("F0");
    }

    public void AddScore(int amount)
    {
        if (!CanUpdate()) return;
        score = Mathf.Max(0, score + amount);
    }

    public void SubtractScore(int amount)
    {
        if (!CanUpdate()) return;
        score = Mathf.Max(0, score - amount);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = displayedScore.ToString("D5");
    }
}
