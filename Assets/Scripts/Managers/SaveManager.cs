using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Comprehensive Save/Load system for game data persistence
/// Handles settings, progress, achievements, statistics, and high scores
/// </summary>
public class SaveManager : MonoBehaviour
{
    [Header("Save Settings")]
    public bool encryptSaveData = true;
    public bool autoSave = true;
    public float autoSaveInterval = 30f; // seconds
    public int maxSaveSlots = 5;
    
    [Header("Debug")]
    public bool enableDebugLogs = false;
    
    // Save data structure
    private GameSaveData currentSaveData;
    private string saveDirectory;
    private string saveFileExtension = ".gamesave";
    private string settingsFileName = "settings";
    private string progressFileName = "progress";
    private float lastAutoSaveTime;
    
    // Encryption key (in production, this should be more secure)
    private readonly string encryptionKey = "ChickenInvaders2024Key";
    
    public static SaveManager Instance { get; private set; }
    
    // Events
    public System.Action OnDataLoaded;
    public System.Action OnDataSaved;
    public System.Action<string> OnSaveError;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadAllData();
    }
    
    private void Update()
    {
        if (autoSave && Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            AutoSave();
            lastAutoSaveTime = Time.time;
        }
    }
    
    void InitializeSaveSystem()
    {
        // Set save directory
        saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
        
        // Create save directory if it doesn't exist
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        
        // Initialize save data structure
        currentSaveData = new GameSaveData();
        
        LogDebug("Save system initialized. Save directory: " + saveDirectory);
    }
    
    #region Settings Management
    
    public void SaveSettings(SettingsData settings)
    {
        try
        {
            currentSaveData.settings = settings;
            
            string json = JsonUtility.ToJson(settings, true);
            string filePath = Path.Combine(saveDirectory, settingsFileName + saveFileExtension);
            
            if (encryptSaveData)
                json = EncryptString(json);
                
            File.WriteAllText(filePath, json);
            
            LogDebug("Settings saved successfully");
            OnDataSaved?.Invoke();
        }
        catch (Exception e)
        {
            LogError("Failed to save settings: " + e.Message);
            OnSaveError?.Invoke("Failed to save settings");
        }
    }
    
    public SettingsData LoadSettings()
    {
        try
        {
            string filePath = Path.Combine(saveDirectory, settingsFileName + saveFileExtension);
            
            if (!File.Exists(filePath))
            {
                LogDebug("Settings file not found, creating default settings");
                return new SettingsData();
            }
            
            string json = File.ReadAllText(filePath);
            
            if (encryptSaveData)
                json = DecryptString(json);
                
            SettingsData settings = JsonUtility.FromJson<SettingsData>(json);
            currentSaveData.settings = settings;
            
            LogDebug("Settings loaded successfully");
            return settings;
        }
        catch (Exception e)
        {
            LogError("Failed to load settings: " + e.Message);
            return new SettingsData(); // Return default settings on error
        }
    }
    
    #endregion
    
    #region Progress Management
    
    public void SaveProgress(GameProgressData progress)
    {
        try
        {
            currentSaveData.progress = progress;
            
            string json = JsonUtility.ToJson(progress, true);
            string filePath = Path.Combine(saveDirectory, progressFileName + saveFileExtension);
            
            if (encryptSaveData)
                json = EncryptString(json);
                
            File.WriteAllText(filePath, json);
            
            LogDebug("Progress saved successfully");
            OnDataSaved?.Invoke();
        }
        catch (Exception e)
        {
            LogError("Failed to save progress: " + e.Message);
            OnSaveError?.Invoke("Failed to save progress");
        }
    }
    
    public GameProgressData LoadProgress()
    {
        try
        {
            string filePath = Path.Combine(saveDirectory, progressFileName + saveFileExtension);
            
            if (!File.Exists(filePath))
            {
                LogDebug("Progress file not found, creating default progress");
                return new GameProgressData();
            }
            
            string json = File.ReadAllText(filePath);
            
            if (encryptSaveData)
                json = DecryptString(json);
                
            GameProgressData progress = JsonUtility.FromJson<GameProgressData>(json);
            currentSaveData.progress = progress;
            
            LogDebug("Progress loaded successfully");
            return progress;
        }
        catch (Exception e)
        {
            LogError("Failed to load progress: " + e.Message);
            return new GameProgressData(); // Return default progress on error
        }
    }
    
    #endregion
    
    #region High Scores Management
    
    public void SaveHighScore(HighScoreEntry newScore)
    {
        if (currentSaveData.highScores == null)
            currentSaveData.highScores = new List<HighScoreEntry>();
            
        // Add new score
        currentSaveData.highScores.Add(newScore);
        
        // Sort and limit to top scores
        currentSaveData.highScores.Sort((a, b) => b.score.CompareTo(a.score));
        
        if (currentSaveData.highScores.Count > 10)
        {
            currentSaveData.highScores.RemoveRange(10, currentSaveData.highScores.Count - 10);
        }
        
        // Update ranks
        for (int i = 0; i < currentSaveData.highScores.Count; i++)
        {
            currentSaveData.highScores[i].rank = i + 1;
        }
        
        SaveCompleteData();
    }
    
    public List<HighScoreEntry> GetHighScores()
    {
        if (currentSaveData.highScores == null)
            return new List<HighScoreEntry>();
            
        return new List<HighScoreEntry>(currentSaveData.highScores);
    }
    
    public bool IsNewHighScore(long score)
    {
        if (currentSaveData.highScores == null || currentSaveData.highScores.Count < 10)
            return true;
            
        return score > currentSaveData.highScores[currentSaveData.highScores.Count - 1].score;
    }
    
    public void ClearHighScores()
    {
        currentSaveData.highScores?.Clear();
        SaveCompleteData();
    }
    
    #endregion
    
    #region Statistics Management
    
    public void UpdateStatistics(GameStatistics stats)
    {
        if (currentSaveData.statistics == null)
            currentSaveData.statistics = new GameStatistics();
            
        // Add to cumulative statistics
        currentSaveData.statistics.enemiesKilled += stats.enemiesKilled;
        currentSaveData.statistics.shotsFired += stats.shotsFired;
        currentSaveData.statistics.shotsHit += stats.shotsHit;
        currentSaveData.statistics.survivalTime += stats.survivalTime;
        currentSaveData.statistics.bonusPoints += stats.bonusPoints;
        currentSaveData.statistics.powerUpsCollected += stats.powerUpsCollected;
        currentSaveData.statistics.coinsCollected += stats.coinsCollected;
        
        // Update session statistics
        currentSaveData.statistics.gamesPlayed++;
        currentSaveData.statistics.totalPlayTime += stats.survivalTime;
        
        if (autoSave)
            SaveCompleteData();
    }
    
    public GameStatistics GetStatistics()
    {
        return currentSaveData.statistics ?? new GameStatistics();
    }
    
    public void ResetStatistics()
    {
        currentSaveData.statistics = new GameStatistics();
        SaveCompleteData();
    }
    
    #endregion
    
    #region Achievements Management
    
    public void UnlockAchievement(string achievementId)
    {
        if (currentSaveData.achievements == null)
            currentSaveData.achievements = new List<string>();
            
        if (!currentSaveData.achievements.Contains(achievementId))
        {
            currentSaveData.achievements.Add(achievementId);
            currentSaveData.achievementUnlockDates[achievementId] = DateTime.Now.ToBinary();
            
            LogDebug("Achievement unlocked: " + achievementId);
            
            if (autoSave)
                SaveCompleteData();
        }
    }
    
    public bool IsAchievementUnlocked(string achievementId)
    {
        return currentSaveData.achievements?.Contains(achievementId) ?? false;
    }
    
    public List<string> GetUnlockedAchievements()
    {
        return currentSaveData.achievements ?? new List<string>();
    }
    
    public DateTime GetAchievementUnlockDate(string achievementId)
    {
        if (currentSaveData.achievementUnlockDates?.ContainsKey(achievementId) == true)
        {
            return DateTime.FromBinary(currentSaveData.achievementUnlockDates[achievementId]);
        }
        return DateTime.MinValue;
    }
    
    #endregion
    
    #region Complete Data Management
    
    void LoadAllData()
    {
        currentSaveData.settings = LoadSettings();
        currentSaveData.progress = LoadProgress();
        LoadCompleteData(); // Load other data from complete save file
        
        OnDataLoaded?.Invoke();
    }
    
    void AutoSave()
    {
        if (currentSaveData != null)
        {
            SaveCompleteData();
            LogDebug("Auto-save completed");
        }
    }
    
    public void SaveCompleteData()
    {
        try
        {
            currentSaveData.lastSaveTime = DateTime.Now.ToBinary();
            
            string json = JsonUtility.ToJson(currentSaveData, true);
            string filePath = Path.Combine(saveDirectory, "complete_save" + saveFileExtension);
            
            if (encryptSaveData)
                json = EncryptString(json);
                
            File.WriteAllText(filePath, json);
            
            LogDebug("Complete save data saved successfully");
            OnDataSaved?.Invoke();
        }
        catch (Exception e)
        {
            LogError("Failed to save complete data: " + e.Message);
            OnSaveError?.Invoke("Failed to save game data");
        }
    }
    
    void LoadCompleteData()
    {
        try
        {
            string filePath = Path.Combine(saveDirectory, "complete_save" + saveFileExtension);
            
            if (!File.Exists(filePath))
            {
                LogDebug("Complete save file not found, using default data");
                return;
            }
            
            string json = File.ReadAllText(filePath);
            
            if (encryptSaveData)
                json = DecryptString(json);
                
            GameSaveData loadedData = JsonUtility.FromJson<GameSaveData>(json);
            
            // Merge loaded data with current data (keep settings and progress from individual files)
            if (loadedData.highScores != null)
                currentSaveData.highScores = loadedData.highScores;
                
            if (loadedData.statistics != null)
                currentSaveData.statistics = loadedData.statistics;
                
            if (loadedData.achievements != null)
                currentSaveData.achievements = loadedData.achievements;
                
            if (loadedData.achievementUnlockDates != null)
                currentSaveData.achievementUnlockDates = loadedData.achievementUnlockDates;
            
            LogDebug("Complete save data loaded successfully");
        }
        catch (Exception e)
        {
            LogError("Failed to load complete data: " + e.Message);
        }
    }
    
    #endregion
    
    #region Save Slot Management
    
    public void SaveToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            LogError("Invalid save slot index: " + slotIndex);
            return;
        }
        
        try
        {
            string json = JsonUtility.ToJson(currentSaveData, true);
            string filePath = Path.Combine(saveDirectory, $"save_slot_{slotIndex}" + saveFileExtension);
            
            if (encryptSaveData)
                json = EncryptString(json);
                
            File.WriteAllText(filePath, json);
            
            LogDebug($"Game saved to slot {slotIndex}");
            OnDataSaved?.Invoke();
        }
        catch (Exception e)
        {
            LogError($"Failed to save to slot {slotIndex}: " + e.Message);
            OnSaveError?.Invoke($"Failed to save to slot {slotIndex}");
        }
    }
    
    public bool LoadFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
        {
            LogError("Invalid save slot index: " + slotIndex);
            return false;
        }
        
        try
        {
            string filePath = Path.Combine(saveDirectory, $"save_slot_{slotIndex}" + saveFileExtension);
            
            if (!File.Exists(filePath))
            {
                LogDebug($"Save slot {slotIndex} not found");
                return false;
            }
            
            string json = File.ReadAllText(filePath);
            
            if (encryptSaveData)
                json = DecryptString(json);
                
            currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
            
            LogDebug($"Game loaded from slot {slotIndex}");
            OnDataLoaded?.Invoke();
            return true;
        }
        catch (Exception e)
        {
            LogError($"Failed to load from slot {slotIndex}: " + e.Message);
            return false;
        }
    }
    
    public bool IsSlotUsed(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSaveSlots)
            return false;
            
        string filePath = Path.Combine(saveDirectory, $"save_slot_{slotIndex}" + saveFileExtension);
        return File.Exists(filePath);
    }
    
    public DateTime GetSlotSaveTime(int slotIndex)
    {
        if (!IsSlotUsed(slotIndex))
            return DateTime.MinValue;
            
        try
        {
            string filePath = Path.Combine(saveDirectory, $"save_slot_{slotIndex}" + saveFileExtension);
            return File.GetLastWriteTime(filePath);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    string EncryptString(string text)
    {
        // Simple XOR encryption (in production, use more secure encryption)
        char[] chars = text.ToCharArray();
        char[] keyChars = encryptionKey.ToCharArray();
        
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)(chars[i] ^ keyChars[i % keyChars.Length]);
        }
        
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(chars));
    }
    
    string DecryptString(string encryptedText)
    {
        try
        {
            char[] chars = System.Text.Encoding.UTF8.GetChars(Convert.FromBase64String(encryptedText));
            char[] keyChars = encryptionKey.ToCharArray();
            
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] ^ keyChars[i % keyChars.Length]);
            }
            
            return new string(chars);
        }
        catch
        {
            throw new Exception("Failed to decrypt save data");
        }
    }
    
    void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log("[SaveManager] " + message);
    }
    
    void LogError(string message)
    {
        Debug.LogError("[SaveManager] " + message);
    }
    
    #endregion
    
    #region Public Interface
    
    public GameSaveData GetCurrentSaveData()
    {
        return currentSaveData;
    }
    
    public void ClearAllData()
    {
        try
        {
            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
                Directory.CreateDirectory(saveDirectory);
            }
            
            currentSaveData = new GameSaveData();
            LogDebug("All save data cleared");
        }
        catch (Exception e)
        {
            LogError("Failed to clear save data: " + e.Message);
        }
    }
    
    public long GetTotalPlayTime()
    {
        return currentSaveData.statistics?.totalPlayTime ?? 0;
    }
    
    public int GetGamesPlayed()
    {
        return currentSaveData.statistics?.gamesPlayed ?? 0;
    }
    
    #endregion
}

