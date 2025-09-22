using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Advanced Particle Effects Manager for enhanced visual experience
/// Manages explosions, muzzle flashes, trails, and environmental effects
/// </summary>
public class ParticleManager : MonoBehaviour
{
    [Header("Explosion Effects")]
    public GameObject[] explosionPrefabs;
    public GameObject enemyExplosionPrefab;
    public GameObject playerExplosionPrefab;
    public GameObject bigExplosionPrefab;
    
    [Header("Weapon Effects")]
    public GameObject[] muzzleFlashPrefabs;
    public GameObject[] bulletTrailPrefabs;
    public GameObject laserBeamEffect;
    public GameObject plasmaArcEffect;
    
    [Header("Environmental Effects")]
    public GameObject starFieldEffect;
    public GameObject nebulaDustEffect;
    public GameObject engineTrailEffect;
    public GameObject shieldEffect;
    
    [Header("UI Effects")]
    public GameObject coinSparkleEffect;
    public GameObject powerUpGlowEffect;
    public GameObject levelUpEffect;
    public GameObject screenFlashEffect;
    
    [Header("Pool Settings")]
    public int explosionPoolSize = 20;
    public int muzzleFlashPoolSize = 30;
    public int trailPoolSize = 50;
    public int miscEffectPoolSize = 25;
    
    [Header("Performance")]
    public int maxActiveParticles = 100;
    public bool enableDynamicQuality = true;
    public float particleLifetimeLimit = 10f;
    
    // Object pools
    private Queue<GameObject> explosionPool = new Queue<GameObject>();
    private Queue<GameObject> muzzleFlashPool = new Queue<GameObject>();
    private Queue<GameObject> trailPool = new Queue<GameObject>();
    private Queue<GameObject> miscEffectPool = new Queue<GameObject>();
    
    // Active effects tracking
    private List<GameObject> activeEffects = new List<GameObject>();
    private Dictionary<string, float> effectCooldowns = new Dictionary<string, float>();
    
    // Performance tracking
    private int currentActiveParticles = 0;
    private float lastQualityCheck = 0f;
    private int particleQualityLevel = 2; // 0=Low, 1=Medium, 2=High
    
