using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Victory UI with score bonus and level progression
/// </summary>
public class VictoryUI : MonoBehaviour
{
    [Header("Victory Elements")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryTitleText;
    public TextMeshProUGUI congratulationsText;
    
    [Header("Score Display")]
    public TextMeshProUGUI baseScoreText;
    public TextMeshProUGUI timeBonusText;
    public TextMeshProUGUI accuracyBonusText;
    public TextMeshProUGUI survivalBonusText;
    public TextMeshProUGUI totalScoreText;
    
    [Header("Performance Stats")]
    public TextMeshProUGUI enemiesDefeatedText;
    public TextMeshProUGUI accuracyPercentText;
    public TextMeshProUGUI completionTimeText;
    public Image[] performanceStars;
    
    [Header("Buttons")]
    public Button nextLevelButton;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Animation")]
    public AnimationCurve bounceAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1.2f);
    public float animationDuration = 0.8f;
    public float bonusCountDuration = 1.5f;
    
    [Header("Audio")]
    public AudioClip victorySound;
    public AudioClip bonusCountSound;
    public AudioClip perfectScoreSound;
    
    [Header("Effects")]
    public ParticleSystem celebrationFireworks;
    public ParticleSystem starBurstEffect;
    public GameObject perfectScoreBanner;
    
    private long baseScore = 0;
    private long timeBonus = 0;
    private long accuracyBonus = 0;
    private long survivalBonus = 0;
    private long totalScore = 0;
    private GameStatistics gameStats;
    private int starsEarned = 0;
    
