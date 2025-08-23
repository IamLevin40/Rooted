using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
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

    public GameplayScript gameplay;

    private void Start()
    {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        currentHealthLerpSpeed = healthLerpSpeed;
        UpdateHealthUI();
    }

    private void Update()
    {
        AnimateHealth();
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

    public void AddHealth(float amount)
    {
        if (!CanUpdate()) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    public void SubtractHealth(float amount)
    {
        if (!CanUpdate()) return;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
            healthBar.value = displayedHealth / maxHealth;
        if (healthText != null)
            healthText.text = displayedHealth.ToString("F0");
    }
}
