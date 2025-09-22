using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Main Menu UI with professional animations and transitions
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject highScorePanel;
    public GameObject creditsPanel;
    
    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button highScoresButton;
    public Button creditsButton;
    public Button quitButton;
    
    [Header("Settings Buttons")]
    public Button backFromSettingsButton;
    public Button backFromHighScoresButton;
    public Button backFromCreditsButton;
    
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public Image backgroundImage;
    public RectTransform logoTransform;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float buttonAnimationDelay = 0.1f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio")]
    public AudioClip menuMusic;
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;
    
    private List<Button> menuButtons = new List<Button>();
    private bool isTransitioning = false;
    
    public static MainMenuUI Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        InitializeMenuButtons();
    }
    
    private void Start()
    {
        InitializeMenu();
        StartCoroutine(AnimateMenuEntrance());
    }
    
    void InitializeMenuButtons()
    {
        // Collect all main menu buttons for animation
        if (playButton != null) menuButtons.Add(playButton);
        if (settingsButton != null) menuButtons.Add(settingsButton);
        if (highScoresButton != null) menuButtons.Add(highScoresButton);
        if (creditsButton != null) menuButtons.Add(creditsButton);
        if (quitButton != null) menuButtons.Add(quitButton);
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Setup button hover effects
        SetupButtonHoverEffects();
    }
    
    void SetupButtonListeners()
    {
        if (playButton != null)
            playButton.onClick.AddListener(() => StartCoroutine(TransitionToGame()));
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));
            
        if (highScoresButton != null)
            highScoresButton.onClick.AddListener(() => ShowPanel(highScorePanel));
            
        if (creditsButton != null)
            creditsButton.onClick.AddListener(() => ShowPanel(creditsPanel));
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        // Back buttons
        if (backFromSettingsButton != null)
            backFromSettingsButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));
            
        if (backFromHighScoresButton != null)
            backFromHighScoresButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));
            
        if (backFromCreditsButton != null)
            backFromCreditsButton.onClick.AddListener(() => ShowPanel(mainMenuPanel));
    }
    
    void SetupButtonHoverEffects()
    {
        foreach (var button in menuButtons)
        {
            if (button != null)
            {
                var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                    eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                // Hover enter
                var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                hoverEntry.callback.AddListener((data) => OnButtonHover(button));
                eventTrigger.triggers.Add(hoverEntry);
                
                // Hover exit
                var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => OnButtonExit(button));
                eventTrigger.triggers.Add(exitEntry);
            }
        }
    }
    
    void InitializeMenu()
    {
        // Show main menu, hide others
        ShowPanel(mainMenuPanel, false);
        
        // Set initial alpha to 0 for fade-in animation
        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = 0f;
            backgroundImage.color = bgColor;
        }
        
        // Hide buttons initially for entrance animation
        foreach (var button in menuButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }
        
        // Start menu music
        if (AudioManager.Instance != null && menuMusic != null)
        {
            AudioManager.Instance.PlayBackgroundMusic();
        }
    }
    
    IEnumerator AnimateMenuEntrance()
    {
        // Fade in background
        if (backgroundImage != null)
        {
            yield return StartCoroutine(FadeInBackground());
        }
        
        // Animate logo
        if (logoTransform != null)
        {
            yield return StartCoroutine(AnimateLogo());
        }
        
        // Show buttons with staggered animation
        yield return StartCoroutine(AnimateButtonsEntrance());
        
        // Title text animation
        if (titleText != null)
        {
            yield return StartCoroutine(AnimateTitle());
        }
    }
    
    IEnumerator FadeInBackground()
    {
        float elapsed = 0f;
        Color startColor = backgroundImage.color;
        Color targetColor = startColor;
        targetColor.a = 1f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = animationCurve.Evaluate(elapsed / fadeInDuration);
            backgroundImage.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        
        backgroundImage.color = targetColor;
    }
    
    IEnumerator AnimateLogo()
    {
        if (logoTransform == null) yield break;
        
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        logoTransform.localScale = startScale;
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = animationCurve.Evaluate(elapsed / fadeInDuration);
            logoTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }
        
        logoTransform.localScale = targetScale;
    }
    
    IEnumerator AnimateButtonsEntrance()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            if (menuButtons[i] != null)
            {
                menuButtons[i].gameObject.SetActive(true);
                StartCoroutine(AnimateButtonSlideIn(menuButtons[i].transform));
                yield return new WaitForSecondsRealtime(buttonAnimationDelay);
            }
        }
    }
    
    IEnumerator AnimateButtonSlideIn(Transform buttonTransform)
    {
        Vector3 startPos = buttonTransform.localPosition + Vector3.right * 500f;
        Vector3 targetPos = buttonTransform.localPosition;
        buttonTransform.localPosition = startPos;
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration * 0.8f)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = animationCurve.Evaluate(elapsed / (fadeInDuration * 0.8f));
            buttonTransform.localPosition = Vector3.Lerp(startPos, targetPos, progress);
            yield return null;
        }
        
        buttonTransform.localPosition = targetPos;
    }
    
    IEnumerator AnimateTitle()
    {
        if (titleText == null) yield break;
        
        Color startColor = titleText.color;
        startColor.a = 0f;
        Color targetColor = titleText.color;
        titleText.color = startColor;
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / fadeInDuration;
            titleText.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        
        titleText.color = targetColor;
    }
    
    void OnButtonHover(Button button)
    {
        if (isTransitioning) return;
        
        // Play hover sound
        if (AudioManager.Instance != null && buttonHoverSound != null)
        {
            AudioManager.Instance.PlaySFX(buttonHoverSound);
        }
        
        // Scale animation
        StartCoroutine(ScaleButton(button.transform, Vector3.one * 1.1f, 0.1f));
    }
    
    void OnButtonExit(Button button)
    {
        if (isTransitioning) return;
        
        // Scale back to normal
        StartCoroutine(ScaleButton(button.transform, Vector3.one, 0.1f));
    }
    
    IEnumerator ScaleButton(Transform buttonTransform, Vector3 targetScale, float duration)
    {
        Vector3 startScale = buttonTransform.localScale;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }
        
        buttonTransform.localScale = targetScale;
    }
    
    public void ShowPanel(GameObject panelToShow, bool playSound = true)
    {
        if (isTransitioning) return;
        
        if (playSound && AudioManager.Instance != null && buttonClickSound != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound);
        }
        
        StartCoroutine(TransitionToPanel(panelToShow));
    }
    
    IEnumerator TransitionToPanel(GameObject panelToShow)
    {
        isTransitioning = true;
        
        // Fade out current panel
        yield return StartCoroutine(FadeOutCurrentPanel());
        
        // Hide all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (highScorePanel != null) highScorePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        
        // Show target panel
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
            yield return StartCoroutine(FadeInPanel(panelToShow));
        }
        
        isTransitioning = false;
    }
    
    IEnumerator FadeOutCurrentPanel()
    {
        CanvasGroup currentCanvasGroup = GetActivePanel()?.GetComponent<CanvasGroup>();
        if (currentCanvasGroup == null)
        {
            GameObject activePanel = GetActivePanel();
            if (activePanel != null)
                currentCanvasGroup = activePanel.AddComponent<CanvasGroup>();
        }
        
        if (currentCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.unscaledDeltaTime;
                currentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                yield return null;
            }
            currentCanvasGroup.alpha = 0f;
        }
    }
    
    IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        float elapsed = 0f;
        
        while (elapsed < 0.3f)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.3f);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    GameObject GetActivePanel()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeInHierarchy) return mainMenuPanel;
        if (settingsPanel != null && settingsPanel.activeInHierarchy) return settingsPanel;
        if (highScorePanel != null && highScorePanel.activeInHierarchy) return highScorePanel;
        if (creditsPanel != null && creditsPanel.activeInHierarchy) return creditsPanel;
        return null;
    }
    
    IEnumerator TransitionToGame()
    {
        if (isTransitioning) yield break;
        
        isTransitioning = true;
        
        // Play click sound
        if (AudioManager.Instance != null && buttonClickSound != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound);
        }
        
        // Fade out menu
        yield return StartCoroutine(FadeOutCurrentPanel());
        
        // Start game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
            UIManager.Instance?.ShowGameplay();
        }
        
        // Hide main menu
        gameObject.SetActive(false);
    }
    
    void QuitGame()
    {
        if (AudioManager.Instance != null && buttonClickSound != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound);
        }
        
        // Add confirmation dialog here if needed
        StartCoroutine(QuitGameCoroutine());
    }
    
    IEnumerator QuitGameCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f); // Wait for sound to play
        
        Debug.Log("Quitting game...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // Public methods for external access
    public void ShowMainMenu()
    {
        gameObject.SetActive(true);
        ShowPanel(mainMenuPanel, false);
        isTransitioning = false;
    }
    
    public void HideMainMenu()
    {
        gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        // Reset transition state when menu is enabled
        isTransitioning = false;
    }
}