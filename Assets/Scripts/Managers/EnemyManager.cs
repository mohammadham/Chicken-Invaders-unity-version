using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all enemies in the game - formation, spawning, behavior
/// Equivalent to enemy management logic from C++ Main.cpp
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform enemyContainer;
    public int totalEnemies = 30;
    public float enemyFireRate = 0.5f;
    
    [Header("Formation Settings")]
    public Vector3 formationCenter = Vector3.zero;
    public float horizontalSpacing = 1f;
    public float verticalSpacing = 1f;
    public int enemiesPerRow = 10;
    public int rows = 3;
    
    [Header("Sprites")]
    public Sprite[] chickenSprites;
    public Sprite[] explosionSprites;
    
    [Header("Audio")]
    public AudioClip enemyHitSound;
    public AudioClip enemyExplosionSound;
    
    private List<EnemyController> enemies = new List<EnemyController>();
    private float enemyFireTimer = 0f;
    private int enemiesDestroyed = 0;
    
    public static EnemyManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        CreateEnemyFormation();
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.gameStarted || GameManager.Instance.gamePaused))
            return;
        
        HandleEnemyFiring();
        UpdateEnemies();
    }
    
    void CreateEnemyFormation()
    {
        enemies.Clear();
        enemiesDestroyed = 0;
        
        // Create enemy formation similar to C++ version
        // 3 rows of 10 enemies each
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < enemiesPerRow; col++)
            {
                Vector3 position = formationCenter + new Vector3(
                    (col - (enemiesPerRow - 1) / 2f) * horizontalSpacing,
                    (2 - row) * verticalSpacing,
                    0f
                );
                
                CreateEnemy(position, row * enemiesPerRow + col, row);
            }
        }
        
        Debug.Log($"Created {enemies.Count} enemies in formation");
    }
    
    void CreateEnemy(Vector3 position, int enemyIndex, int row)
    {
        GameObject enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemyObj.name = $"Enemy_{enemyIndex}";
        enemyObj.transform.SetParent(enemyContainer);
        
        EnemyController enemyController = enemyObj.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.Initialize(enemyIndex, position);
            
            // Set sprites
            if (chickenSprites != null && chickenSprites.Length > 0)
            {
                enemyController.idleSprites = chickenSprites;
                
                SpriteRenderer spriteRenderer = enemyObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                    spriteRenderer.sprite = chickenSprites[0];
            }
            
            if (explosionSprites != null)
            {
                enemyController.explosionSprites = explosionSprites;
            }
            
            // Set movement boundaries based on position
            float leftBound = position.x - 0.5f;
            float rightBound = position.x + 0.5f;
            enemyController.startPoint = leftBound;
            enemyController.endPoint = rightBound;
            
            enemies.Add(enemyController);
        }
    }
    
    void HandleEnemyFiring()
    {
        enemyFireTimer += Time.deltaTime;
        
        if (enemyFireTimer >= enemyFireRate)
        {
            enemyFireTimer = 0f;
            
            // Fire random enemies (similar to C++ logic)
            List<EnemyController> aliveEnemies = GetAliveEnemies();
            
            if (aliveEnemies.Count > 0)
            {
                // Fire up to 2 enemies randomly
                int firesToMake = Mathf.Min(2, aliveEnemies.Count);
                
                for (int i = 0; i < firesToMake; i++)
                {
                    int randomIndex = Random.Range(0, aliveEnemies.Count);
                    EnemyController randomEnemy = aliveEnemies[randomIndex];
                    
                    if (randomEnemy != null && randomEnemy.IsAlive())
                    {
                        randomEnemy.Shoot();
                    }
                }
            }
        }
    }
    
    void UpdateEnemies()
    {
        // Clean up destroyed enemies
        enemies.RemoveAll(enemy => enemy == null);
        
        // Check win condition
        List<EnemyController> aliveEnemies = GetAliveEnemies();
        if (aliveEnemies.Count == 0)
        {
            OnAllEnemiesDestroyed();
        }
        
        // Update enemy speed based on remaining count
        UpdateEnemySpeed(aliveEnemies.Count);
    }
    
    void UpdateEnemySpeed(int aliveCount)
    {
        // Increase speed as fewer enemies remain
        float speedMultiplier = 1f + (1f - (float)aliveCount / totalEnemies) * 2f;
        
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.IsAlive())
            {
                // Apply speed multiplier but preserve original logic
                // This affects the individual enemy speed calculations
            }
        }
    }
    
    public List<EnemyController> GetAliveEnemies()
    {
        return enemies.FindAll(enemy => enemy != null && enemy.IsAlive());
    }
    
    public void OnEnemyDestroyed(EnemyController enemy)
    {
        enemiesDestroyed++;
        
        // Update enemy fire rate based on remaining enemies
        List<EnemyController> aliveEnemies = GetAliveEnemies();
        
        if (aliveEnemies.Count < 10)
        {
            // Increase fire rate when fewer enemies remain
            enemyFireRate = Mathf.Max(0.2f, enemyFireRate * 0.9f);
        }
        
        Debug.Log($"Enemy destroyed. Remaining: {aliveEnemies.Count}");
    }
    
    public void RemoveEnemy(EnemyController enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }
    
    void OnAllEnemiesDestroyed()
    {
        // All enemies destroyed - victory condition
        if (GameManager.Instance != null)
        {
            // Could trigger next wave or victory
            Debug.Log("All enemies destroyed! Victory!");
        }
    }
    
    public void ClearAllEnemies()
    {
        foreach (var enemy in enemies.ToArray())
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        
        enemies.Clear();
        enemiesDestroyed = 0;
    }
    
    public void SpawnNewWave()
    {
        // Clear existing enemies and create new formation
        ClearAllEnemies();
        
        // Wait a moment then create new formation
        StartCoroutine(SpawnWaveAfterDelay(2f));
    }
    
    IEnumerator SpawnWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CreateEnemyFormation();
    }
    
    // Public getters for other systems
    public int GetTotalEnemyCount() => enemies.Count;
    public int GetAliveEnemyCount() => GetAliveEnemies().Count;
    public int GetEnemiesDestroyed() => enemiesDestroyed;
    public float GetEnemyFireRate() => enemyFireRate;
    
    // Get random alive enemy (useful for targeting)
    public EnemyController GetRandomAliveEnemy()
    {
        List<EnemyController> aliveEnemies = GetAliveEnemies();
        if (aliveEnemies.Count > 0)
        {
            return aliveEnemies[Random.Range(0, aliveEnemies.Count)];
        }
        return null;
    }
    
    // Get enemy at specific position (useful for formations)
    public EnemyController GetEnemyAt(int row, int col)
    {
        int index = row * enemiesPerRow + col;
        if (index >= 0 && index < enemies.Count)
        {
            return enemies[index];
        }
        return null;
    }
}