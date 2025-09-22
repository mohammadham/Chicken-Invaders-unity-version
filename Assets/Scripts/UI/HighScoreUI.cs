using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// High Score UI with local leaderboard
/// </summary>
public class HighScoreUI : MonoBehaviour
{
    [Header("High Score Elements")]
    public GameObject highScorePanel;
    public TextMeshProUGUI titleText;
    public Button backButton;
    public Button clearScoresButton;
    
    [Header("Score Display")]
    public Transform scoreListParent;
    public GameObject scoreEntryPrefab;
    public int maxScoreEntries = 10;
    
    [Header("New Score Entry")]
    public GameObject newScoreEntryPanel;
    public TMP_InputField playerNameInput;
    public Button submitScoreButton;
    public Button cancelSubmitButton;
    public TextMeshProUGUI newScoreText;
    
    [Header("Animation")]
    public AnimationCurve entryAnimationCurve = AnimationCurve.EaseOutBounce(0, 0, 1, 1);
    public float entryAnimationDuration = 0.5f;
    public float entryAnimationDelay = 0.1f;
    
    [Header("Colors")]
    public Color normalScoreColor = Color.white;
    public Color highlightScoreColor = Color.yellow;
    public Color newScoreColor = Color.green;
    
    private List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    private bool isEnteringNewScore = false;
    private long pendingScore = 0;
    private List<GameObject> scoreEntryObjects = new List<GameObject>();
    
    public static HighScoreUI Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        SetupEventListeners();
        LoadHighScores();
        