    public static ParticleManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        InitializeParticlePools();
    }
    
    private void Start()
    {
        StartEnvironmentalEffects();
    }
    
    private void Update()
    {
        UpdateActiveEffects();
        UpdatePerformanceOptimization();
        UpdateEffectCooldowns();
    }
    
    void InitializeParticlePools()
    {
        // Initialize explosion pool
        for (int i = 0; i < explosionPoolSize; i++)
        {
            if (explosionPrefabs != null && explosionPrefabs.Length > 0)
            {
                GameObject explosion = CreatePooledEffect(explosionPrefabs[0], "ExplosionPool");
                explosionPool.Enqueue(explosion);
            }
        }
        
        // Initialize muzzle flash pool
        for (int i = 0; i < muzzleFlashPoolSize; i++)
        {
            if (muzzleFlashPrefabs != null && muzzleFlashPrefabs.Length > 0)
            {
                GameObject muzzleFlash = CreatePooledEffect(muzzleFlashPrefabs[0], "MuzzleFlashPool");
                muzzleFlashPool.Enqueue(muzzleFlash);
            }
        }
        
        // Initialize trail pool
        for (int i = 0; i < trailPoolSize; i++)
        {
            if (bulletTrailPrefabs != null && bulletTrailPrefabs.Length > 0)
            {
                GameObject trail = CreatePooledEffect(bulletTrailPrefabs[0], "TrailPool");
                trailPool.Enqueue(trail);
            }
        }
        
        // Initialize misc effect pool
        for (int i = 0; i < miscEffectPoolSize; i++)
        {
            if (coinSparkleEffect != null)
            {
                GameObject misc = CreatePooledEffect(coinSparkleEffect, "MiscEffectPool");
                miscEffectPool.Enqueue(misc);
            }
        }
        
        Debug.Log($"Particle Manager initialized with {explosionPoolSize + muzzleFlashPoolSize + trailPoolSize + miscEffectPoolSize} pooled effects");
    }
    
    GameObject CreatePooledEffect(GameObject prefab, string poolName)
    {
        GameObject poolParent = GameObject.Find(poolName);
        if (poolParent == null)
        {
            poolParent = new GameObject(poolName);
            poolParent.transform.SetParent(transform);
        }
        
        GameObject effect = Instantiate(prefab, poolParent.transform);
        effect.SetActive(false);
        
        // Add automatic return to pool component
        EffectAutoReturn autoReturn = effect.GetComponent<EffectAutoReturn>();
        if (autoReturn == null)
            autoReturn = effect.AddComponent<EffectAutoReturn>();
        
        return effect;
    }
    
    void StartEnvironmentalEffects()
    {
        // Start continuous environmental effects
        if (starFieldEffect != null)
        {
            GameObject starField = Instantiate(starFieldEffect);
            starField.transform.position = Vector3.zero;
            // Don't add to active effects as it's permanent
        }
        
        if (nebulaDustEffect != null)
        {
            GameObject nebula = Instantiate(nebulaDustEffect);
            nebula.transform.position = Vector3.zero;
        }
    }
    
    void UpdateActiveEffects()
    {
        // Clean up completed effects
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i] == null || !activeEffects[i].activeInHierarchy)
            {
                activeEffects.RemoveAt(i);
                continue;
            }
            
            // Check if effect has exceeded lifetime limit
            EffectAutoReturn autoReturn = activeEffects[i].GetComponent<EffectAutoReturn>();
            if (autoReturn != null && autoReturn.GetAge() > particleLifetimeLimit)
            {
                ReturnEffectToPool(activeEffects[i]);
                activeEffects.RemoveAt(i);
            }
        }
        
        // Update active particle count
        currentActiveParticles = 0;
        foreach (var effect in activeEffects)
        {
            if (effect != null)
            {
                ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particles)
                {
                    if (ps.isPlaying)
                        currentActiveParticles += ps.particleCount;
                }
            }
        }
    }
    
    void UpdatePerformanceOptimization()
    {
        if (!enableDynamicQuality) return;
        
        if (Time.time - lastQualityCheck > 1f) // Check every second
        {
            lastQualityCheck = Time.time;
            
            // Adjust quality based on performance
            float frameRate = 1f / Time.deltaTime;
            
            if (frameRate < 30f && particleQualityLevel > 0)
            {
                // Reduce quality
                particleQualityLevel--;
                AdjustParticleQuality();
            }
            else if (frameRate > 60f && particleQualityLevel < 2)
            {
                // Increase quality
                particleQualityLevel++;
                AdjustParticleQuality();
            }
        }
        
        // Limit active particles if too many
        if (currentActiveParticles > maxActiveParticles)
        {
            ReduceParticleCount();
        }
    }
    
    void AdjustParticleQuality()
    {
        float qualityMultiplier = 1f;
        switch (particleQualityLevel)
        {
            case 0: qualityMultiplier = 0.5f; break; // Low
            case 1: qualityMultiplier = 0.75f; break; // Medium
            case 2: qualityMultiplier = 1f; break; // High
        }
        
        // Apply quality settings to all active particle systems
        foreach (var effect in activeEffects)
        {
            if (effect != null)
            {
                ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particles)
                {
                    var main = ps.main;
                    var emission = ps.emission;
                    
                    // Adjust particle count based on quality
                    emission.rateOverTime = emission.rateOverTime.constant * qualityMultiplier;
                }
            }
        }
    }
    
    void ReduceParticleCount()
    {
        // Stop oldest effects to reduce particle count
        int effectsToStop = Mathf.CeilToInt(activeEffects.Count * 0.2f);
        
        for (int i = 0; i < effectsToStop && i < activeEffects.Count; i++)
        {
            if (activeEffects[i] != null)
            {
                ParticleSystem[] particles = activeEffects[i].GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particles)
                {
                    ps.Stop();
                }
            }
        }
    }
    
    void UpdateEffectCooldowns()
    {
        List<string> keysToRemove = new List<string>();
        
        foreach (var kvp in effectCooldowns)
        {
            if (Time.time >= kvp.Value)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (string key in keysToRemove)
        {
            effectCooldowns.Remove(key);
        }
    }
    
    // Public effect creation methods
    public void CreateExplosion(Vector3 position, ExplosionType type = ExplosionType.Normal, float scale = 1f)
    {
        GameObject explosionPrefab = GetExplosionPrefab(type);
        if (explosionPrefab == null) return;
        
        GameObject explosion = GetPooledEffect(explosionPool, explosionPrefab, "explosion");
        if (explosion != null)
        {
            explosion.transform.position = position;
            explosion.transform.localScale = Vector3.one * scale;
            explosion.SetActive(true);
            
            activeEffects.Add(explosion);
            
            // Screen shake effect
            if (CameraController.Instance != null)
            {
                float shakeIntensity = Mathf.Clamp(scale * 0.1f, 0.05f, 0.3f);
                CameraController.Instance.AddShake(shakeIntensity, 0.3f);
            }
            
            // Play explosion sound
            if (AdvancedAudioManager.Instance != null)
            {
                AdvancedAudioManager.Instance.PlaySFX(GetExplosionSound(type), position);
            }
        }
    }
    
    public void CreateMuzzleFlash(Vector3 position, Vector3 direction, BulletType weaponType)
    {
        if (IsEffectOnCooldown("muzzleflash", 0.05f)) return;
        
        GameObject muzzleFlashPrefab = GetMuzzleFlashPrefab(weaponType);
        if (muzzleFlashPrefab == null) return;
        
        GameObject muzzleFlash = GetPooledEffect(muzzleFlashPool, muzzleFlashPrefab, "muzzleflash");
        if (muzzleFlash != null)
        {
            muzzleFlash.transform.position = position;
            muzzleFlash.transform.rotation = Quaternion.LookRotation(direction);
            muzzleFlash.SetActive(true);
            
            activeEffects.Add(muzzleFlash);
            
            // Auto-destroy after short time
            StartCoroutine(ReturnEffectAfterDelay(muzzleFlash, 0.2f));
        }
    }
    
    public void CreateBulletTrail(Vector3 startPos, Vector3 endPos, BulletType bulletType, float duration = 0.5f)
    {
        GameObject trailPrefab = GetTrailPrefab(bulletType);
        if (trailPrefab == null) return;
        
        GameObject trail = GetPooledEffect(trailPool, trailPrefab, "trail");
        if (trail != null)
        {
            trail.transform.position = startPos;
            
            // Configure trail to go from start to end
            TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                StartCoroutine(AnimateTrail(trail, startPos, endPos, duration));
            }
            
            trail.SetActive(true);
            activeEffects.Add(trail);
        }
    }
    
    public void CreateCoinSparkle(Vector3 position)
    {
        GameObject sparkle = GetPooledEffect(miscEffectPool, coinSparkleEffect, "sparkle");
        if (sparkle != null)
        {
            sparkle.transform.position = position;
            sparkle.SetActive(true);
            
            activeEffects.Add(sparkle);
            StartCoroutine(ReturnEffectAfterDelay(sparkle, 2f));
        }
    }
    
    public void CreatePowerUpGlow(Vector3 position, Color glowColor)
    {
        GameObject glow = GetPooledEffect(miscEffectPool, powerUpGlowEffect, "glow");
        if (glow != null)
        {
            glow.transform.position = position;
            
            // Set glow color
            ParticleSystem ps = glow.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = glowColor;
            }
            
            glow.SetActive(true);
            activeEffects.Add(glow);
            StartCoroutine(ReturnEffectAfterDelay(glow, 3f));
        }
    }
    
    public void CreateScreenFlash(Color flashColor, float intensity = 0.5f, float duration = 0.2f)
    {
        if (screenFlashEffect != null)
        {
            GameObject flash = Instantiate(screenFlashEffect);
            
            // Configure flash effect
            Image flashImage = flash.GetComponent<Image>();
            if (flashImage != null)
            {
                flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, intensity);
                StartCoroutine(FadeFlashEffect(flashImage, duration));
            }
        }
    }
    
    public void CreateShieldEffect(Transform target, float duration = 3f)
    {
        if (shieldEffect != null)
        {
            GameObject shield = Instantiate(shieldEffect, target);
            shield.transform.localPosition = Vector3.zero;
            
            activeEffects.Add(shield);
            StartCoroutine(ReturnEffectAfterDelay(shield, duration));
        }
    }
    
    // Utility methods
    GameObject GetExplosionPrefab(ExplosionType type)
    {
        switch (type)
        {
            case ExplosionType.Normal:
                return explosionPrefabs != null && explosionPrefabs.Length > 0 ? explosionPrefabs[0] : null;
            case ExplosionType.Enemy:
                return enemyExplosionPrefab;
            case ExplosionType.Player:
                return playerExplosionPrefab;
            case ExplosionType.Big:
                return bigExplosionPrefab;
            default:
                return explosionPrefabs != null && explosionPrefabs.Length > 0 ? explosionPrefabs[0] : null;
        }
    }
    
    GameObject GetMuzzleFlashPrefab(BulletType weaponType)
    {
        if (muzzleFlashPrefabs == null || muzzleFlashPrefabs.Length == 0) return null;
        
        int index = (int)weaponType % muzzleFlashPrefabs.Length;
        return muzzleFlashPrefabs[index];
    }
    
    GameObject GetTrailPrefab(BulletType bulletType)
    {
        if (bulletTrailPrefabs == null || bulletTrailPrefabs.Length == 0) return null;
        
        int index = (int)bulletType % bulletTrailPrefabs.Length;
        return bulletTrailPrefabs[index];
    }
    
    AudioClip GetExplosionSound(ExplosionType type)
    {
        // This would return appropriate sound based on explosion type
        // For now, return null and let AudioManager handle it
        return null;
    }
    
    GameObject GetPooledEffect(Queue<GameObject> pool, GameObject prefab, string effectType)
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        
        // Create new effect if pool is empty
        GameObject newEffect = CreatePooledEffect(prefab, effectType + "Pool");
        return newEffect;
    }
    
    void ReturnEffectToPool(GameObject effect)
    {
        if (effect == null) return;
        
        effect.SetActive(false);
        
        // Determine which pool this effect belongs to and return it
        if (effect.name.Contains("Explosion"))
            explosionPool.Enqueue(effect);
        else if (effect.name.Contains("MuzzleFlash"))
            muzzleFlashPool.Enqueue(effect);
        else if (effect.name.Contains("Trail"))
            trailPool.Enqueue(effect);
        else
            miscEffectPool.Enqueue(effect);
    }
    
    bool IsEffectOnCooldown(string effectType, float cooldownTime)
    {
        if (effectCooldowns.ContainsKey(effectType) && Time.time < effectCooldowns[effectType])
            return true;
            
        effectCooldowns[effectType] = Time.time + cooldownTime;
        return false;
    }
    
    IEnumerator ReturnEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (effect != null)
        {
            activeEffects.Remove(effect);
            ReturnEffectToPool(effect);
        }
    }
    
    IEnumerator AnimateTrail(GameObject trail, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, progress);
            trail.transform.position = currentPos;
            
            yield return null;
        }
    }
    
    IEnumerator FadeFlashEffect(Image flashImage, float duration)
    {
        Color startColor = flashImage.color;
        Color endColor = startColor;
        endColor.a = 0f;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            flashImage.color = Color.Lerp(startColor, endColor, progress);
            yield return null;
        }
        
        Destroy(flashImage.gameObject);
    }
    
    // Public utility methods
    public void ClearAllEffects()
    {
        foreach (var effect in activeEffects)
        {
            if (effect != null)
                ReturnEffectToPool(effect);
        }
        activeEffects.Clear();
    }
    
    public void SetParticleQuality(int quality)
    {
        particleQualityLevel = Mathf.Clamp(quality, 0, 2);
        AdjustParticleQuality();
    }
    
    public int GetActiveEffectCount()
    {
        return activeEffects.Count;
    }
    
    public int GetActiveParticleCount()
    {
        return currentActiveParticles;
    }
}

