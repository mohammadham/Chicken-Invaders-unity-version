using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Advanced Audio Manager with dynamic music, spatial audio, and professional mixing
/// Complete replacement for basic AudioManager with advanced features
/// </summary>
public class AdvancedAudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixerGroup masterMixerGroup;
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup voiceMixerGroup;
    public AudioMixerGroup ambientMixerGroup;
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambientSource;
    public AudioSource uiSfxSource;
    [SerializeField] private List<AudioSource> sfxSourcePool = new List<AudioSource>();
    
    [Header("Dynamic Music System")]
    public AudioClip[] menuMusic;
    public AudioClip[] gameplayMusicLayers;
    public AudioClip[] intenseMusicLayers;
    public AudioClip[] victoryMusic;
    public AudioClip[] gameOverMusic;
    public float musicFadeDuration = 2f;
    public AnimationCurve musicFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Weapon Audio Library")]
    public WeaponAudioData[] weaponAudioData;
    
    [Header("UI Audio Library")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    public AudioClip menuTransitionSound;
    public AudioClip errorSound;
    public AudioClip successSound;
    public AudioClip coinPickupSound;
    public AudioClip powerUpSound;
    
    [Header("Enemy Audio Library")]
    public AudioClip[] enemyHitSounds;
    public AudioClip[] enemyDeathSounds;
    public AudioClip[] enemyMovementSounds;
    
    [Header("Player Audio Library")]
    public AudioClip playerHitSound;
    public AudioClip playerDeathSound;
    public AudioClip playerRespawnSound;
    public AudioClip shieldActivateSound;
    public AudioClip weaponOverheatSound;
    
    [Header("Ambient Audio")]
    public AudioClip spaceAmbientLoop;
    public AudioClip engineHumLoop;
    public float ambientVolume = 0.3f;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    [Range(0f, 1f)] public float ambientVolumeLevel = 0.5f;
    
    [Header("Advanced Features")]
    public bool enableSpatialAudio = true;
    public bool enableDopplerEffect = false;
    public bool enableReverbZones = true;
    public int maxSimultaneousSFX = 32;
    public float sfxCooldownTime = 0.05f; // Prevent audio spam
    
    [Header("Performance")]
    public int audioSourcePoolSize = 20;
    public bool enableAudioCompression = true;
    public AudioCompressionFormat compressionFormat = AudioCompressionFormat.Vorbis;
    
    // Internal state
    private MusicState currentMusicState = MusicState.Menu;
    private Dictionary<BulletType, WeaponAudioData> weaponAudioLookup;
    private Dictionary<string, float> sfxCooldowns = new Dictionary<string, float>();
    private Coroutine musicTransitionCoroutine;
    private float gameIntensity = 0f; // 0-1 scale for dynamic music
    
    // Audio source management
    private Queue<AudioSource> availableSfxSources = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    
    public static AdvancedAudioManager Instance { get; private set; }
    
    public enum MusicState
    {
        Menu,
        Gameplay,
        Intense,
        Victory,
        GameOver,
        Paused
    }
    
    private void Awake()
    {
        // Singleton with persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadAudioSettings();
        SetupDynamicMusic();
        StartAmbientAudio();
    }
    
    private void Update()
    {
        UpdateAudioSystem();
        UpdateSfxCooldowns();
        UpdateDynamicMusic();
    }
    
    void InitializeAudioSystem()
    {
        // Create weapon audio lookup
        weaponAudioLookup = new Dictionary<BulletType, WeaponAudioData>();
        if (weaponAudioData != null)
        {
            foreach (var weaponAudio in weaponAudioData)
            {
                weaponAudioLookup[weaponAudio.weaponType] = weaponAudio;
            }
        }
        
        // Initialize audio source pool
        InitializeAudioSourcePool();
        
        // Setup audio mixer
        SetupAudioMixer();
        
        Debug.Log("Advanced Audio Manager initialized with " + audioSourcePoolSize + " audio sources");
    }
    
    void InitializeAudioSourcePool()
    {
        // Create audio source pool for better performance
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject audioSourceObj = new GameObject($"SFX_AudioSource_{i}");
            audioSourceObj.transform.SetParent(transform);
            
            AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = enableSpatialAudio ? 1f : 0f;
            
            sfxSourcePool.Add(audioSource);
            availableSfxSources.Enqueue(audioSource);
        }
    }
    
    void SetupAudioMixer()
    {
        if (masterMixerGroup == null) return;
        
        // Set initial mixer volumes
        SetMixerVolume("MasterVolume", masterVolume);
        SetMixerVolume("MusicVolume", musicVolume);
        SetMixerVolume("SFXVolume", sfxVolume);
        SetMixerVolume("VoiceVolume", voiceVolume);
        SetMixerVolume("AmbientVolume", ambientVolumeLevel);
    }
    
    void SetMixerVolume(string parameterName, float volume)
    {
        if (masterMixerGroup?.audioMixer != null)
        {
            float dbValue = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            masterMixerGroup.audioMixer.SetFloat(parameterName, dbValue);
        }
    }
    
    void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
        ambientVolumeLevel = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);
        
        ApplyAudioSettings();
    }
    
    void ApplyAudioSettings()
    {
        SetMixerVolume("MasterVolume", masterVolume);
        SetMixerVolume("MusicVolume", musicVolume);
        SetMixerVolume("SFXVolume", sfxVolume);
        SetMixerVolume("VoiceVolume", voiceVolume);
        SetMixerVolume("AmbientVolume", ambientVolumeLevel);
    }
    
    void UpdateAudioSystem()
    {
        // Update spatial audio positions
        if (enableSpatialAudio)
        {
            UpdateSpatialAudio();
        }
        
        // Manage active audio sources
        ManageActiveSfxSources();
    }
    
    void UpdateSpatialAudio()
    {
        // Update listener position to camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            AudioListener listener = mainCamera.GetComponent<AudioListener>();
            if (listener == null)
                listener = mainCamera.gameObject.AddComponent<AudioListener>();
        }
    }
    
    void ManageActiveSfxSources()
    {
        // Check for completed audio sources and return them to pool
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeSfxSources[i];
            if (source != null && !source.isPlaying)
            {
                ReturnSfxSourceToPool(source);
                activeSfxSources.RemoveAt(i);
            }
        }
    }
    
    void UpdateSfxCooldowns()
    {
        List<string> keysToRemove = new List<string>();
        
        foreach (var kvp in sfxCooldowns)
        {
            if (Time.time >= kvp.Value)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (string key in keysToRemove)
        {
            sfxCooldowns.Remove(key);
        }
    }
    
    void UpdateDynamicMusic()
    {
        if (currentMusicState != MusicState.Gameplay) return;
        
        // Calculate game intensity based on various factors
        float targetIntensity = CalculateGameIntensity();
        
        // Smooth transition to target intensity
        gameIntensity = Mathf.Lerp(gameIntensity, targetIntensity, Time.deltaTime * 2f);
        
        // Adjust music layers based on intensity
        AdjustMusicLayers();
    }
    
    float CalculateGameIntensity()
    {
        float intensity = 0f;
        
        // Factor 1: Enemy count (more enemies = higher intensity)
        if (EnemyManager.Instance != null)
        {
            int enemyCount = EnemyManager.Instance.GetAliveEnemyCount();
            intensity += Mathf.Clamp01(enemyCount / 20f) * 0.3f;
        }
        
        // Factor 2: Player health (lower health = higher intensity)
        if (GameManager.Instance != null)
        {
            var players = GameManager.Instance.GetPlayers();
            if (players.Count > 0)
            {
                float avgHealth = 0f;
                foreach (var player in players)
                {
                    if (player != null)
                        avgHealth += (float)player.lives / GameConstants.PLAYER_LIVES;
                }
                avgHealth /= players.Count;
                intensity += (1f - avgHealth) * 0.4f;
            }
        }
        
        // Factor 3: Weapon overheat (higher overheat = higher intensity)
        // This would need to be implemented based on your player controller
        
        // Factor 4: Recent combat activity
        intensity += 0.3f; // Base combat intensity
        
        return Mathf.Clamp01(intensity);
    }
    
    void AdjustMusicLayers()
    {
        // This would control multiple music layers based on intensity
        // For now, we'll switch between normal and intense music
        if (gameIntensity > 0.7f && currentMusicState == MusicState.Gameplay)
        {
            TransitionToMusicState(MusicState.Intense);
        }
        else if (gameIntensity < 0.3f && currentMusicState == MusicState.Intense)
        {
            TransitionToMusicState(MusicState.Gameplay);
        }
    }
    
    // Public Music Control Methods
    public void TransitionToMusicState(MusicState newState)
    {
        if (currentMusicState == newState) return;
        
        if (musicTransitionCoroutine != null)
            StopCoroutine(musicTransitionCoroutine);
            
        musicTransitionCoroutine = StartCoroutine(MusicTransitionCoroutine(newState));
    }
    
    IEnumerator MusicTransitionCoroutine(MusicState newState)
    {
        MusicState previousState = currentMusicState;
        currentMusicState = newState;
        
        AudioClip[] targetMusic = GetMusicForState(newState);
        if (targetMusic == null || targetMusic.Length == 0) yield break;
        
        AudioClip targetClip = targetMusic[Random.Range(0, targetMusic.Length)];
        
        // Fade out current music
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeAudioSource(musicSource, 0f, musicFadeDuration * 0.5f));
        }
        
        // Switch to new music
        musicSource.clip = targetClip;
        musicSource.loop = (newState == MusicState.Gameplay || newState == MusicState.Intense || newState == MusicState.Menu);
        
        // Fade in new music
        musicSource.Play();
        yield return StartCoroutine(FadeAudioSource(musicSource, musicVolume, musicFadeDuration * 0.5f));
        
        Debug.Log($"Music transitioned from {previousState} to {newState}");
    }
    
    AudioClip[] GetMusicForState(MusicState state)
    {
        switch (state)
        {
            case MusicState.Menu: return menuMusic;
            case MusicState.Gameplay: return gameplayMusicLayers;
            case MusicState.Intense: return intenseMusicLayers;
            case MusicState.Victory: return victoryMusic;
            case MusicState.GameOver: return gameOverMusic;
            default: return null;
        }
    }
    
    IEnumerator FadeAudioSource(AudioSource audioSource, float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = musicFadeCurve.Evaluate(elapsed / duration);
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, progress);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        
        if (targetVolume <= 0f)
        {
            audioSource.Stop();
        }
    }
    
    void SetupDynamicMusic()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
        }
        
        musicSource.outputAudioMixerGroup = musicMixerGroup;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        
        // Start with menu music
        TransitionToMusicState(MusicState.Menu);
    }
    
    void StartAmbientAudio()
    {
        if (ambientSource == null)
        {
            GameObject ambientObj = new GameObject("AmbientSource");
            ambientObj.transform.SetParent(transform);
            ambientSource = ambientObj.AddComponent<AudioSource>();
        }
        
        ambientSource.outputAudioMixerGroup = ambientMixerGroup;
        ambientSource.loop = true;
        ambientSource.volume = ambientVolume;
        
        if (spaceAmbientLoop != null)
        {
            ambientSource.clip = spaceAmbientLoop;
            ambientSource.Play();
        }
    }
    
    // SFX Playing Methods
    public void PlayWeaponSound(BulletType weaponType, Vector3 position = default)
    {
        if (!weaponAudioLookup.ContainsKey(weaponType)) return;
        
        WeaponAudioData weaponAudio = weaponAudioLookup[weaponType];
        AudioClip[] sounds = weaponAudio.fireSounds;
        
        if (sounds == null || sounds.Length == 0) return;
        
        AudioClip clipToPlay = sounds[Random.Range(0, sounds.Length)];
        PlaySFX(clipToPlay, position, weaponAudio.volume, weaponAudio.pitch);
    }
    
    public void PlayEnemyHit(Vector3 position = default)
    {
        if (enemyHitSounds == null || enemyHitSounds.Length == 0) return;
        
        AudioClip clip = enemyHitSounds[Random.Range(0, enemyHitSounds.Length)];
        PlaySFX(clip, position);
    }
    
    public void PlayEnemyDeath(Vector3 position = default)
    {
        if (enemyDeathSounds == null || enemyDeathSounds.Length == 0) return;
        
        AudioClip clip = enemyDeathSounds[Random.Range(0, enemyDeathSounds.Length)];
        PlaySFX(clip, position, 0.8f);
    }
    
    public void PlayPlayerHit()
    {
        PlaySFX(playerHitSound);
    }
    
    public void PlayPlayerDeath()
    {
        PlaySFX(playerDeathSound, Vector3.zero, 1f, 1f, sfxMixerGroup);
    }
    
    public void PlayCoinPickup()
    {
        PlaySFX(coinPickupSound, Vector3.zero, 0.7f, Random.Range(0.9f, 1.1f));
    }
    
    public void PlayPowerUp()
    {
        PlaySFX(powerUpSound, Vector3.zero, 0.8f);
    }
    
    public void PlayButtonClick()
    {
        PlayUISound(buttonClickSound);
    }
    
    public void PlayButtonHover()
    {
        PlayUISound(buttonHoverSound, 0.5f);
    }
    
    public void PlayMenuTransition()
    {
        PlayUISound(menuTransitionSound);
    }
    
    public void PlayError()
    {
        PlayUISound(errorSound);
    }
    
    public void PlaySuccess()
    {
        PlayUISound(successSound);
    }
    
    void PlayUISound(AudioClip clip, float volume = 1f)
    {
        if (clip == null || uiSfxSource == null) return;
        
        uiSfxSource.outputAudioMixerGroup = sfxMixerGroup;
        uiSfxSource.PlayOneShot(clip, volume);
    }
    
    public void PlaySFX(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f, AudioMixerGroup mixerGroup = null)
    {
        if (clip == null) return;
        
        // Check cooldown to prevent audio spam
        string clipName = clip.name;
        if (sfxCooldowns.ContainsKey(clipName) && Time.time < sfxCooldowns[clipName])
            return;
            
        sfxCooldowns[clipName] = Time.time + sfxCooldownTime;
        
        // Get audio source from pool
        AudioSource sfxSource = GetSfxSourceFromPool();
        if (sfxSource == null) return;
        
        // Configure audio source
        sfxSource.clip = clip;
        sfxSource.volume = volume * sfxVolume;
        sfxSource.pitch = pitch;
        sfxSource.outputAudioMixerGroup = mixerGroup ?? sfxMixerGroup;
        
        // Set spatial audio
        if (enableSpatialAudio && position != default)
        {
            sfxSource.transform.position = position;
            sfxSource.spatialBlend = 1f;
        }
        else
        {
            sfxSource.spatialBlend = 0f;
        }
        
        // Play sound
        sfxSource.Play();
        activeSfxSources.Add(sfxSource);
    }
    
    AudioSource GetSfxSourceFromPool()
    {
        if (availableSfxSources.Count > 0)
        {
            return availableSfxSources.Dequeue();
        }
        
        // If pool is empty, create new source (up to limit)
        if (sfxSourcePool.Count < maxSimultaneousSFX)
        {
            GameObject audioSourceObj = new GameObject($"SFX_AudioSource_Extra_{sfxSourcePool.Count}");
            audioSourceObj.transform.SetParent(transform);
            
            AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
            audioSource.playOnAwake = false;
            
            sfxSourcePool.Add(audioSource);
            return audioSource;
        }
        
        // Use oldest active source if we've hit the limit
        if (activeSfxSources.Count > 0)
        {
            AudioSource oldestSource = activeSfxSources[0];
            activeSfxSources.RemoveAt(0);
            oldestSource.Stop();
            return oldestSource;
        }
        
        return null;
    }
    
    void ReturnSfxSourceToPool(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            availableSfxSources.Enqueue(audioSource);
        }
    }
    
    // Public Settings Methods
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        SetMixerVolume("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        SetMixerVolume("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SetMixerVolume("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        SetMixerVolume("VoiceVolume", voiceVolume);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolumeLevel = Mathf.Clamp01(volume);
        SetMixerVolume("AmbientVolume", ambientVolumeLevel);
        PlayerPrefs.SetFloat("AmbientVolume", ambientVolumeLevel);
    }
    
    public void PauseAllAudio()
    {
        AudioListener.pause = true;
    }
    
    public void ResumeAllAudio()
    {
        AudioListener.pause = false;
    }
    
    public void StopAllSFX()
    {
        foreach (var source in activeSfxSources)
        {
            if (source != null)
                source.Stop();
        }
        activeSfxSources.Clear();
        
        // Return all sources to pool
        foreach (var source in sfxSourcePool)
        {
            if (source != null && !availableSfxSources.Contains(source))
                availableSfxSources.Enqueue(source);
        }
    }
    
    // Cleanup
    private void OnDestroy()
    {
        if (musicTransitionCoroutine != null)
            StopCoroutine(musicTransitionCoroutine);
    }
    
    // Compatibility methods with original AudioManager
    public void PlayBackgroundMusic() => TransitionToMusicState(MusicState.Menu);
    public void StopBackgroundMusic() => musicSource?.Stop();
    public void PlaySFX(AudioClip clip) => PlaySFX(clip, Vector3.zero);
    public void PlayPlayerExplosion() => PlayPlayerDeath();
    public void PlayEnemyExplosion() => PlayEnemyDeath();
    public void PlayWeaponPickup() => PlayPowerUp();
    public void ToggleMusic() => SetMusicVolume(musicVolume > 0 ? 0 : 0.7f);
    public void ToggleSFX() => SetSFXVolume(sfxVolume > 0 ? 0 : 0.8f);
}

/// <summary>
/// Weapon-specific audio data
/// </summary>
[System.Serializable]
public class WeaponAudioData
{
    public BulletType weaponType;
    public AudioClip[] fireSounds;
    public AudioClip[] reloadSounds;
    public AudioClip overheatSound;
    public AudioClip cooldownCompleteSound;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float pitch = 1f;
    public bool randomizePitch = true;
    [Range(0f, 0.5f)] public float pitchRandomRange = 0.1f;
}