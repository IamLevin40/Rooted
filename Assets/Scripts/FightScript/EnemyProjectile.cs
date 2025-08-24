using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyProjectile : MonoBehaviour
{
    #region Fields
    [Header("Projectile Settings")]
    public float projectileDamage = 5f;
    public float maxSpeed = 4f;
    public float acceleration = 1.5f;
    public float initialSpeed = 0f;
    public float initialSlowDuration = 1f;
    public float curveAmplitude = 1.2f;
    public float curveFrequency = 1.2f;
    public bool moveRight = true;
    
    [Header("References")]
    public EnemyScript enemy;
    public PlayerScript player;
    public Button interactButton;
    public Text wordText;

    [Header("Runtime Data")]
    public Vector3 spawnOrigin;
    public string rootWord = "";
    
    private static List<string> rootWordsList;
    private bool hasHitPlayer = false;
    private float t = 0f;
    private float currentSpeed = 0f;
    #endregion

    private void Start()
    {
        interactButton?.onClick.AddListener(OnProjectileInteract);
        currentSpeed = initialSpeed;
        SetRandomRootWord();
    }

    private void Update()
    {
        if (player == null || enemy == null) return;
        
        t += Time.deltaTime;
        MoveProjectile(player.transform);
        CheckPlayerCollision(player.transform);
    }

    private void SetRandomRootWord()
    {
        LoadRootWordsIfNeeded();
        
        if (rootWordsList?.Count > 0)
        {
            rootWord = rootWordsList[Random.Range(0, rootWordsList.Count)];
            UpdateWordText();
        }
    }

    private void LoadRootWordsIfNeeded()
    {
        if (rootWordsList != null) return;
        
        rootWordsList = new List<string>();
        var txt = Resources.Load<TextAsset>("ExternalFiles/rootwords_list");
        
        if (txt != null)
        {
            using var reader = new System.IO.StringReader(txt.text);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    rootWordsList.Add(line.Trim());
            }
        }
    }

    private void UpdateWordText()
    {
        wordText ??= GetComponentInChildren<Text>();
        if (wordText != null) wordText.text = rootWord;
    }

    private void MoveProjectile(Transform playerTransform)
    {
        UpdateSpeed();
        
        float progress = CalculateProgress(playerTransform);
        Vector3 linearPos = Vector3.Lerp(spawnOrigin, playerTransform.position, progress);
        Vector3 curvyPos = ApplyCurve(linearPos, playerTransform, progress);
        
        transform.position = curvyPos;
    }

    private void UpdateSpeed()
    {
        if (t < initialSlowDuration)
            currentSpeed = Mathf.Lerp(initialSpeed, maxSpeed * 0.2f, t / initialSlowDuration);
        else
            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime);
    }

    private float CalculateProgress(Transform playerTransform)
    {
        float totalDist = Vector3.Distance(spawnOrigin, playerTransform.position);
        float traveledDist = currentSpeed * t;
        return Mathf.Clamp01(traveledDist / totalDist);
    }

    private Vector3 ApplyCurve(Vector3 linearPos, Transform playerTransform, float progress)
    {
        Vector3 pathDir = (playerTransform.position - spawnOrigin).normalized;
        Vector3 perp = Vector3.Cross(pathDir, Vector3.forward).normalized;
        float curve = Mathf.Sin(progress * Mathf.PI * curveFrequency) * curveAmplitude * (moveRight ? 1 : -1);
        return linearPos + perp * curve * (1f - progress);
    }

    private void CheckPlayerCollision(Transform playerTransform)
    {
        if (!hasHitPlayer && Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
        {
            player?.SubtractHealth((int)projectileDamage);
            hasHitPlayer = true;
            
            if (player != null) player.rootWord = "";
            enemy?.OnProjectileDestroyed(gameObject);
            Destroy(gameObject);
        }
    }

    public void OnProjectileInteract()
    {
        if (player != null)
        {
            player.rootWord = rootWord;
            player.TriggerWordBuilding();
            Debug.Log($"Projectile was interacted, root word: {rootWord}");
        }
        else
        {
            Debug.Log("Projectile was interacted");
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        enemy?.OnProjectileDestroyed(gameObject);
    }
}
