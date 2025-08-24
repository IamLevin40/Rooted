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

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public PlayerScript player;
    public float projectileCooldown = 1f;
    private float projectileTimer = 0f;
    private GameObject currentProjectile = null;

    private void Start()
    {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        currentHealthLerpSpeed = healthLerpSpeed;
        UpdateHealthUI();
        projectileTimer = 0f;
        currentProjectile = null;
    }

    private void Update()
    {
        AnimateHealth();
        HandleProjectileSpawning();
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

    private void HandleProjectileSpawning()
    {
        if (!CanUpdate() || projectilePrefab == null || player == null) return;

        // If no projectile exists, start cooldown and spawn after 1s
        if (currentProjectile == null)
        {
            projectileTimer += Time.deltaTime;
            if (projectileTimer >= projectileCooldown)
            {
                projectileTimer = 0f;
                SpawnProjectile();
            }
        }
    }

    private void SpawnProjectile()
    {
        // Randomly choose left or right
        bool right = Random.value > 0.5f;
        float minOffset = 1.0f, maxOffset = 2.5f;
        float xOffset = (right ? 1 : -1) * Random.Range(minOffset, maxOffset);
        float yOffset = Random.Range(-0.5f, 0.5f);
        Vector3 spawnPos = transform.position + new Vector3(xOffset, yOffset, 0);
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity, this.transform);
        currentProjectile = proj;
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            ep.moveRight = right;
            ep.enemy = this;
            ep.player = player;
            ep.spawnOrigin = spawnPos;
        }
    }

    // Called by projectile when destroyed
    public void OnProjectileDestroyed(GameObject proj)
    {
        if (currentProjectile == proj)
        {
            currentProjectile = null;
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
