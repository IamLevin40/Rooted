using UnityEngine;
using UnityEngine.UI;

public class PlayerProjectile : MonoBehaviour
{
    #region Fields
    [Header("Projectile Settings")]
    public float maxSpeed = 4f;
    public float acceleration = 1.5f;
    public float initialSpeed = 0f;
    public float initialSlowDuration = 1f;
    public float curveAmplitude = 1.2f;
    public float curveFrequency = 1.2f;
    public bool moveRight = true;
    
    [Header("References")]
    public PlayerScript player;
    public EnemyScript enemy;
    public Text wordText;

    [Header("Runtime Data")]
    public Vector3 spawnOrigin;
    public string projectileWord = "";
    public float damage = 0;
    public int score = 0;
    
    private bool hasHitEnemy = false;
    private float t = 0f;
    private float currentSpeed = 0f;
    #endregion

    private void Start()
    {
        currentSpeed = initialSpeed;
        InitializeWordText();
    }

    private void InitializeWordText()
    {
        wordText ??= GetComponentInChildren<Text>();
        if (wordText != null) wordText.text = projectileWord;
    }

    private void Update()
    {
        if (player == null || enemy == null) return;
        
        t += Time.deltaTime;
        MoveProjectile(enemy.transform);
        CheckEnemyCollision(enemy.transform);
    }

    private void MoveProjectile(Transform enemyTransform)
    {
        UpdateSpeed();
        
        float progress = CalculateProgress(enemyTransform);
        Vector3 linearPos = Vector3.Lerp(spawnOrigin, enemyTransform.position, progress);
        Vector3 curvyPos = ApplyCurve(linearPos, enemyTransform, progress);
        
        transform.position = curvyPos;
    }

    private void UpdateSpeed()
    {
        if (t < initialSlowDuration)
            currentSpeed = Mathf.Lerp(initialSpeed, maxSpeed * 0.2f, t / initialSlowDuration);
        else
            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime);
    }

    private float CalculateProgress(Transform enemyTransform)
    {
        float totalDist = Vector3.Distance(spawnOrigin, enemyTransform.position);
        float traveledDist = currentSpeed * t;
        return Mathf.Clamp01(traveledDist / totalDist);
    }

    private Vector3 ApplyCurve(Vector3 linearPos, Transform enemyTransform, float progress)
    {
        Vector3 pathDir = (enemyTransform.position - spawnOrigin).normalized;
        Vector3 perp = Vector3.Cross(pathDir, Vector3.forward).normalized;
        float curve = Mathf.Sin(progress * Mathf.PI * curveFrequency) * curveAmplitude * (moveRight ? 1 : -1);
        return linearPos + perp * curve * (1f - progress);
    }

    private void CheckEnemyCollision(Transform enemyTransform)
    {
        if (!hasHitEnemy && Vector3.Distance(transform.position, enemyTransform.position) < 0.5f)
            HitEnemy();
    }

    private void HitEnemy()
    {
        hasHitEnemy = true;
        
        enemy?.SubtractHealth(damage);
        player?.AddScore(score);
        player?.EndWordBuilding();
        
        Debug.Log($"Enemy takes {damage} damage from projectile word: {projectileWord}");
        Debug.Log($"Player gains {score} points from projectile word: {projectileWord}");
        
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        player?.OnProjectileDestroyed(gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        player?.OnProjectileDestroyed(gameObject);
    }
}