/// <summary>
/// Complete game save data structure
/// </summary>
[System.Serializable]
public class GameSaveData
{
    public SettingsData settings = new SettingsData();
    public GameProgressData progress = new GameProgressData();
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
    public GameStatistics statistics = new GameStatistics();
    public List<string> achievements = new List<string>();
    public SerializableDictionary<string, long> achievementUnlockDates = new SerializableDictionary<string, long>();
    public long lastSaveTime;
    public string gameVersion = "1.0.0";
}

/// <summary>
/// Game progress data
/// </summary>
[System.Serializable]
public class GameProgressData
{
    public int currentLevel = 1;
    public int maxLevelReached = 1;
    public bool[] levelsCompleted = new bool[50]; // Support for 50 levels
    public long totalScore = 0;
    public int totalEnemiesKilled = 0;
    public bool tutorialCompleted = false;
    public string lastPlayedDate = "";
}

/// <summary>
/// Enhanced settings data
/// </summary>
[System.Serializable]
public class SettingsData
{
    // Audio settings
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    public float voiceVolume = 1f;
    public float ambientVolume = 0.5f;
    public bool musicEnabled = true;
    public bool sfxEnabled = true;
    
    // Graphics settings
    public int qualityLevel = 2;
    public bool fullscreen = true;
    public bool vsync = true;
    public int targetFrameRate = 60;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    
    // Controls settings
    public float touchSensitivity = 2f;
    public bool invertYAxis = false;
    public bool hapticFeedback = true;
    public bool autoFire = false;
    public bool showFPSCounter = false;
    
    // Gameplay settings
    public int difficulty = 1; // 0=Easy, 1=Normal, 2=Hard
    public bool showDamageNumbers = true;
    public bool enableScreenShake = true;
    public string playerName = "Player";
}

/// <summary>
/// Serializable dictionary for achievements
/// </summary>
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();
    
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        
        foreach (var kvp in this)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
    
    public void OnAfterDeserialize()
    {
        Clear();
        
        for (int i = 0; i < keys.Count; i++)
        {
            Add(keys[i], values[i]);
        }
    }
}