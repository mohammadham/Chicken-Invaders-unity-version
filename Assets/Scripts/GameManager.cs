using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main GameManager - Core game controller equivalent to Main.cpp from C++ version
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public bool gameStarted = false;
    public bool gamePaused = false;
    public bool gameOver = false;
    
    [Header("Player Settings")]
    public GameObject playerPrefab;
    public Transform[] playerSpawnPoints;
    public List<PlayerController> players = new List<PlayerController>();
    
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform enemyContainer;
    public List<EnemyController> enemies = new List<EnemyController>();
    public int totalEnemies = 30;
    public float enemyFireRate = 0.5f;
    
    [Header("Game Objects")]
    public BulletManager bulletManager;
    public WeaponManager weaponManager;
    public AudioManager audioManager;
    public UIManager uiManager;
    
    [Header("Background")]
    public SpriteRenderer backgroundRenderer;
    public Sprite[] backgroundSprites;
    
    // Static instance for global access
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeGame();
    }
    
    private void Start()
    {
        SetupBackground();
        StartGame();
    }
    
    private void Update()
    {
        if (!gameStarted || gamePaused || gameOver) return;
        
        GameLoop();
    }
    
    void InitializeGame()
    {
        // Set target framerate (equivalent to window.setFramerateLimit(120))
        Application.targetFrameRate = 120;
        
        // Find or create managers
        FindManagers();
    }
    
    void FindManagers()
    {
        if (bulletManager == null) bulletManager = FindObjectOfType<BulletManager>();
        if (weaponManager == null) weaponManager = FindObjectOfType<WeaponManager>();
        if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
    }
    
    void SetupBackground()
    {
        if (backgroundRenderer != null && backgroundSprites.Length > 0)
        {
            backgroundRenderer.sprite = backgroundSprites[0]; // Space background
        }
    }
    
    public void StartGame()
    {
        gameStarted = true;
        CreatePlayers();
        CreateEnemies();
        
        if (audioManager != null)
            audioManager.PlayBackgroundMusic();
    }
    
    void CreatePlayers()
    {
        // Create Player 1 (WASD + Space)
        if (playerSpawnPoints.Length > 0)
        {
            GameObject player1 = CreatePlayer(0, "Player1", playerSpawnPoints[0].position);
            PlayerController player1Controller = player1.GetComponent<PlayerController>();
            player1Controller.SetupControls(PlayerController.ControlScheme.WASD);
            players.Add(player1Controller);
        }
        
        // Create Player 2 (Arrow Keys + RShift)
        if (playerSpawnPoints.Length > 1)
        {
            GameObject player2 = CreatePlayer(1, "Player2", playerSpawnPoints[1].position);
            PlayerController player2Controller = player2.GetComponent<PlayerController>();
            player2Controller.SetupControls(PlayerController.ControlScheme.Arrows);
            players.Add(player2Controller);
        }
    }
    
    GameObject CreatePlayer(int playerIndex, string playerName, Vector3 position)
    {
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.name = playerName;
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.playerIndex = playerIndex;
            playerController.playerName = playerName;
            playerController.Initialize();
        }
        
        return player;
    }
    
    void CreateEnemies()
    {
        // Create enemy formation similar to C++ version
        // 3 rows of 10 enemies each
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                Vector3 position = new Vector3(
                    -4.5f + col * 1.0f,  // X position
                    4f - row * 1.0f,     // Y position (top to bottom)
                    0f
                );
                
                CreateEnemy(position, row * 10 + col);
            }
        }
    }
    
    void CreateEnemy(Vector3 position, int enemyIndex)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.name = $"Enemy_{enemyIndex}";
        enemy.transform.SetParent(enemyContainer);
        
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.Initialize(enemyIndex, position);
            enemies.Add(enemyController);
        }
    }
    
    void GameLoop()
    {
        // Main game loop equivalent to the while loop in C++ main()
        UpdateEnemies();
        CheckCollisions();
        CleanupDestroyedObjects();
    }
    
    void UpdateEnemies()
    {
        // Handle enemy firing similar to C++ version
        if (Random.Range(0f, 1f) < enemyFireRate * Time.deltaTime)
        {
            List<EnemyController> aliveEnemies = enemies.FindAll(e => e != null && !e.isDead);
            if (aliveEnemies.Count > 0)
            {
                EnemyController randomEnemy = aliveEnemies[Random.Range(0, aliveEnemies.Count)];
                randomEnemy.Shoot();
            }
        }
    }
    
    void CheckCollisions()
    {
        // Handle collision detection between bullets, players, and enemies
        // This will be implemented in individual controllers for better performance
    }
    
    void CleanupDestroyedObjects()
    {
        // Remove destroyed enemies from list
        enemies.RemoveAll(e => e == null);
        
        // Check win condition
        if (enemies.Count == 0)
        {
            OnAllEnemiesDestroyed();
        }
    }
    
    public void OnPlayerDeath(PlayerController player)
    {
        // Handle player death
        if (uiManager != null)
            uiManager.UpdatePlayerLives(player);
        
        // Check game over condition
        bool allPlayersDead = true;
        foreach (var p in players)
        {
            if (p != null && p.lives > 0)
            {
                allPlayersDead = false;
                break;
            }
        }
        
        if (allPlayersDead)
        {
            GameOver();
        }
    }
    
    public void OnEnemyDestroyed(EnemyController enemy)
    {
        // Handle enemy destruction and scoring
        if (uiManager != null)
            uiManager.AddScore(enemy.scoreValue);
        
        // Handle item drops
        enemy.DropItems();
    }
    
    void OnAllEnemiesDestroyed()
    {
        // Win condition
        if (uiManager != null)
            uiManager.ShowVictoryScreen();
        
        gameOver = true;
    }
    
    public void GameOver()
    {
        gameOver = true;
        gameStarted = false;
        
        if (uiManager != null)
            uiManager.ShowGameOverScreen();
        
        if (audioManager != null)
            audioManager.StopBackgroundMusic();
    }
    
    public void PauseGame()
    {
        gamePaused = !gamePaused;
        Time.timeScale = gamePaused ? 0f : 1f;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}