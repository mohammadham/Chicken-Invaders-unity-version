using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Comprehensive Settings UI with all game options
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Settings Panels")]
    public GameObject settingsMainPanel;
    public GameObject audioSettingsPanel;
    public GameObject graphicsSettingsPanel;
    public GameObject controlsSettingsPanel;
    
    [Header("Navigation Buttons")]
    public Button audioTabButton;
    public Button graphicsTabButton;
    public Button controlsTabButton;
    public Button backButton;
    public Button resetDefaultsButton;
    
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicEnabledToggle;
    public Toggle sfxEnabledToggle;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    
    [Header("Graphics Settings")]
    public Dropdown qualityDropdown;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Slider frameRateSlider;
    public TextMeshProUGUI frameRateText;
    
    [Header("Controls Settings")]
    public Slider touchSensitivitySlider;
    public Toggle invertYAxisToggle;
    public Toggle hapticFeedbackToggle;
    public Toggle autoFireToggle;
    public TextMeshProUGUI touchSensitivityText;
    public Button calibrateTouchButton;
    
    [Header("Tab Colors")]
    public Color activeTabColor = Color.white;
    public Color inactiveTabColor = Color.gray;
    
    // Current settings values
    private SettingsData currentSettings;
    
    public static SettingsUI Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        currentSettings = new SettingsData();
    }
    
    private void Start()
    {
        InitializeSettings();
        SetupEventListeners();
        LoadSettings();
        ShowPanel(audioSettingsPanel); // Show audio panel by default
    }
    
    void InitializeSettings()
    {
        // Setup quality dropdown
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            List<string> qualityOptions = new List<string>();
            string[] qualityNames = QualitySettings.names;
            qualityOptions.AddRange(qualityNames);
            qualityDropdown.AddOptions(qualityOptions);
        }
        
        // Setup resolution dropdown
        SetupResolutionDropdown();
        
        // Initialize text displays
        UpdateVolumeTexts();
        UpdateFrameRateText();
        UpdateTouchSensitivityText();
    }
    
    void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        Resolution[] resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        
        // Filter unique resolutions
        List<Resolution> filteredResolutions = new List<Resolution>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            bool duplicate = false;
            for (int j = 0; j < filteredResolutions.Count; j++)
            {
                if (filteredResolutions[j].width == resolutions[i].width &&
                    filteredResolutions[j].height == resolutions[i].height)
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate)
                filteredResolutions.Add(resolutions[i]);
        }
        
        // Add resolution options
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + " x " + filteredResolutions[i].height;
            options.Add(option);
            
            if (filteredResolutions[i].width == Screen.currentResolution.width &&
                filteredResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    
    void SetupEventListeners()
    {
        // Navigation buttons
        if (audioTabButton != null)
            audioTabButton.onClick.AddListener(() => ShowPanel(audioSettingsPanel));
            
        if (graphicsTabButton != null)
            graphicsTabButton.onClick.AddListener(() => ShowPanel(graphicsSettingsPanel));
            
        if (controlsTabButton != null)
            controlsTabButton.onClick.AddListener(() => ShowPanel(controlsSettingsPanel));
            
        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);
            
        if (resetDefaultsButton != null)
            resetDefaultsButton.onClick.AddListener(ResetToDefaults);
        
        // Audio settings
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            
        if (musicEnabledToggle != null)
            musicEnabledToggle.onValueChanged.AddListener(ToggleMusic);
            
        if (sfxEnabledToggle != null)
            sfxEnabledToggle.onValueChanged.AddListener(ToggleSFX);
        
        // Graphics settings
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
            
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            
        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(SetVSync);
            
        if (frameRateSlider != null)
            frameRateSlider.onValueChanged.AddListener(SetFrameRate);
        
        // Controls settings
        if (touchSensitivitySlider != null)
            touchSensitivitySlider.onValueChanged.AddListener(SetTouchSensitivity);
            
        if (invertYAxisToggle != null)
            invertYAxisToggle.onValueChanged.AddListener(SetInvertYAxis);
            
        if (hapticFeedbackToggle != null)
            hapticFeedbackToggle.onValueChanged.AddListener(SetHapticFeedback);
            
        if (autoFireToggle != null)
            autoFireToggle.onValueChanged.AddListener(SetAutoFire);
            
        if (calibrateTouchButton != null)
            calibrateTouchButton.onClick.AddListener(CalibrateTouch);
    }
    
    void LoadSettings()
    {
        // Load audio settings
        currentSettings.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        currentSettings.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        currentSettings.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        currentSettings.musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        currentSettings.sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
        
        // Load graphics settings
        currentSettings.qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        currentSettings.fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        currentSettings.vsync = PlayerPrefs.GetInt("VSync", QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        currentSettings.targetFrameRate = PlayerPrefs.GetInt("FrameRate", 60);
        
        // Load controls settings
        currentSettings.touchSensitivity = PlayerPrefs.GetFloat("TouchSensitivity", 2f);
        currentSettings.invertYAxis = PlayerPrefs.GetInt("InvertYAxis", 0) == 1;
        currentSettings.hapticFeedback = PlayerPrefs.GetInt("HapticFeedback", 1) == 1;
        currentSettings.autoFire = PlayerPrefs.GetInt("AutoFire", 0) == 1;
        
        // Apply settings to UI
        ApplySettingsToUI();
        
        // Apply settings to game
        ApplySettings();
    }
    
    void ApplySettingsToUI()
    {
        // Audio UI
        if (masterVolumeSlider != null) masterVolumeSlider.value = currentSettings.masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = currentSettings.musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = currentSettings.sfxVolume;
        if (musicEnabledToggle != null) musicEnabledToggle.isOn = currentSettings.musicEnabled;
        if (sfxEnabledToggle != null) sfxEnabledToggle.isOn = currentSettings.sfxEnabled;
        
        // Graphics UI
        if (qualityDropdown != null) qualityDropdown.value = currentSettings.qualityLevel;
        if (fullscreenToggle != null) fullscreenToggle.isOn = currentSettings.fullscreen;
        if (vsyncToggle != null) vsyncToggle.isOn = currentSettings.vsync;
        if (frameRateSlider != null) frameRateSlider.value = currentSettings.targetFrameRate;
        
        // Controls UI
        if (touchSensitivitySlider != null) touchSensitivitySlider.value = currentSettings.touchSensitivity;
        if (invertYAxisToggle != null) invertYAxisToggle.isOn = currentSettings.invertYAxis;
        if (hapticFeedbackToggle != null) hapticFeedbackToggle.isOn = currentSettings.hapticFeedback;
        if (autoFireToggle != null) autoFireToggle.isOn = currentSettings.autoFire;
        
        // Update text displays
        UpdateVolumeTexts();
        UpdateFrameRateText();
        UpdateTouchSensitivityText();
    }
    
    void ApplySettings()
    {
        // Apply audio settings
        AudioListener.volume = currentSettings.masterVolume;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(currentSettings.musicVolume);
            AudioManager.Instance.SetSFXVolume(currentSettings.sfxVolume);
            
            if (!currentSettings.musicEnabled)
                AudioManager.Instance.ToggleMusic();
                
            if (!currentSettings.sfxEnabled)
                AudioManager.Instance.ToggleSFX();
        }
        
        // Apply graphics settings
        QualitySettings.SetQualityLevel(currentSettings.qualityLevel);
        Screen.fullScreen = currentSettings.fullscreen;
        QualitySettings.vSyncCount = currentSettings.vsync ? 1 : 0;
        Application.targetFrameRate = currentSettings.targetFrameRate;
        
        // Apply controls settings
        if (InputManager.Instance != null)
        {
            // InputManager would need methods to handle these settings
        }
    }
    
    void SaveSettings()
    {
        // Save audio settings
        PlayerPrefs.SetFloat("MasterVolume", currentSettings.masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", currentSettings.musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", currentSettings.sfxVolume);
        PlayerPrefs.SetInt("MusicEnabled", currentSettings.musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", currentSettings.sfxEnabled ? 1 : 0);
        
        // Save graphics settings
        PlayerPrefs.SetInt("QualityLevel", currentSettings.qualityLevel);
        PlayerPrefs.SetInt("Fullscreen", currentSettings.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", currentSettings.vsync ? 1 : 0);
        PlayerPrefs.SetInt("FrameRate", currentSettings.targetFrameRate);
        
        // Save controls settings
        PlayerPrefs.SetFloat("TouchSensitivity", currentSettings.touchSensitivity);
        PlayerPrefs.SetInt("InvertYAxis", currentSettings.invertYAxis ? 1 : 0);
        PlayerPrefs.SetInt("HapticFeedback", currentSettings.hapticFeedback ? 1 : 0);
        PlayerPrefs.SetInt("AutoFire", currentSettings.autoFire ? 1 : 0);
        
        PlayerPrefs.Save();
    }
    
    public void ShowPanel(GameObject panelToShow)
    {
        // Hide all panels
        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
        if (graphicsSettingsPanel != null) graphicsSettingsPanel.SetActive(false);
        if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(false);
        
        // Show target panel
        if (panelToShow != null)
            panelToShow.SetActive(true);
            
        // Update tab colors
        UpdateTabColors(panelToShow);
        
        // Play button sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    void UpdateTabColors(GameObject activePanel)
    {
        // Reset all tab colors
        SetButtonColor(audioTabButton, inactiveTabColor);
        SetButtonColor(graphicsTabButton, inactiveTabColor);
        SetButtonColor(controlsTabButton, inactiveTabColor);
        
        // Set active tab color
        if (activePanel == audioSettingsPanel)
            SetButtonColor(audioTabButton, activeTabColor);
        else if (activePanel == graphicsSettingsPanel)
            SetButtonColor(graphicsTabButton, activeTabColor);
        else if (activePanel == controlsSettingsPanel)
            SetButtonColor(controlsTabButton, activeTabColor);
    }
    
    void SetButtonColor(Button button, Color color)
    {
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            button.colors = colors;
        }
    }
    
    // Audio settings handlers
    void SetMasterVolume(float volume)
    {
        currentSettings.masterVolume = volume;
        AudioListener.volume = volume;
        UpdateVolumeTexts();
        SaveSettings();
    }
    
    void SetMusicVolume(float volume)
    {
        currentSettings.musicVolume = volume;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(volume);
        UpdateVolumeTexts();
        SaveSettings();
    }
    
    void SetSFXVolume(float volume)
    {
        currentSettings.sfxVolume = volume;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(volume);
        UpdateVolumeTexts();
        SaveSettings();
    }
    
    void ToggleMusic(bool enabled)
    {
        currentSettings.musicEnabled = enabled;
        if (AudioManager.Instance != null && !enabled)
            AudioManager.Instance.ToggleMusic();
        SaveSettings();
    }
    
    void ToggleSFX(bool enabled)
    {
        currentSettings.sfxEnabled = enabled;
        if (AudioManager.Instance != null && !enabled)
            AudioManager.Instance.ToggleSFX();
        SaveSettings();
    }
    
    // Graphics settings handlers
    void SetQualityLevel(int qualityIndex)
    {
        currentSettings.qualityLevel = qualityIndex;
        QualitySettings.SetQualityLevel(qualityIndex);
        SaveSettings();
    }
    
    void SetResolution(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, currentSettings.fullscreen);
        }
    }
    
    void SetFullscreen(bool fullscreen)
    {
        currentSettings.fullscreen = fullscreen;
        Screen.fullScreen = fullscreen;
        SaveSettings();
    }
    
    void SetVSync(bool vsync)
    {
        currentSettings.vsync = vsync;
        QualitySettings.vSyncCount = vsync ? 1 : 0;
        SaveSettings();
    }
    
    void SetFrameRate(float frameRate)
    {
        currentSettings.targetFrameRate = Mathf.RoundToInt(frameRate);
        Application.targetFrameRate = currentSettings.targetFrameRate;
        UpdateFrameRateText();
        SaveSettings();
    }
    
    // Controls settings handlers
    void SetTouchSensitivity(float sensitivity)
    {
        currentSettings.touchSensitivity = sensitivity;
        UpdateTouchSensitivityText();
        SaveSettings();
    }
    
    void SetInvertYAxis(bool invert)
    {
        currentSettings.invertYAxis = invert;
        SaveSettings();
    }
    
    void SetHapticFeedback(bool enabled)
    {
        currentSettings.hapticFeedback = enabled;
        SaveSettings();
    }
    
    void SetAutoFire(bool enabled)
    {
        currentSettings.autoFire = enabled;
        SaveSettings();
    }
    
    void CalibrateTouch()
    {
        // Implement touch calibration routine
        StartCoroutine(TouchCalibrationRoutine());
    }
    
    IEnumerator TouchCalibrationRoutine()
    {
        // This would implement a touch calibration screen
        Debug.Log("Touch calibration started...");
        yield return new WaitForSecondsRealtime(2f);
        Debug.Log("Touch calibration completed!");
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    // UI update methods
    void UpdateVolumeTexts()
    {
        if (masterVolumeText != null)
            masterVolumeText.text = $"{(currentSettings.masterVolume * 100):F0}%";
            
        if (musicVolumeText != null)
            musicVolumeText.text = $"{(currentSettings.musicVolume * 100):F0}%";
            
        if (sfxVolumeText != null)
            sfxVolumeText.text = $"{(currentSettings.sfxVolume * 100):F0}%";
    }
    
    void UpdateFrameRateText()
    {
        if (frameRateText != null)
            frameRateText.text = $"{currentSettings.targetFrameRate} FPS";
    }
    
    void UpdateTouchSensitivityText()
    {
        if (touchSensitivityText != null)
            touchSensitivityText.text = $"{currentSettings.touchSensitivity:F1}x";
    }
    
    // Public methods
    void ResetToDefaults()
    {
        currentSettings = new SettingsData(); // Reset to default values
        ApplySettingsToUI();
        ApplySettings();
        SaveSettings();
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    void CloseSettings()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        gameObject.SetActive(false);
    }
    
    public void ShowSettings()
    {
        gameObject.SetActive(true);
        ShowPanel(audioSettingsPanel);
    }
    
    // Settings data structure
    [System.Serializable]
    public class SettingsData
    {
        // Audio settings
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.8f;
        public bool musicEnabled = true;
        public bool sfxEnabled = true;
        
        // Graphics settings
        public int qualityLevel = 2;
        public bool fullscreen = true;
        public bool vsync = true;
        public int targetFrameRate = 60;
        
        // Controls settings
        public float touchSensitivity = 2f;
        public bool invertYAxis = false;
        public bool hapticFeedback = true;
        public bool autoFire = false;
    }
}