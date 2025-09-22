using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Enhanced Gameplay HUD with advanced features
/// Shows score, lives, weapon info, overheat bars, mini-map, etc.
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [Header("Player 1 HUD")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player1LivesText;
    public Image[] player1LifeIcons;
    public Slider player1OverheatBar;
    public TextMeshProUGUI player1WeaponText;
    public TextMeshProUGUI player1WeaponLevelText;
    public Image player1WeaponIcon;
    
    [Header("Player 2 HUD (Co-op)")]
    public TextMeshProUGUI player2ScoreText;
    public TextMeshProUGUI player2LivesText;
    public Image[] player2LifeIcons;
    public Slider player2OverheatBar;
    public TextMeshProUGUI player2WeaponText;
    public TextMeshProUGUI player2WeaponLevelText;
    public Image player2WeaponIcon;
    
    [Header("Game Status")]
    public TextMeshProUGUI enemiesRemainingText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timeText;
    public Slider bossHealthBar;
    
    [Header("Power-ups & Effects")]
    public GameObject shieldIndicator;
    public GameObject invulnerabilityIndicator;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI bonusScoreText;
    
    [Header("Mini-map")]
    public RectTransform miniMapContainer;
    public RectTransform playerIcon;
    public RectTransform[] enemyIcons;
    
    [Header("UI Controls")]
    public Button pauseButton;
    public Button weaponSwapButton;
    
    [Header("Colors & Animation")]
    public Color normalOverheatColor = Color.green;
    public Color warningOverheatColor = Color.yellow;
    public Color dangerOverheatColor = Color.red;
    public Color criticalOverheatColor = Color.red;
    
    // Private variables
    private PlayerController player1;
    private PlayerController player2;
    private float gameTime = 0f;
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private Coroutine bonusScoreCoroutine;
    
    public static GameplayHUD Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        InitializeHUD();
        SetupEventListeners();
    }
    
    void InitializeHUD()
    {
        // Find player controllers
        FindPlayerControllers();
        
        // Initialize UI elements
        UpdateAllPlayerInfo();
        UpdateGameStatus();
        
        // Setup mini-map
        SetupMiniMap();
        
        // Hide boss health bar initially
        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(false);
        
        // Hide power-up indicators
        if (shieldIndicator != null) shieldIndicator.SetActive(false);
        if (invulnerabilityIndicator != null) invulnerabilityIndicator.SetActive(false);
    }
    
    void FindPlayerControllers()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        
        foreach (var player in players)
        {
            if (player.playerIndex == 0)
                player1 = player;
            else if (player.playerIndex == 1)
                player2 = player;
        }
        
        // Show/hide player 2 HUD based on availability
        SetPlayer2HUDVisibility(player2 != null && !InputManager.Instance.IsUsingTouchControls());
    }
    
    void SetupEventListeners()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
            
        if (weaponSwapButton != null)
            weaponSwapButton.onClick.AddListener(SwapWeapon);
    }
    
    void SetupMiniMap()
    {
        if (miniMapContainer == null) return;
        
        // Initialize mini-map (simplified version)
        // This would need proper implementation based on game camera bounds
    }
    
    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive())
            return;
            
        UpdateAllPlayerInfo();
        UpdateGameStatus();
        UpdateGameTime();
        UpdateComboSystem();
        UpdateMiniMap();
    }
    
    void UpdateAllPlayerInfo()
    {
        // Update Player 1
        if (player1 != null)
        {
            UpdatePlayerHUD(player1, 1);
        }
        
        // Update Player 2
        if (player2 != null)
        {
            UpdatePlayerHUD(player2, 2);
        }
    }
    
    void UpdatePlayerHUD(PlayerController player, int playerNumber)
    {
        // Get UI elements for this player
        TextMeshProUGUI scoreText = playerNumber == 1 ? player1ScoreText : player2ScoreText;
        TextMeshProUGUI livesText = playerNumber == 1 ? player1LivesText : player2LivesText;
        Image[] lifeIcons = playerNumber == 1 ? player1LifeIcons : player2LifeIcons;
        Slider overheatBar = playerNumber == 1 ? player1OverheatBar : player2OverheatBar;
        TextMeshProUGUI weaponText = playerNumber == 1 ? player1WeaponText : player2WeaponText;
        TextMeshProUGUI weaponLevelText = playerNumber == 1 ? player1WeaponLevelText : player2WeaponLevelText;
        Image weaponIcon = playerNumber == 1 ? player1WeaponIcon : player2WeaponIcon;
        
        // Update score
        if (scoreText != null)
        {
            scoreText.text = player.score.ToString("N0");
        }
        
        // Update lives
        if (livesText != null)
        {
            livesText.text = $"x{player.lives}";
        }
        
        // Update life icons
        if (lifeIcons != null)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].gameObject.SetActive(i < player.lives);
                }
            }
        }
        
        // Update weapon overheat
        if (overheatBar != null)
        {
            float overheat = player.GetWeaponOverheat();
            overheatBar.value = overheat;
            
            // Change color based on overheat level
            Image fillImage = overheatBar.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                if (overheat >= 0.9f)
                    fillImage.color = criticalOverheatColor;
                else if (overheat >= 0.7f)
                    fillImage.color = dangerOverheatColor;
                else if (overheat >= 0.4f)
                    fillImage.color = warningOverheatColor;
                else
                    fillImage.color = normalOverheatColor;
            }
        }
        
        // Update weapon info
        WeaponData currentWeapon = player.GetCurrentWeaponData();
        if (currentWeapon != null)
        {
            if (weaponText != null)
                weaponText.text = currentWeapon.weaponName;
                
            if (weaponLevelText != null)
                weaponLevelText.text = $"Lv.{currentWeapon.level}";
                
            // Update weapon icon (if you have weapon sprites)
            if (weaponIcon != null && currentWeapon.weaponSprite != null)
                weaponIcon.sprite = currentWeapon.weaponSprite;
        }
        
        // Update shield indicator
        if (playerNumber == 1 && shieldIndicator != null)
        {
            shieldIndicator.SetActive(player.hasShield);
        }
    }
    
    void UpdateGameStatus()
    {
        // Update enemies remaining
        if (enemiesRemainingText != null && EnemyManager.Instance != null)
        {
            int remaining = EnemyManager.Instance.GetAliveEnemyCount();
            enemiesRemainingText.text = $"Enemies: {remaining}";
        }
        
        // Update wave info
        if (waveText != null)
        {
            waveText.text = "Wave 1"; // This could be dynamic based on game state
        }
    }
    
    void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
        
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
    
    void UpdateComboSystem()
    {
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
        
        // Update combo display
        if (comboText != null)
        {
            if (currentCombo > 1)
            {
                comboText.text = $"COMBO x{currentCombo}";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }
    
    void UpdateMiniMap()
    {
        if (miniMapContainer == null || !miniMapContainer.gameObject.activeInHierarchy)
            return;
            
        // Update player position on mini-map
        if (playerIcon != null && player1 != null)
        {
            Vector3 worldPos = player1.transform.position;
            Vector2 miniMapPos = WorldToMiniMapPosition(worldPos);
            playerIcon.anchoredPosition = miniMapPos;
        }
        
        // Update enemy positions (simplified)
        // This would need proper implementation with object pooling for performance
    }
    
    Vector2 WorldToMiniMapPosition(Vector3 worldPosition)
    {
        // Convert world position to mini-map coordinates
        // This is a simplified version - you'd need proper bounds calculation
        Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float xPercent = (worldPosition.x + screenBounds.x) / (screenBounds.x * 2);
        float yPercent = (worldPosition.y + screenBounds.y) / (screenBounds.y * 2);
        
        RectTransform mapRect = miniMapContainer.GetComponent<RectTransform>();
        float mapWidth = mapRect.rect.width;
        float mapHeight = mapRect.rect.height;
        
        return new Vector2(
            (xPercent - 0.5f) * mapWidth,
            (yPercent - 0.5f) * mapHeight
        );
    }
    
    // Public methods for game events
    public void OnEnemyKilled()
    {
        currentCombo++;
        comboTimer = 3f; // 3 seconds to continue combo
        
        // Show bonus score if combo is high enough
        if (currentCombo >= 5)
        {
            ShowBonusScore(currentCombo * 100);
        }
    }
    
    public void ResetCombo()
    {
        currentCombo = 0;
        comboTimer = 0f;
    }
    
    public void ShowBonusScore(int bonus)
    {
        if (bonusScoreText != null)
        {
            if (bonusScoreCoroutine != null)
                StopCoroutine(bonusScoreCoroutine);
                
            bonusScoreCoroutine = StartCoroutine(AnimateBonusScore(bonus));
        }
    }
    
    IEnumerator AnimateBonusScore(int bonus)
    {
        bonusScoreText.text = $"+{bonus}";
        bonusScoreText.gameObject.SetActive(true);
        
        // Animate scale and fade
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * 1.5f;
        Color startColor = bonusScoreText.color;
        Color endColor = startColor;
        endColor.a = 0f;
        
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            bonusScoreText.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            bonusScoreText.color = Color.Lerp(startColor, endColor, progress);
            
            yield return null;
        }
        
        bonusScoreText.gameObject.SetActive(false);
        bonusScoreText.transform.localScale = startScale;
        bonusScoreText.color = startColor;
    }
    
    public void ShowBossHealthBar(bool show)
    {
        if (bossHealthBar != null)
            bossHealthBar.gameObject.SetActive(show);
    }
    
    public void UpdateBossHealth(float healthPercent)
    {
        if (bossHealthBar != null)
            bossHealthBar.value = healthPercent;
    }
    
    void SetPlayer2HUDVisibility(bool visible)
    {
        GameObject[] player2Elements = {
            player2ScoreText?.gameObject,
            player2LivesText?.gameObject,
            player2OverheatBar?.gameObject,
            player2WeaponText?.gameObject,
            player2WeaponLevelText?.gameObject,
            player2WeaponIcon?.gameObject
        };
        
        foreach (var element in player2Elements)
        {
            if (element != null)
                element.SetActive(visible);
        }
        
        if (player2LifeIcons != null)
        {
            foreach (var icon in player2LifeIcons)
            {
                if (icon != null)
                    icon.gameObject.SetActive(visible);
            }
        }
    }
    
    // Button event handlers
    void PauseGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();
    }
    
    void SwapWeapon()
    {
        // Swap to secondary weapon if available
        if (player1 != null && player1.secondaryWeapon != null)
        {
            // This would need implementation in PlayerController
            Debug.Log("Weapon swap requested");
        }
    }
    
    // Public getters
    public float GetGameTime() => gameTime;
    public int GetCurrentCombo() => currentCombo;
}