using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pause Menu UI with settings and options
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Pause Menu Elements")]
    public GameObject pauseMenuPanel;
    public TextMeshProUGUI pausedTitleText;
    
    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Button backFromSettingsButton;
    
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicToggle;
    public Toggle sfxToggle;
    
    [Header("Graphics Settings")]
    public Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;
    
    [Header("Controls Settings")]
    public Slider touchSensitivitySlider;
    public Toggle vibrationToggle;
    
    [Header("Animation")]
    public AnimationCurve scaleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float animationDuration = 0.3f;
    
    private bool isAnimating = false;
    private Vector3 originalScale;
    
    public static PauseMenuUI Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        originalScale = transform.localScale;
    }
    
    private void Start()
    {
        InitializePauseMenu();
        SetupEventListeners();
        LoadSettings();
    }
    
    void InitializePauseMenu()
    {
        // Hide pause menu initially
        gameObject.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    void SetupEventListeners()
    {
        // Main pause menu buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettings);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        // Settings panel
        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(HideSettings);
            
        // Audio settings
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            
        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(ToggleMusic);
            
        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(ToggleSFX);
            
        // Graphics settings
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
            
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            
        // Controls
        if (touchSensitivitySlider != null)
            touchSensitivitySlider.onValueChanged.AddListener(SetTouchSensitivity);
            
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(ToggleVibration);
    }
    
    void LoadSettings()
    {
        // Load audio settings
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            
        if (musicToggle != null)
            musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            
        if (sfxToggle != null)
            sfxToggle.isOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
            
        // Load graphics settings
        if (qualityDropdown != null)
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;
            
        // Load controls settings
        if (touchSensitivitySlider != null)
            touchSensitivitySlider.value = PlayerPrefs.GetFloat("TouchSensitivity", 2f);
            
        if (vibrationToggle != null)
            vibrationToggle.isOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
            
        // Setup resolution dropdown
        SetupResolutionDropdown();
    }
    
    void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        Resolution[] resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    
    public void ShowPauseMenu()
    {
        if (isAnimating) return;
        
        gameObject.SetActive(true);
        Time.timeScale = 0f; // Pause game
        
        StartCoroutine(AnimateMenuShow());
        
        // Play pause sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
    
    public void HidePauseMenu()
    {
        if (isAnimating) return;
        
        StartCoroutine(AnimateMenuHide());
    }
    
    IEnumerator AnimateMenuShow()
    {
        isAnimating = true;
        
        // Start from scale 0 and animate to original scale
        transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = scaleAnimationCurve.Evaluate(elapsed / animationDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
        isAnimating = false;
    }
    
    IEnumerator AnimateMenuHide()
    {
        isAnimating = true;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = scaleAnimationCurve.Evaluate(elapsed / animationDuration);
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            yield return null;
        }
        
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
        Time.timeScale = 1f; // Resume game
        isAnimating = false;
    }
    
    // Button event handlers
    void ResumeGame()
    {
        PlayButtonSound();
        
        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame(); // Toggle pause off
        else
            HidePauseMenu();
    }
    
    void ShowSettings()
    {
        PlayButtonSound();
        
        if (settingsPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
    }
    
    void HideSettings()
    {
        PlayButtonSound();
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            pauseMenuPanel.SetActive(true);
        }
    }
    
    void RestartGame()
    {
        PlayButtonSound();
        
        Time.timeScale = 1f;
        
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
    
    void ReturnToMainMenu()
    {
        PlayButtonSound();
        
        Time.timeScale = 1f;
        
        // Show main menu
        if (MainMenuUI.Instance != null)
            MainMenuUI.Instance.ShowMainMenu();
            
        // Hide gameplay UI
        if (UIManager.Instance != null)
            UIManager.Instance.ShowMainMenu();
            
        HidePauseMenu();
    }
    
    void QuitGame()
    {
        PlayButtonSound();
        
        StartCoroutine(QuitGameCoroutine());
    }
    
    IEnumerator QuitGameCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // Settings event handlers
    void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }
    
    void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    void ToggleMusic(bool enabled)
    {
        if (AudioManager.Instance != null)
        {
            if (!enabled)
                AudioManager.Instance.ToggleMusic();
        }
        PlayerPrefs.SetInt("MusicEnabled", enabled ? 1 : 0);
    }
    
    void ToggleSFX(bool enabled)
    {
        if (AudioManager.Instance != null)
        {
            if (!enabled)
                AudioManager.Instance.ToggleSFX();
        }
        PlayerPrefs.SetInt("SFXEnabled", enabled ? 1 : 0);
    }
    
    void SetQualityLevel(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }
    
    void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
    }
    
    void SetResolution(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
    
    void SetTouchSensitivity(float sensitivity)
    {
        if (InputManager.Instance != null)
        {
            // InputManager would need a method to set sensitivity
            PlayerPrefs.SetFloat("TouchSensitivity", sensitivity);
        }
    }
    
    void ToggleVibration(bool enabled)
    {
        PlayerPrefs.SetInt("VibrationEnabled", enabled ? 1 : 0);
        // Mobile vibration would be implemented here
    }
    
    void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    private void OnEnable()
    {
        // Ensure proper time scale when menu is shown
        if (gameObject.activeInHierarchy)
            Time.timeScale = 0f;
    }
    
    private void OnDisable()
    {
        // Ensure time scale is restored when menu is hidden
        if (!isAnimating)
            Time.timeScale = 1f;
    }
}