        if (newScoreEntryPanel != null)
            newScoreEntryPanel.SetActive(false);
    }
    
    void SetupEventListeners()
    {
        if (backButton != null)
            backButton.onClick.AddListener(CloseHighScores);
            
        if (clearScoresButton != null)
            clearScoresButton.onClick.AddListener(ClearAllScores);
            
        if (submitScoreButton != null)
            submitScoreButton.onClick.AddListener(SubmitNewScore);
            
        if (cancelSubmitButton != null)
            cancelSubmitButton.onClick.AddListener(CancelNewScore);
            
        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.AddListener(OnNameInputEnd);
            playerNameInput.characterLimit = 20;
        }
    }
    
    void LoadHighScores()
    {
        highScores.Clear();
        
        // Load high scores from PlayerPrefs
        for (int i = 0; i < maxScoreEntries; i++)
        {
            string scoreName = PlayerPrefs.GetString($"HighScore_{i}_Name", "");
            if (!string.IsNullOrEmpty(scoreName))
            {
                long score = System.Convert.ToInt64(PlayerPrefs.GetString($"HighScore_{i}_Score", "0"));
                string date = PlayerPrefs.GetString($"HighScore_{i}_Date", "");
                
                highScores.Add(new HighScoreEntry
                {
                    playerName = scoreName,
                    score = score,
                    date = date,
                    rank = i + 1
                });
            }
        }
        
        // Sort scores in descending order
        highScores = highScores.OrderByDescending(s => s.score).ToList();
        
        // Update ranks
        for (int i = 0; i < highScores.Count; i++)
        {
            highScores[i].rank = i + 1;
        }
    }
    
    void SaveHighScores()
    {
        // Clear existing scores
        for (int i = 0; i < maxScoreEntries; i++)
        {
            PlayerPrefs.DeleteKey($"HighScore_{i}_Name");
            PlayerPrefs.DeleteKey($"HighScore_{i}_Score");
            PlayerPrefs.DeleteKey($"HighScore_{i}_Date");
        }
        
        // Save current scores
        for (int i = 0; i < Mathf.Min(highScores.Count, maxScoreEntries); i++)
        {
            PlayerPrefs.SetString($"HighScore_{i}_Name", highScores[i].playerName);
            PlayerPrefs.SetString($"HighScore_{i}_Score", highScores[i].score.ToString());
            PlayerPrefs.SetString($"HighScore_{i}_Date", highScores[i].date);
        }
        
        PlayerPrefs.Save();
    }
    
    public void ShowHighScores()
    {
        gameObject.SetActive(true);
        
        if (!isEnteringNewScore)
        {
            RefreshScoreDisplay();
        }
    }
    
    void RefreshScoreDisplay()
    {
        // Clear existing display
        ClearScoreDisplay();
        
        // Create score entries
        StartCoroutine(AnimateScoreEntries());
    }
    
    void ClearScoreDisplay()
    {
        foreach (var obj in scoreEntryObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        scoreEntryObjects.Clear();
    }
    
    IEnumerator AnimateScoreEntries()
    {
        if (scoreEntryPrefab == null || scoreListParent == null) yield break;
        
        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject entryObject = CreateScoreEntry(highScores[i], i);
            if (entryObject != null)
            {
                yield return StartCoroutine(AnimateEntryAppearance(entryObject));
                yield return new WaitForSecondsRealtime(entryAnimationDelay);
            }
        }
        
        // If no scores, show empty message
        if (highScores.Count == 0)
        {
            CreateEmptyScoreMessage();
        }
    }
    
    GameObject CreateScoreEntry(HighScoreEntry scoreEntry, int index)
    {
        GameObject entryObject = Instantiate(scoreEntryPrefab, scoreListParent);
        scoreEntryObjects.Add(entryObject);
        
        // Setup entry components
        HighScoreEntryUI entryUI = entryObject.GetComponent<HighScoreEntryUI>();
        if (entryUI == null)
            entryUI = entryObject.AddComponent<HighScoreEntryUI>();
            
        entryUI.SetupEntry(scoreEntry, index);
        
        // Set initial scale for animation
        entryObject.transform.localScale = Vector3.zero;
        
        return entryObject;
    }
    
    void CreateEmptyScoreMessage()
    {
        GameObject emptyMessage = new GameObject("EmptyScoreMessage");
        emptyMessage.transform.SetParent(scoreListParent);
        
        TextMeshProUGUI text = emptyMessage.AddComponent<TextMeshProUGUI>();
        text.text = "No high scores yet!\nPlay the game to set your first record!";
        text.fontSize = 24;
        text.color = Color.gray;
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        scoreEntryObjects.Add(emptyMessage);
    }
    
    IEnumerator AnimateEntryAppearance(GameObject entryObject)
    {
        if (entryObject == null) yield break;
        
        Vector3 targetScale = Vector3.one;
        float elapsed = 0f;
        
        while (elapsed < entryAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = entryAnimationCurve.Evaluate(elapsed / entryAnimationDuration);
            entryObject.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, progress);
            yield return null;
        }
        
        entryObject.transform.localScale = targetScale;
    }
    
    public bool IsNewHighScore(long score)
    {
        if (highScores.Count < maxScoreEntries)
            return true;
            
        return score > highScores.LastOrDefault()?.score ?? 0;
    }
    
    public void RequestNewScoreEntry(long score)
    {
        if (!IsNewHighScore(score)) return;
        
        pendingScore = score;
        isEnteringNewScore = true;
        
        ShowNewScoreEntryPanel();
    }
    
    void ShowNewScoreEntryPanel()
    {
        if (newScoreEntryPanel != null)
        {
            newScoreEntryPanel.SetActive(true);
            
            if (newScoreText != null)
                newScoreText.text = $"New High Score!\n{pendingScore:N0}";
                
            if (playerNameInput != null)
            {
                playerNameInput.text = PlayerPrefs.GetString("LastPlayerName", "Player");
                playerNameInput.Select();
                playerNameInput.ActivateInputField();
            }
        }
    }
    
    void OnNameInputEnd(string playerName)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitNewScore();
        }
    }
    
    void SubmitNewScore()
    {
        string playerName = playerNameInput != null ? playerNameInput.text.Trim() : "Anonymous";
        
        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonymous";
            
        // Save player name for next time
        PlayerPrefs.SetString("LastPlayerName", playerName);
        
        // Add new score entry
        HighScoreEntry newEntry = new HighScoreEntry
        {
            playerName = playerName,
            score = pendingScore,
            date = System.DateTime.Now.ToString("yyyy-MM-dd"),
            rank = 0 // Will be calculated when sorted
        };
        
        highScores.Add(newEntry);
        
        // Sort and limit scores
        highScores = highScores.OrderByDescending(s => s.score).Take(maxScoreEntries).ToList();
        
        // Update ranks
        for (int i = 0; i < highScores.Count; i++)
        {
            highScores[i].rank = i + 1;
        }
        
        // Save to PlayerPrefs
        SaveHighScores();
        
        // Hide new score entry panel
        if (newScoreEntryPanel != null)
            newScoreEntryPanel.SetActive(false);
            
        isEnteringNewScore = false;
        
        // Refresh display with highlighting
        StartCoroutine(RefreshWithHighlight(newEntry));
        
        // Play success sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    IEnumerator RefreshWithHighlight(HighScoreEntry highlightEntry)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        
        RefreshScoreDisplay();
        
        // TODO: Add highlighting logic for the new entry
    }
    
    void CancelNewScore()
    {
        if (newScoreEntryPanel != null)
            newScoreEntryPanel.SetActive(false);
            
        isEnteringNewScore = false;
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    void ClearAllScores()
    {
        // Show confirmation dialog (simplified version)
        StartCoroutine(ConfirmClearScores());
    }
    
    IEnumerator ConfirmClearScores()
    {
        // In a full implementation, you'd show a proper confirmation dialog
        Debug.Log("Clear all scores? This cannot be undone!");
        
        yield return new WaitForSecondsRealtime(1f);
        
        // Clear scores
        highScores.Clear();
        
        // Clear PlayerPrefs
        for (int i = 0; i < maxScoreEntries; i++)
        {
            PlayerPrefs.DeleteKey($"HighScore_{i}_Name");
            PlayerPrefs.DeleteKey($"HighScore_{i}_Score");
            PlayerPrefs.DeleteKey($"HighScore_{i}_Date");
        }
        PlayerPrefs.Save();
        
        // Refresh display
        RefreshScoreDisplay();
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
    
    void CloseHighScores()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
            
        gameObject.SetActive(false);
    }
    
    public List<HighScoreEntry> GetHighScores()
    {
        return new List<HighScoreEntry>(highScores);
    }
    
    public int GetPlayerRank(long score)
    {
        int rank = 1;
        foreach (var entry in highScores)
        {
            if (score <= entry.score)
                rank++;
            else
                break;
        }
        return rank;
    }
}