    public static VictoryUI Instance { get; private set; }
    
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
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }
    
    public void ShowVictory(long playerScore, GameStatistics statistics = null)
    {
        gameObject.SetActive(true);
        
        baseScore = playerScore;
        gameStats = statistics ?? new GameStatistics();
        
        CalculateBonuses();
        CalculateStars();
        
        StartCoroutine(AnimateVictoryScreen());
    }
    
    void CalculateBonuses()
    {
        // Time bonus: faster completion = higher bonus
        if (gameStats.survivalTime > 0)
        {
            float timeThreshold = 300f; // 5 minutes target time
            float timeRatio = Mathf.Clamp01(timeThreshold / gameStats.survivalTime);
            timeBonus = (long)(baseScore * 0.5f * timeRatio);
        }
        
        // Accuracy bonus: higher accuracy = higher bonus
        float accuracy = gameStats.GetAccuracy();
        accuracyBonus = (long)(baseScore * 0.3f * (accuracy / 100f));
        
        // Survival bonus: base bonus for completing level
        survivalBonus = (long)(baseScore * 0.2f);
        
        totalScore = baseScore + timeBonus + accuracyBonus + survivalBonus;
    }
    
    void CalculateStars()
    {
        starsEarned = 1; // Base star for completion
        
        // Second star for good accuracy (>75%)
        if (gameStats.GetAccuracy() >= 75f)
            starsEarned = 2;
            
        // Third star for excellent performance (>90% accuracy + fast completion)
        if (gameStats.GetAccuracy() >= 90f && gameStats.survivalTime <= 180f) // Under 3 minutes
            starsEarned = 3;
    }
    
    IEnumerator AnimateVictoryScreen()
    {
        // Play victory sound
        if (AudioManager.Instance != null && victorySound != null)
        {
            AudioManager.Instance.PlaySFX(victorySound);
        }
        
        // Start celebration effects
        if (celebrationFireworks != null)
        {
            celebrationFireworks.Play();
        }
        
        // Initially hide all elements
        SetElementsAlpha(0f);
        
        // Animate victory title with bounce
        yield return StartCoroutine(AnimateElementBounce(victoryTitleText, 0.8f));
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Show congratulations
        if (congratulationsText != null)
        {
            yield return StartCoroutine(AnimateElement(congratulationsText, 0.5f));
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Show base score
        if (baseScoreText != null)
        {
            baseScoreText.text = $"Base Score: {baseScore:N0}";
            yield return StartCoroutine(AnimateElement(baseScoreText, 0.3f));
        }
        
        yield return new WaitForSecondsRealtime(0.3f);
        
        // Animate bonus calculations
        yield return StartCoroutine(AnimateBonusCalculation());
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Show performance stats
        yield return StartCoroutine(ShowPerformanceStats());
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Show stars
        yield return StartCoroutine(ShowStars());
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Show buttons
        yield return StartCoroutine(ShowButtons());
        
        // Check for perfect score
        if (starsEarned >= 3)
        {
            yield return StartCoroutine(ShowPerfectScore());
        }
    }
    
    IEnumerator AnimateBonusCalculation()
    {
        // Time bonus
        if (timeBonusText != null)
        {
            yield return StartCoroutine(AnimateBonusCount(timeBonusText, "Time Bonus", timeBonus));
        }
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Accuracy bonus
        if (accuracyBonusText != null)
        {
            yield return StartCoroutine(AnimateBonusCount(accuracyBonusText, "Accuracy Bonus", accuracyBonus));
        }
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Survival bonus
        if (survivalBonusText != null)
        {
            yield return StartCoroutine(AnimateBonusCount(survivalBonusText, "Completion Bonus", survivalBonus));
        }
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Total score with emphasis
        if (totalScoreText != null)
        {
            totalScoreText.text = $"Total Score: {totalScore:N0}";
            yield return StartCoroutine(AnimateElementBounce(totalScoreText, 0.8f));
        }
    }
    
    IEnumerator AnimateBonusCount(TextMeshProUGUI textElement, string label, long bonusValue)
    {
        if (textElement == null) yield break;
        
        // Show label first
        textElement.text = $"{label}: +0";
        yield return StartCoroutine(AnimateElement(textElement, 0.3f));
        
        // Play count sound
        if (AudioManager.Instance != null && bonusCountSound != null)
        {
            AudioManager.Instance.PlaySFX(bonusCountSound);
        }
        
        // Count up animation
        float elapsed = 0f;
        while (elapsed < bonusCountDuration * 0.6f)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / (bonusCountDuration * 0.6f);
            long currentValue = (long)Mathf.Lerp(0, bonusValue, progress);
            textElement.text = $"{label}: +{currentValue:N0}";
            yield return null;
        }
        
        textElement.text = $"{label}: +{bonusValue:N0}";
    }
    
    IEnumerator ShowPerformanceStats()
    {
        // Enemies defeated
        if (enemiesDefeatedText != null)
        {
            enemiesDefeatedText.text = $"Enemies Defeated: {gameStats.enemiesKilled}";
            yield return StartCoroutine(AnimateElement(enemiesDefeatedText, 0.3f));
        }
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Accuracy
        if (accuracyPercentText != null)
        {
            accuracyPercentText.text = $"Accuracy: {gameStats.GetAccuracy():F1}%";
            yield return StartCoroutine(AnimateElement(accuracyPercentText, 0.3f));
        }
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Completion time
        if (completionTimeText != null)
        {
            int minutes = Mathf.FloorToInt(gameStats.survivalTime / 60f);
            int seconds = Mathf.FloorToInt(gameStats.survivalTime % 60f);
            completionTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            yield return StartCoroutine(AnimateElement(completionTimeText, 0.3f));
        }
    }
    
    IEnumerator ShowStars()
    {
        if (performanceStars == null || performanceStars.Length == 0) yield break;
        
        for (int i = 0; i < performanceStars.Length; i++)
        {
            if (performanceStars[i] != null)
            {
                if (i < starsEarned)
                {
                    // Show earned star with effect
                    performanceStars[i].gameObject.SetActive(true);
                    performanceStars[i].color = Color.white;
                    
                    if (starBurstEffect != null)
                    {
                        starBurstEffect.transform.position = performanceStars[i].transform.position;
                        starBurstEffect.Play();
                    }
                    
                    yield return StartCoroutine(AnimateStarAppearance(performanceStars[i].transform));
                }
                else
                {
                    // Show unearned star (grayed out)
                    performanceStars[i].gameObject.SetActive(true);
                    performanceStars[i].color = Color.gray;
                }
                
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }
    }
    
    IEnumerator AnimateStarAppearance(Transform starTransform)
    {
        Vector3 originalScale = starTransform.localScale;
        starTransform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = bounceAnimationCurve.Evaluate(elapsed / 0.5f);
            starTransform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }
        
        starTransform.localScale = originalScale;
    }
    
    IEnumerator ShowButtons()
    {
        Button[] buttons = { nextLevelButton, restartButton, mainMenuButton };
        
        foreach (var button in buttons)
        {
            if (button != null)
            {
                yield return StartCoroutine(AnimateElement(button.GetComponentInChildren<TextMeshProUGUI>(), 0.2f));
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }
    
    IEnumerator ShowPerfectScore()
    {
        // Show perfect score banner
        if (perfectScoreBanner != null)
        {
            perfectScoreBanner.SetActive(true);
            yield return StartCoroutine(AnimateElementBounce(perfectScoreBanner.GetComponent<TextMeshProUGUI>(), 1f));
        }
        
        // Play perfect score sound
        if (AudioManager.Instance != null && perfectScoreSound != null)
        {
            AudioManager.Instance.PlaySFX(perfectScoreSound);
        }
        
        // Extra fireworks
        if (celebrationFireworks != null)
        {
            celebrationFireworks.Play();
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
            float progress = elapsed / duration;
            textElement.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }
        
        textElement.color = endColor;
    }
    
    IEnumerator AnimateElementBounce(TextMeshProUGUI textElement, float duration)
    {
        if (textElement == null) yield break;
        
        // First make it visible
        yield return StartCoroutine(AnimateElement(textElement, duration * 0.3f));
        
        // Then animate scale with bounce
        Transform elementTransform = textElement.transform;
        Vector3 originalScale = elementTransform.localScale;
        
        float elapsed = 0f;
        while (elapsed < duration * 0.7f)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = bounceAnimationCurve.Evaluate(elapsed / (duration * 0.7f));
            elementTransform.localScale = originalScale * progress;
            yield return null;
        }
        
        elementTransform.localScale = originalScale;
    }
    
    void SetElementsAlpha(float alpha)
    {
        TextMeshProUGUI[] allTexts = {
            victoryTitleText, congratulationsText, baseScoreText, timeBonusText,
            accuracyBonusText, survivalBonusText, totalScoreText, enemiesDefeatedText,
            accuracyPercentText, completionTimeText
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
        
        // Hide stars
        if (performanceStars != null)
        {
            foreach (var star in performanceStars)
            {
                if (star != null)
                    star.gameObject.SetActive(false);
            }
        }
        
        if (perfectScoreBanner != null)
            perfectScoreBanner.SetActive(false);
    }
    
    // Button event handlers
    void LoadNextLevel()
    {
        PlayButtonSound();
        
        // Save progress
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.Save();
        
        StartCoroutine(LoadNextLevelCoroutine());
    }
    
    IEnumerator LoadNextLevelCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        
        // Load next level (for now, restart same level)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    void RestartLevel()
    {
        PlayButtonSound();
        
        StartCoroutine(RestartLevelCoroutine());
    }
    
    IEnumerator RestartLevelCoroutine()
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
    
    void PlayButtonSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    public void HideVictory()
    {
        gameObject.SetActive(false);
        
        // Stop effects
        if (celebrationFireworks != null)
            celebrationFireworks.Stop();
            
        if (starBurstEffect != null)
            starBurstEffect.Stop();
    }
}