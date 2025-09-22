using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main GameManager - Core game controller equivalent to Main.cpp from C++ version
/// Updated for Phase 3 with complete Enemy and Drop management
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
    
    [Header("Drop Settings")]
    public Transform dropContainer;
    
    [Header("Game Objects")]
    public BulletManager bulletManager;
    public WeaponManager weaponManager;
    public AudioManager audioManager;
    public UIManager uiManager;
    public EnemyManager enemyManager;
    public DropManager dropManager;
    
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
        if (enemyManager == null) enemyManager = FindObjectOfType<EnemyManager>();
        if (dropManager == null) dropManager = FindObjectOfType<DropManager>();
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
        gameOver = false;
        gamePaused = false;
        
        CreatePlayers();
        
        if (audioManager != null)
            audioManager.PlayBackgroundMusic();
        
        if (uiManager != null)
            uiManager.ShowGameplay();
        
        Debug.Log("Game started!");
    }
    
    void CreatePlayers()
    {
        players.Clear();
        
        // Create Player 1 (WASD + Space)
        if (playerSpawnPoints.Length > 0)
        {
            GameObject player1 = CreatePlayer(0, "Player1", playerSpawnPoints[0].position);
            PlayerController player1Controller = player1.GetComponent<PlayerController>();
            player1Controller.SetupControls(PlayerController.ControlScheme.WASD);
            players.Add(player1Controller);
        }
        
        // Create Player 2 (Arrow Keys + RShift) - only if not using touch controls
        if (playerSpawnPoints.Length > 1 && InputManager.Instance != null && !InputManager.Instance.IsUsingTouchControls())
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
    
    void GameLoop()
    {
        // Main game loop equivalent to the while loop in C++ main()
        CheckCollisions();
        CheckGameConditions();
    }
    
    void CheckCollisions()
    {
        // Most collision detection is handled by individual controllers
        // This is for global collision management if needed
    }
    
    void CheckGameConditions()
    {
        // Check win condition
        if (enemyManager != null && enemyManager.GetAliveEnemyCount() == 0)
        {
            OnAllEnemiesDestroyed();
        }
        
        // Check game over condition
        bool allPlayersDead = true;
        foreach (var player in players)
        {
            if (player != null && player.IsAlive && player.lives > 0)
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
    
    public void OnPlayerDeath(PlayerController player)
    {
        // Handle player death
        if (uiManager != null)
            uiManager.UpdatePlayerLives(player);
        
        Debug.Log($"Player {player.playerIndex} died. Lives remaining: {player.lives}");
    }
    
    public void OnEnemyDestroyed(EnemyController enemy)
    {
        // Handle enemy destruction and scoring
        // Score is handled by individual players when they collect drops
        Debug.Log($"Enemy {enemy.enemyIndex} destroyed");
    }
    
    void OnAllEnemiesDestroyed()
    {
        // Win condition
        if (uiManager != null)
            uiManager.ShowVictoryScreen();
        
        gameOver = true;
        
        Debug.Log("All enemies destroyed! Victory!");
    }
    
    public void GameOver()
    {
        gameOver = true;
        gameStarted = false;
        
        if (uiManager != null)
            uiManager.ShowGameOverScreen();
        
        if (audioManager != null)
            audioManager.StopBackgroundMusic();
        
        Debug.Log("Game Over!");
    }
    
    public void PauseGame()
    {
        gamePaused = !gamePaused;
        Time.timeScale = gamePaused ? 0f : 1f;
        
        if (uiManager != null)
        {
            if (gamePaused)
                uiManager.ShowPauseMenu();
            else
                uiManager.ShowGameplay();
        }
        
        Debug.Log($"Game {(gamePaused ? "paused" : "resumed")}");
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        
        // Clear all game objects
        if (bulletManager != null)
            bulletManager.ClearAllBullets();
        
        if (enemyManager != null)
            enemyManager.ClearAllEnemies();
        
        if (dropManager != null)
            dropManager.ClearAllDrops();
        
        // Restart scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // Public getters for other systems
    public List<PlayerController> GetPlayers() => players;
    public List<EnemyController> GetEnemies() => enemies;
    public bool IsGameActive() => gameStarted && !gamePaused && !gameOver;
}