/// <summary>
/// High score entry data structure
/// </summary>
[System.Serializable]
public class HighScoreEntry
{
    public string playerName;
    public long score;
    public string date;
    public int rank;
}

/// <summary>
/// Individual high score entry UI component
/// </summary>
public class HighScoreEntryUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI dateText;
    public Image backgroundImage;
    
    public void SetupEntry(HighScoreEntry entry, int visualIndex)
    {
        // Find UI elements if not assigned
        if (rankText == null) rankText = transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        if (nameText == null) nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (scoreText == null) scoreText = transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        if (dateText == null) dateText = transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        
        // Set text content
        if (rankText != null)
            rankText.text = $"#{entry.rank}";
            
        if (nameText != null)
            nameText.text = entry.playerName;
            
        if (scoreText != null)
            scoreText.text = entry.score.ToString("N0");
            
        if (dateText != null)
            dateText.text = entry.date;
        
        // Set colors based on rank
        Color textColor = Color.white;
        Color bgColor = Color.clear;
        
        switch (entry.rank)
        {
            case 1:
                textColor = Color.yellow; // Gold
                bgColor = new Color(1f, 1f, 0f, 0.1f);
                break;
            case 2:
                textColor = Color.gray; // Silver
                bgColor = new Color(0.7f, 0.7f, 0.7f, 0.1f);
                break;
            case 3:
                textColor = new Color(0.8f, 0.5f, 0.2f); // Bronze
                bgColor = new Color(0.8f, 0.5f, 0.2f, 0.1f);
                break;
        }
        
        // Apply colors
        SetTextColor(textColor);
        
        if (backgroundImage != null)
            backgroundImage.color = bgColor;
    }
    
    void SetTextColor(Color color)
    {
        if (rankText != null) rankText.color = color;
        if (nameText != null) nameText.color = color;
        if (scoreText != null) scoreText.color = color;
        if (dateText != null) dateText.color = color;
    }
}