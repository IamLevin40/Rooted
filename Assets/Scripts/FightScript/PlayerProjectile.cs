using UnityEngine;
using UnityEngine.UI;

public class PlayerProjectile : MonoBehaviour
{
    #region Inspector Fields
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
    #endregion

    #region Runtime Fields
    public Vector3 spawnOrigin;
    public string projectileWord = "";
    public int damage = 0;
    public int score = 0;
    private bool hasHitEnemy = false;
    private float t = 0f;
    private float currentSpeed = 0f;
    #endregion

    private void Start()
    {
        currentSpeed = initialSpeed;
        
        // Set the projectile word text
        if (wordText == null)
        {
            wordText = GetComponentInChildren<Text>();
        }
        if (wordText != null)
        {
            wordText.text = projectileWord;
        }
    }

    private void Update()
    {
        if (player == null || enemy == null) return;
        
        Transform enemyTransform = enemy.transform;
        t += Time.deltaTime;
        MoveProjectile(enemyTransform);
        CheckEnemyCollision(enemyTransform);
    }

    private void MoveProjectile(Transform enemyTransform)
    {
        // Initial slow movement, then accelerate
        if (t < initialSlowDuration)
        {
            currentSpeed = Mathf.Lerp(initialSpeed, maxSpeed * 0.2f, t / initialSlowDuration);
        }
        else
        {
            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime);
        }

        // Calculate progress (0=start, 1=at enemy)
        float totalDist = Vector3.Distance(spawnOrigin, enemyTransform.position);
        float traveledDist = currentSpeed * t;
        float progress = Mathf.Clamp01(traveledDist / totalDist);

        // Linear path
        Vector3 linearPos = Vector3.Lerp(spawnOrigin, enemyTransform.position, progress);

        // Curvy offset (sinusoidal perpendicular to path)
        Vector3 pathDir = (enemyTransform.position - spawnOrigin).normalized;
        Vector3 perp = Vector3.Cross(pathDir, Vector3.forward).normalized;
        float curve = Mathf.Sin(progress * Mathf.PI * curveFrequency) * curveAmplitude * (moveRight ? 1 : -1);
        Vector3 curvyPos = linearPos + perp * curve * (1f - progress); // curve fades as it nears enemy

        transform.position = curvyPos;
    }

    private void CheckEnemyCollision(Transform enemyTransform)
    {
        if (!hasHitEnemy && Vector3.Distance(transform.position, enemyTransform.position) < 0.5f)
        {
            HitEnemy();
        }
    }

    private void HitEnemy()
    {
        hasHitEnemy = true;
        
        // Apply damage to enemy
        if (enemy != null)
        {
            enemy.SubtractHealth(damage);
            Debug.Log($"Enemy takes {damage} damage from projectile word: {projectileWord}");
        }

        // Add score to player
        if (player != null)
        {
            player.AddScore(score);
            Debug.Log($"Player gains {score} points from projectile word: {projectileWord}");
        }

        if (player != null)
        {
            player.EndWordBuilding();
        }

        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        if (player != null)
        {
            player.OnProjectileDestroyed(gameObject);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnProjectileDestroyed(gameObject);
        }
    }
}
