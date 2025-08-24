using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    #region Health & Projectile Fields
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

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public PlayerScript player;
    public float projectileCooldown = 1f;
    private float projectileTimer = 0f;
    private GameObject currentProjectile = null;

    public GameplayScript gameplay;
    #endregion

    #region Unity Methods
    private void Start()
    {
        InitializeHealth();
        projectileTimer = 0f;
        currentProjectile = null;
    }

    private void InitializeHealth()
    {
        currentHealth = displayedHealth = maxHealth;
        currentHealthLerpSpeed = healthLerpSpeed;
        UpdateHealthUI();
    }

    private void Update()
    {
        AnimateHealth();
        HandleProjectileSpawning();
    }
    #endregion

    #region Health Methods
    private void AnimateHealth()
    {
        if (Mathf.Abs(displayedHealth - currentHealth) > 0.01f)
        {
            UpdateHealthLerpSpeed();
            displayedHealth = Mathf.MoveTowards(displayedHealth, currentHealth, currentHealthLerpSpeed * Time.deltaTime);
            UpdateHealthUI();
        }
        else
        {
            currentHealthLerpSpeed = Mathf.Lerp(currentHealthLerpSpeed, healthLerpSpeed, healthLerpEase * Time.deltaTime);
        }
    }

    private void UpdateHealthLerpSpeed()
    {
        float gap = Mathf.Abs(currentHealth - displayedHealth);
        float targetLerpSpeed = Mathf.Lerp(healthLerpSpeed, maxHealthLerpSpeed, Mathf.Clamp01(gap / 10f));
        currentHealthLerpSpeed = Mathf.Lerp(currentHealthLerpSpeed, targetLerpSpeed, healthLerpEase * Time.deltaTime);
    }

    public void AddHealth(float amount) => ModifyHealth(amount);
    public void SubtractHealth(float amount) => ModifyHealth(-amount);

    private void ModifyHealth(float amount)
    {
        if (!CanUpdate()) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null) healthBar.value = displayedHealth / maxHealth;
        if (healthText != null) healthText.text = displayedHealth.ToString("F0");
    }
    #endregion

    #region Projectile Methods
    private void HandleProjectileSpawning()
    {
        if (!CanUpdate() || projectilePrefab == null || player == null) return;
        if (player.onWordBuildingPhase || currentProjectile != null) return;

        projectileTimer += Time.deltaTime;
        if (projectileTimer >= projectileCooldown)
        {
            projectileTimer = 0f;
            SpawnProjectile();
        }
    }

    private void SpawnProjectile()
    {
        var (spawnPos, moveRight) = GetProjectileSpawnData();
        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity, transform);
        currentProjectile = proj;
        
        var ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null) ConfigureProjectile(ep, spawnPos, moveRight);
    }

    private (Vector3 spawnPos, bool moveRight) GetProjectileSpawnData()
    {
        bool right = Random.value > 0.5f;
        float xOffset = (right ? 1 : -1) * Random.Range(1.5f, 2.5f);
        float yOffset = Random.Range(0f, 0.75f);
        Vector3 spawnPos = transform.position + new Vector3(xOffset, yOffset, 0);
        return (spawnPos, right);
    }

    private void ConfigureProjectile(EnemyProjectile ep, Vector3 spawnPos, bool moveRight)
    {
        ep.moveRight = moveRight;
        ep.enemy = this;
        ep.player = player;
        ep.spawnOrigin = spawnPos;
    }

    public void OnProjectileDestroyed(GameObject proj)
    {
        if (currentProjectile == proj) currentProjectile = null;
    }
    #endregion

    #region Utility
    private bool CanUpdate() => gameplay?.gameActive == true && !gameplay.gameEnded;
    #endregion
}
