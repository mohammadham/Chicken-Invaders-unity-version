using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all UI elements in the game
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public Image[] player1LivesIcons;
    public Image[] player2LivesIcons;
    
    [Header("Game Screens")]
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    
    [Header("Buttons")]
    public Button playButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;
    
    [Header("Settings")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicToggle;
    public Toggle sfxToggle;
    
    private long player1Score = 0;
    private long player2Score = 0;
    
    public static UIManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        SetupUI();
    }
    
    private void Start()
    {
        ShowMainMenu();
        SetupEventListeners();
    }
    
    void SetupUI()
    {
        // Ensure all panels are initially inactive except main menu
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }
    
    void SetupEventListeners()
    {
        // Button listeners
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        // Audio settings listeners
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        
        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        
        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(ToggleSFX);
    }
    
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }
    
    public void ShowGameplay()
    {
        SetActivePanel(gameplayPanel);
        UpdateScoreDisplay();
    }
    
    public void ShowPauseMenu()
    {
        SetActivePanel(pausePanel);
    }
    
    public void ShowGameOverScreen()
    {
        SetActivePanel(gameOverPanel);
    }
    
    public void ShowVictoryScreen()
    {
        SetActivePanel(victoryPanel);
    }
    
    void SetActivePanel(GameObject activePanel)
    {
        // Hide all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        
        // Show active panel
        if (activePanel != null)
            activePanel.SetActive(true);
    }
    
    public void UpdatePlayerScore(int playerIndex, long score)
    {
        if (playerIndex == 0)
        {
            player1Score = score;
            if (player1ScoreText != null)
                player1ScoreText.text = player1Score.ToString();
        }
        else if (playerIndex == 1)
        {
            player2Score = score;
            if (player2ScoreText != null)
                player2ScoreText.text = player2Score.ToString();
        }
    }
    
    public void AddScore(long points)
    {
        // Add to both players for now - will be modified when we track which player killed enemy
        player1Score += points;
        player2Score += points;
        UpdateScoreDisplay();
    }
    
    void UpdateScoreDisplay()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = player1Score.ToString();
        
        if (player2ScoreText != null)
            player2ScoreText.text = player2Score.ToString();
    }
    
    public void UpdatePlayerLives(PlayerController player)
    {
        Image[] livesIcons = player.playerIndex == 0 ? player1LivesIcons : player2LivesIcons;
        
        if (livesIcons != null)
        {
            for (int i = 0; i < livesIcons.Length; i++)
            {
                livesIcons[i].enabled = i < player.lives;
            }
        }
    }
    
    // Button event handlers
    void StartGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        ShowGameplay();
        
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }
    
    void PauseGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        ShowPauseMenu();
        
        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();
    }
    
    void ResumeGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        ShowGameplay();
        
        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame(); // Toggle pause off
    }
    
    void RestartGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
    
    void QuitGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
        
        Application.Quit();
    }
    
    // Audio settings handlers
    void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(volume);
    }
    
    void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(volume);
    }
    
    void ToggleMusic(bool enabled)
    {
        if (AudioManager.Instance != null && !enabled)
            AudioManager.Instance.ToggleMusic();
    }
    
    void ToggleSFX(bool enabled)
    {
        if (AudioManager.Instance != null && !enabled)
            AudioManager.Instance.ToggleSFX();
    }
}