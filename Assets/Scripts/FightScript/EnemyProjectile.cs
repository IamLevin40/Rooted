using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyProjectile : MonoBehaviour
{
    public float maxSpeed = 4f;
    public float acceleration = 1.5f;
    public float initialSpeed = 0f;
    public float initialSlowDuration = 1f;
    public float curveAmplitude = 1.2f;
    public float curveFrequency = 1.2f;
    public bool moveRight = true;
    public EnemyScript enemy;
    public PlayerScript player;
    public Vector3 spawnOrigin;
    private bool hasHitPlayer = false;
    private float t = 0f;
    private float currentSpeed = 0f;
    public Button interactButton;
    public Text wordText;
    public string rootWord = "";
    private static List<string> rootWordsList;

    private void Start()
    {
        if (interactButton != null)
        {
            interactButton.onClick.AddListener(OnProjectileInteract);
        }
        currentSpeed = initialSpeed;
        SetRandomRootWord();
    }

    private void Update()
    {
        if (player == null || enemy == null) return;
        Transform playerTransform = player.transform;
        t += Time.deltaTime;
        UpdateProjectileMovement(playerTransform);
        CheckPlayerCollision(playerTransform);
    }

    private void SetRandomRootWord()
    {
        if (rootWordsList == null)
        {
            rootWordsList = new List<string>();
            // Load from Resources/ExternalFiles/rootwords_list.txt
            TextAsset txt = Resources.Load<TextAsset>("ExternalFiles/rootwords_list");
            if (txt != null)
            {
                using (System.IO.StringReader reader = new System.IO.StringReader(txt.text))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            rootWordsList.Add(line.Trim());
                    }
                }
            }
        }
        if (rootWordsList != null && rootWordsList.Count > 0)
        {
            int idx = Random.Range(0, rootWordsList.Count);
            rootWord = rootWordsList[idx];
            if (wordText == null)
            {
                // Try to find Text component in children
                wordText = GetComponentInChildren<Text>();
            }
            if (wordText != null)
                wordText.text = rootWord;
        }
    }

    private void UpdateProjectileMovement(Transform playerTransform)
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

        // Calculate progress (0=start, 1=at player)
        float totalDist = Vector3.Distance(spawnOrigin, playerTransform.position);
        float traveledDist = currentSpeed * t;
        float progress = Mathf.Clamp01(traveledDist / totalDist);

        // Linear path
        Vector3 linearPos = Vector3.Lerp(spawnOrigin, playerTransform.position, progress);

        // Curvy offset (sinusoidal perpendicular to path)
        Vector3 pathDir = (playerTransform.position - spawnOrigin).normalized;
        Vector3 perp = Vector3.Cross(pathDir, Vector3.forward).normalized;
        float curve = Mathf.Sin(progress * Mathf.PI * curveFrequency) * curveAmplitude * (moveRight ? 1 : -1);
        Vector3 curvyPos = linearPos + perp * curve * (1f - progress); // curve fades as it nears player

        transform.position = curvyPos;
    }

    private void CheckPlayerCollision(Transform playerTransform)
    {
        if (!hasHitPlayer && Vector3.Distance(transform.position, playerTransform.position) < 0.5f)
        {
            player.SubtractHealth(5);
            hasHitPlayer = true;
            if (player != null) player.rootWord = "";
            if (enemy != null) enemy.OnProjectileDestroyed(gameObject);
            Destroy(gameObject);
        }
    }

    // Call this from the Button's OnClick event
    public void OnProjectileInteract()
    {
        if (enemy != null)
        {
            enemy.SubtractHealth(5);
            enemy.OnProjectileDestroyed(gameObject);
        }
        if (player != null)
        {
            player.rootWord = rootWord;
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
        if (enemy != null)
        {
            enemy.OnProjectileDestroyed(gameObject);
        }
    }
}