/// <summary>
/// Explosion types for different visual effects
/// </summary>
public enum ExplosionType
{
    Normal,
    Enemy,
    Player,
    Big
}

/// <summary>
/// Component to automatically return effects to pool
/// </summary>
public class EffectAutoReturn : MonoBehaviour
{
    private float startTime;
    
    private void OnEnable()
    {
        startTime = Time.time;
    }
    
    public float GetAge()
    {
        return Time.time - startTime;
    }
}

/// <summary>
/// Camera controller for screen shake effects
/// </summary>
public class CameraController : MonoBehaviour
{
    private Vector3 originalPosition;
    private float shakeIntensity = 0f;
    private float shakeDuration = 0f;
    private float shakeTimer = 0f;
    
    public static CameraController Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        originalPosition = transform.position;
    }
    
    private void Update()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity;
            shakeOffset.z = 0f; // Keep Z position unchanged for 2D
            
            transform.position = originalPosition + shakeOffset;
            
            if (shakeTimer <= 0f)
            {
                transform.position = originalPosition;
                shakeIntensity = 0f;
            }
        }
    }
    
    public void AddShake(float intensity, float duration)
    {
        if (intensity > shakeIntensity)
        {
            shakeIntensity = intensity;
            shakeDuration = duration;
            shakeTimer = duration;
        }
    }
}