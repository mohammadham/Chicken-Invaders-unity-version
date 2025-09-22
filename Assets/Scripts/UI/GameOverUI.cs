using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Over UI with score comparison and options
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Game Over Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI newHighScoreText;
    
    [Header("Player Statistics")]
    public TextMeshProUGUI enemiesKilledText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI survivalTimeText;
    public TextMeshProUGUI bonusPointsText;
    
    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;
    public Button shareScoreButton;
    
    [Header("Animation")]
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float animationDuration = 1f;
    public float statisticsDelay = 0.5f;
    
    [Header("Audio")]
    public AudioClip gameOverSound;
    public AudioClip newHighScoreSound;
    public AudioClip buttonClickSound;
    
    [Header("Effects")]
    public ParticleSystem confettiEffect;
    public GameObject newRecordBanner;
    
    private long finalScore = 0;
    private long previousHighScore = 0;
    private bool isNewHighScore = false;
    private GameStatistics gameStats;
    
    public static GameOverUI Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
    
    private void Start()
    {
        SetupEventListeners();
    }
    
    void SetupEventListeners()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        if (shareScoreButton != null)
            shareScoreButton.onClick.AddListener(ShareScore);
    }
    
    public void ShowGameOver(long score, GameStatistics statistics = null)
    {
        gameObject.SetActive(true);
        
        finalScore = score;
        gameStats = statistics;
        
        // Load high score from PlayerPrefs
        previousHighScore = System.Convert.ToInt64(PlayerPrefs.GetString("HighScore", "0"));
        
        // Check if new high score
        isNewHighScore = finalScore > previousHighScore;
        
        if (isNewHighScore)
        {
            PlayerPrefs.SetString("HighScore", finalScore.ToString());
            PlayerPrefs.Save();
        }
        
        StartCoroutine(AnimateGameOverScreen());
    }
    
    IEnumerator AnimateGameOverScreen()
    {
        // Play game over sound
        if (AudioManager.Instance != null && gameOverSound != null)
        {
            AudioManager.Instance.PlaySFX(gameOverSound);
        }
        
        // Initially hide all elements
        SetElementsAlpha(0f);
        
        // Animate title
        yield return StartCoroutine(AnimateElement(gameOverTitleText, 0.5f));
        
        // Wait a bit
        yield return new WaitForSecondsRealtime(0.3f);
        
        // Animate final score
        yield return StartCoroutine(AnimateScoreCount(finalScoreText, 0, finalScore, 1f));
        
        // Show high score comparison
        if (highScoreText != null)
        {
            highScoreText.text = $"Best: {previousHighScore:N0}";
            yield return StartCoroutine(AnimateElement(highScoreText, 0.3f));
        }
        
        // Show new high score banner if applicable
        if (isNewHighScore)
        {
            yield return StartCoroutine(ShowNewHighScore());
        }
        
        // Wait before showing statistics
        yield return new WaitForSecondsRealtime(statisticsDelay);
        
        // Show statistics
        yield return StartCoroutine(ShowStatistics());
        
        // Finally show buttons
        yield return StartCoroutine(ShowButtons());
    }
    
    IEnumerator ShowNewHighScore()
    {
        if (newHighScoreText != null)
        {
            newHighScoreText.gameObject.SetActive(true);
            yield return StartCoroutine(AnimateElement(newHighScoreText, 0.5f));
        }
        
        if (newRecordBanner != null)
        {
            newRecordBanner.SetActive(true);
            yield return StartCoroutine(AnimateScale(newRecordBanner.transform, Vector3.zero, Vector3.one, 0.5f));
        }
        
        // Play new high score sound
        if (AudioManager.Instance != null && newHighScoreSound != null)
        {
            AudioManager.Instance.PlaySFX(newHighScoreSound);
        }
        
        // Show confetti effect
        if (confettiEffect != null)
        {
            confettiEffect.Play();
        }
    }
    
    IEnumerator ShowStatistics()
    {
        if (gameStats == null) yield break;
        
        // Enemies killed
        if (enemiesKilledText != null)
        {
            yield return StartCoroutine(AnimateStatistic(enemiesKilledText, "Enemies Defeated", gameStats.enemiesKilled, 0.3f));
        }
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Accuracy
        if (accuracyText != null)
        {
            float accuracy = gameStats.shotsFired > 0 ? (float)gameStats.shotsHit / gameStats.shotsFired * 100f : 0f;
            accuracyText.text = $"Accuracy: {accuracy:F1}%";
            yield return StartCoroutine(AnimateElement(accuracyText, 0.3f));
        }
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Survival time
        if (survivalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameStats.survivalTime / 60f);
            int seconds = Mathf.FloorToInt(gameStats.survivalTime % 60f);
            survivalTimeText.text = $"Survival Time: {minutes:00}:{seconds:00}";
            yield return StartCoroutine(AnimateElement(survivalTimeText, 0.3f));
        }
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Bonus points
        if (bonusPointsText != null)
        {
            yield return StartCoroutine(AnimateStatistic(bonusPointsText, "Bonus Points", gameStats.bonusPoints, 0.3f));
        }
    }
    
    IEnumerator AnimateStatistic(TextMeshProUGUI textElement, string label, int value, float duration)
    {
        if (textElement == null) yield break;
        
        textElement.text = $"{label}: 0";
        yield return StartCoroutine(AnimateElement(textElement, 0.3f));
        
        // Count up animation
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(0, value, progress));
            textElement.text = $"{label}: {currentValue:N0}";
            yield return null;
        }
        
        textElement.text = $"{label}: {value:N0}";
    }
    
    IEnumerator ShowButtons()
    {
        Button[] buttons = { restartButton, mainMenuButton, shareScoreButton, quitButton };
        
        foreach (var button in buttons)
        {
            if (button != null)
            {
                yield return StartCoroutine(AnimateElement(button.GetComponent<TextMeshProUGUI>(), 0.2f));
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }
    
    IEnumerator AnimateElement(TextMeshProUGUI textElement, float duration)
    {
        if (textElement == null) yield break;
        
        Color startColor = textElement.color;
        Color endColor = startColor;
        startColor.a = 0f;
        endColor.a = 1f;
        
        textElement.color = startColor;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = fadeInCurve.Evaluate(elapsed / duration);
            textElement.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }
        
        textElement.color = endColor;
    }
    
    IEnumerator AnimateScoreCount(TextMeshProUGUI scoreText, long startValue, long endValue, float duration)
    {
        if (scoreText == null) yield break;
        
        // First make it visible
        scoreText.text = startValue.ToString("N0");
        yield return StartCoroutine(AnimateElement(scoreText, 0.3f));
        
        // Then count up
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = fadeInCurve.Evaluate(elapsed / duration);
            long currentScore = (long)Mathf.Lerp(startValue, endValue, progress);
            scoreText.text = currentScore.ToString("N0");
            yield return null;
        }
        
        scoreText.text = endValue.ToString("N0");
    }
    
    IEnumerator AnimateScale(Transform target, Vector3 startScale, Vector3 endScale, float duration)
    {
        if (target == null) yield break;
        
        target.localScale = startScale;
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = fadeInCurve.Evaluate(elapsed / duration);
            target.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }
        
        target.localScale = endScale;
    }
    
    void SetElementsAlpha(float alpha)
    {
        TextMeshProUGUI[] allTexts = {
            gameOverTitleText, finalScoreText, highScoreText, newHighScoreText,
            enemiesKilledText, accuracyText, survivalTimeText, bonusPointsText
        };
        
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }
        }
        
        // Hide special elements
        if (newRecordBanner != null)
            newRecordBanner.SetActive(false);
    }
    
    // Button event handlers
    void RestartGame()
    {
        PlayButtonSound();
        
        StartCoroutine(RestartGameCoroutine());
    }
    
    IEnumerator RestartGameCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    void ReturnToMainMenu()
    {
        PlayButtonSound();
        
        StartCoroutine(ReturnToMainMenuCoroutine());
    }
    
    IEnumerator ReturnToMainMenuCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        
        if (MainMenuUI.Instance != null)
            MainMenuUI.Instance.ShowMainMenu();
            
        if (UIManager.Instance != null)
            UIManager.Instance.ShowMainMenu();
            
        gameObject.SetActive(false);
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
    
    void ShareScore()
    {
        PlayButtonSound();
        
        // Implement social sharing here
        string shareText = $"I just scored {finalScore:N0} points in Chicken Invaders! Can you beat my score?";
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Android sharing implementation
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        intentObject.Call<AndroidJavaObject>("setType", "text/plain");
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText);
        
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        currentActivity.Call("startActivity", intentObject);
        #elif UNITY_IOS && !UNITY_EDITOR
        // iOS sharing would require native plugin
        Debug.Log("iOS sharing: " + shareText);
        #else
        // Fallback for other platforms
        GUIUtility.systemCopyBuffer = shareText;
        Debug.Log("Score copied to clipboard: " + shareText);
        #endif
    }
    
    void PlayButtonSound()
    {
        if (AudioManager.Instance != null && buttonClickSound != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
    
    public void HideGameOver()
    {
        gameObject.SetActive(false);
        
        // Stop confetti effect
        if (confettiEffect != null)
            confettiEffect.Stop();
    }
}

/// <summary>
/// Game statistics for end-game display
/// </summary>
[System.Serializable]
public class GameStatistics
{
    public int enemiesKilled = 0;
    public int shotsFired = 0;
    public int shotsHit = 0;
    public float survivalTime = 0f;
    public long bonusPoints = 0;
    public int powerUpsCollected = 0;
    public int coinsCollected = 0;
    
    public float GetAccuracy()
    {
        return shotsFired > 0 ? (float)shotsHit / shotsFired * 100f : 0f;
    }
}