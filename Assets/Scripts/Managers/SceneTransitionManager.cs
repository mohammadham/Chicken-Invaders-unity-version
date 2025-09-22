using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages scene transitions with loading screens and smooth effects
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition UI")]
    public GameObject transitionCanvas;
    public Image fadeImage;
    public Slider loadingProgressBar;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI loadingTipText;
    public GameObject loadingIcon;
    
    [Header("Transition Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float minimumLoadingTime = 2f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Loading Tips")]
    public string[] loadingTips = {
        "Tip: Use different weapons for different situations!",
        "Tip: Collect power-ups to upgrade your weapons!",
        "Tip: Keep moving to avoid enemy fire!",
        "Tip: Watch your weapon overheat meter!",
        "Tip: Coins give you bonus points!",
        "Tip: Some enemies require multiple hits to destroy!",
        "Tip: Shield protects you from one hit!",
        "Tip: Different enemy types have different movement patterns!"
    };
    
    [Header("Audio")]
    public AudioClip transitionSound;
    public AudioClip loadingCompleteSound;
    
    // Private variables
    private bool isTransitioning = false;
    private Coroutine currentTransition;
    private string targetSceneName;
    private System.Action onTransitionComplete;
    
    public static SceneTransitionManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTransitionManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeTransitionManager()
    {
        // Ensure transition canvas exists
        if (transitionCanvas == null)
        {
            CreateTransitionCanvas();
        }
        
        // Initially hide transition UI
        if (transitionCanvas != null)
            transitionCanvas.SetActive(false);
    }
    
    void CreateTransitionCanvas()
    {
        // Create transition canvas programmatically if not assigned
        GameObject canvasGO = new GameObject("TransitionCanvas");
        DontDestroyOnLoad(canvasGO);
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it's on top
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create fade image
        GameObject fadeGO = new GameObject("FadeImage");
        fadeGO.transform.SetParent(canvasGO.transform);
        
        fadeImage = fadeGO.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        RectTransform fadeRect = fadeImage.rectTransform;
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        
        transitionCanvas = canvasGO;
        
        CreateLoadingUI();
    }
    
    void CreateLoadingUI()
    {
        if (transitionCanvas == null) return;
        
        // Create loading UI container
        GameObject loadingContainer = new GameObject("LoadingContainer");
        loadingContainer.transform.SetParent(transitionCanvas.transform);
        
        RectTransform containerRect = loadingContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        // Loading text
        GameObject loadingTextGO = new GameObject("LoadingText");
        loadingTextGO.transform.SetParent(loadingContainer.transform);
        
        loadingText = loadingTextGO.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Loading...";
        loadingText.fontSize = 48;
        loadingText.color = Color.white;
        loadingText.alignment = TextAlignmentOptions.Center;
        
        RectTransform loadingTextRect = loadingText.rectTransform;
        loadingTextRect.anchorMin = new Vector2(0.5f, 0.6f);
        loadingTextRect.anchorMax = new Vector2(0.5f, 0.6f);
        loadingTextRect.sizeDelta = new Vector2(400, 100);
        loadingTextRect.anchoredPosition = Vector2.zero;
        
        // Progress bar
        CreateProgressBar(loadingContainer);
        
        // Loading tip text
        GameObject tipTextGO = new GameObject("TipText");
        tipTextGO.transform.SetParent(loadingContainer.transform);
        
        loadingTipText = tipTextGO.AddComponent<TextMeshProUGUI>();
        loadingTipText.fontSize = 24;
        loadingTipText.color = new Color(0.8f, 0.8f, 0.8f);
        loadingTipText.alignment = TextAlignmentOptions.Center;
        
        RectTransform tipTextRect = loadingTipText.rectTransform;
        tipTextRect.anchorMin = new Vector2(0.5f, 0.2f);
        tipTextRect.anchorMax = new Vector2(0.5f, 0.2f);
        tipTextRect.sizeDelta = new Vector2(800, 100);
        tipTextRect.anchoredPosition = Vector2.zero;
    }
    
    void CreateProgressBar(GameObject parent)
    {
        // Progress bar background
        GameObject progressBG = new GameObject("ProgressBarBG");
        progressBG.transform.SetParent(parent.transform);
        
        Image bgImage = progressBG.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgImage.rectTransform;
        bgRect.anchorMin = new Vector2(0.5f, 0.4f);
        bgRect.anchorMax = new Vector2(0.5f, 0.4f);
        bgRect.sizeDelta = new Vector2(600, 20);
        bgRect.anchoredPosition = Vector2.zero;
        
        // Progress bar slider
        GameObject sliderGO = new GameObject("ProgressSlider");
        sliderGO.transform.SetParent(parent.transform);
        
        loadingProgressBar = sliderGO.AddComponent<Slider>();
        loadingProgressBar.minValue = 0f;
        loadingProgressBar.maxValue = 1f;
        loadingProgressBar.value = 0f;
        
        RectTransform sliderRect = loadingProgressBar.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.4f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.4f);
        sliderRect.sizeDelta = new Vector2(600, 20);
        sliderRect.anchoredPosition = Vector2.zero;
        
        // Slider fill
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform);
        
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform);
        
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.white;
        
        RectTransform fillRect = fillImage.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        loadingProgressBar.fillRect = fillRect;
    }
    
    public void TransitionToScene(string sceneName, System.Action onComplete = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress!");
            return;
        }
        
        targetSceneName = sceneName;
        onTransitionComplete = onComplete;
        
        if (currentTransition != null)
            StopCoroutine(currentTransition);
            
        currentTransition = StartCoroutine(TransitionCoroutine());
    }
    
    public void RestartCurrentScene(System.Action onComplete = null)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        TransitionToScene(currentSceneName, onComplete);
    }
    
    public void LoadGameScene(System.Action onComplete = null)
    {
        TransitionToScene("GameplayScene", onComplete);
    }
    
    public void LoadMainMenu(System.Action onComplete = null)
    {
        TransitionToScene("MainMenuScene", onComplete);
    }
    
    IEnumerator TransitionCoroutine()
    {
        isTransitioning = true;
        
        // Show transition canvas
        if (transitionCanvas != null)
            transitionCanvas.SetActive(true);
        
        // Play transition sound
        if (AdvancedAudioManager.Instance != null && transitionSound != null)
        {
            AdvancedAudioManager.Instance.PlaySFX(transitionSound);
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Start loading
        yield return StartCoroutine(LoadSceneAsync());
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Hide transition canvas
        if (transitionCanvas != null)
            transitionCanvas.SetActive(false);
        
        // Call completion callback
        onTransitionComplete?.Invoke();
        
        // Play completion sound
        if (AdvancedAudioManager.Instance != null && loadingCompleteSound != null)
        {
            AdvancedAudioManager.Instance.PlaySFX(loadingCompleteSound);
        }
        
        isTransitioning = false;
    }
    
    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;
        
        Color startColor = fadeImage.color;
        startColor.a = 0f;
        Color endColor = startColor;
        endColor.a = 1f;
        
        fadeImage.color = startColor;
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = fadeCurve.Evaluate(elapsed / fadeInDuration);
            
            Color currentColor = Color.Lerp(startColor, endColor, progress);
            fadeImage.color = currentColor;
            
            yield return null;
        }
        
        fadeImage.color = endColor;
    }
    
    IEnumerator LoadSceneAsync()
    {
        // Show loading UI
        ShowLoadingUI();
        
        // Display random loading tip
        if (loadingTipText != null && loadingTips.Length > 0)
        {
            string randomTip = loadingTips[Random.Range(0, loadingTips.Length)];
            loadingTipText.text = randomTip;
        }
        
        // Start async loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false; // Prevent auto activation
        
        float loadingStartTime = Time.unscaledTime;
        float elapsedTime = 0f;
        
        // Wait for loading to complete or minimum time
        while (!asyncLoad.isDone || elapsedTime < minimumLoadingTime)
        {
            elapsedTime = Time.unscaledTime - loadingStartTime;
            
            // Update progress bar
            float loadProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // Unity loads to 0.9f then waits
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadingTime);
            float displayProgress = Mathf.Min(loadProgress, timeProgress);
            
            if (loadingProgressBar != null)
                loadingProgressBar.value = displayProgress;
            
            // Update loading text with dots animation
            if (loadingText != null)
            {
                int dotCount = Mathf.FloorToInt((elapsedTime * 2f) % 4f);
                string dots = new string('.', dotCount);
                loadingText.text = "Loading" + dots;
            }
            
            // Rotate loading icon if available
            if (loadingIcon != null)
            {
                loadingIcon.transform.Rotate(0, 0, -90 * Time.unscaledDeltaTime);
            }
            
            // Allow activation when both loading and minimum time are complete
            if (asyncLoad.progress >= 0.9f && elapsedTime >= minimumLoadingTime)
            {
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // Ensure progress bar shows 100%
        if (loadingProgressBar != null)
            loadingProgressBar.value = 1f;
        
        if (loadingText != null)
            loadingText.text = "Complete!";
        
        // Small delay to show completion
        yield return new WaitForSecondsRealtime(0.5f);
    }
    
    void ShowLoadingUI()
    {
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = 0f;
            loadingProgressBar.gameObject.SetActive(true);
        }
        
        if (loadingText != null)
        {
            loadingText.text = "Loading...";
            loadingText.gameObject.SetActive(true);
        }
        
        if (loadingTipText != null)
            loadingTipText.gameObject.SetActive(true);
            
        if (loadingIcon != null)
            loadingIcon.SetActive(true);
    }
    
    void HideLoadingUI()
    {
        if (loadingProgressBar != null)
            loadingProgressBar.gameObject.SetActive(false);
            
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);
            
        if (loadingTipText != null)
            loadingTipText.gameObject.SetActive(false);
            
        if (loadingIcon != null)
            loadingIcon.SetActive(false);
    }
    
    IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        
        // Hide loading UI
        HideLoadingUI();
        
        Color startColor = fadeImage.color;
        Color endColor = startColor;
        endColor.a = 0f;
        
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = fadeCurve.Evaluate(elapsed / fadeOutDuration);
            
            Color currentColor = Color.Lerp(startColor, endColor, progress);
            fadeImage.color = currentColor;
            
            yield return null;
        }
        
        fadeImage.color = endColor;
    }
    
    // Public utility methods
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    public void SetLoadingTips(string[] tips)
    {
        loadingTips = tips;
    }
    
    public void ShowQuickFade(System.Action onComplete = null)
    {
        StartCoroutine(QuickFadeCoroutine(onComplete));
    }
    
    IEnumerator QuickFadeCoroutine(System.Action onComplete)
    {
        if (transitionCanvas != null)
            transitionCanvas.SetActive(true);
            
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSecondsRealtime(0.2f);
        yield return StartCoroutine(FadeOut());
        
        if (transitionCanvas != null)
            transitionCanvas.SetActive(false);
            
        onComplete?.Invoke();
    }
    
    // Scene management utilities
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    public bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
    
    private void OnDestroy()
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
    }
}