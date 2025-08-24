using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    #region Health Fields
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
    #endregion

    #region Score Fields
    [Header("Score Settings")]
    public int score = 0;
    public Text scoreText;
    public float scoreLerpSpeed = 10f;
    public float maxScoreLerpSpeed = 1000f;
    public float lerpSpeedEase = 5f;
    private int displayedScore = 0;
    private float currentLerpSpeed = 10f;
    #endregion

    #region Root Word & Gameplay
    public string rootWord = "";
    public string playWord = "";
    public float queueDamage = 0f;
    public int queueScore = 0;
    public GameplayScript gameplay;
    public WordBuildingScript wordBuilding;
    public bool onWordBuildingPhase = false;
    #endregion

    #region Projectile Fields
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public EnemyScript enemy;
    private GameObject currentProjectile = null;
    #endregion

    #region Unity Methods
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
    #endregion

    #region Health Methods
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
    #endregion

    #region Score Methods
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
    #endregion

    #region Utility
    private bool CanUpdate()
    {
        return gameplay != null && gameplay.gameActive && !gameplay.gameEnded;
    }
    
    public void TriggerWordBuilding()
    {
        onWordBuildingPhase = true;
        if (wordBuilding != null)
        {
            wordBuilding.OnStartWordBuilding(rootWord);
        }
    }

    public void EndWordBuilding()
    {
        onWordBuildingPhase = false;
    }

    public void SpawnPlayerProjectile(float damage, int score)
    {
        if (projectilePrefab == null || enemy == null) return;

        // Spawn projectile on left or right side of player
        bool right = Random.value > 0.5f;
        float minOffset = 1.5f, maxOffset = 2.5f;
        float xOffset = (right ? 1 : -1) * Random.Range(minOffset, maxOffset);
        float yOffset = Random.Range(0f, 0.75f);
        Vector3 spawnPos = transform.position + new Vector3(xOffset, yOffset, 0);
        
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity, this.transform);
        currentProjectile = proj;
        
        PlayerProjectile pp = proj.GetComponent<PlayerProjectile>();
        if (pp != null)
        {
            pp.moveRight = right;
            pp.player = this;
            pp.enemy = enemy;
            pp.spawnOrigin = spawnPos;
            pp.projectileWord = playWord;
            pp.damage = damage;
            pp.score = score;
        }
    }

    public void OnProjectileDestroyed(GameObject proj)
    {
        if (currentProjectile == proj)
        {
            currentProjectile = null;
        }
    }
    #endregion
}
