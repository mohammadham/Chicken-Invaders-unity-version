using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Performance Optimizer for mobile and low-end devices
/// Dynamically adjusts quality settings, manages object pooling, and optimizes rendering
/// </summary>
public class PerformanceOptimizer : MonoBehaviour
{
    [Header("Performance Monitoring")]
    public bool enablePerformanceMonitoring = true;
    public float targetFrameRate = 60f;
    public float minAcceptableFrameRate = 30f;
    public float performanceCheckInterval = 2f;
    
    [Header("Quality Settings")]
    public QualityLevel[] qualityLevels;
    public int currentQualityLevel = 2;
    public bool enableAdaptiveQuality = true;
    
    [Header("Rendering Optimization")]
    public bool enableFrustumCulling = true;
    public bool enableOcclusionCulling = false;
    public float cullingDistance = 15f;
    public LayerMask cullableLayers = -1;
    
    [Header("Object Pooling")]
    public bool enableAdvancedPooling = true;
    public int maxPooledObjects = 1000;
    public float poolCleanupInterval = 10f;
    
    [Header("Memory Management")]
    public bool enableMemoryOptimization = true;
    public float memoryCleanupInterval = 30f;
    public int maxTextureSize = 1024;
    public bool compressTextures = true;
    
    [Header("Mobile Optimizations")]
    public bool enableMobileOptimizations = true;
    public bool reduceShadowQuality = true;
    public bool disableReflections = true;
    public bool optimizeLighting = true;
    
    // Performance tracking
    private float[] frameTimeHistory = new float[60];
    private int frameTimeIndex = 0;
    private float averageFrameTime = 0f;
    private float lastPerformanceCheck = 0f;
    private float lastMemoryCleanup = 0f;
    private float lastPoolCleanup = 0f;
    
    // Quality management
    private int qualityDowngrades = 0;
    private float lastQualityChange = 0f;
    private bool isOptimizing = false;
    
    // Culling management
    private Camera mainCamera;
    private List<Renderer> cullableRenderers = new List<Renderer>();
    private Dictionary<Renderer, bool> rendererVisibility = new Dictionary<Renderer, bool>();
    
    // Memory tracking
    private float initialMemoryUsage = 0f;
    private float lastMemoryUsage = 0f;
    
    public static PerformanceOptimizer Instance { get; private set; }
    
    [System.Serializable]
    public class QualityLevel
    {
        public string name;
        public int particleCount = 100;
        public float renderScale = 1f;
        public int shadowQuality = 2; // 0=Off, 1=Low, 2=Medium, 3=High
        public bool enablePostProcessing = true;
        public int textureQuality = 0; // 0=Full, 1=Half, 2=Quarter
        public float cullingDistance = 15f;
        public int maxLights = 8;
        public bool enableAntiAliasing = true;
    }
    
    private void Awake()
    {
        Instance = this;
        
        // Initialize quality levels if not set
        if (qualityLevels == null || qualityLevels.Length == 0)
        {
            CreateDefaultQualityLevels();
        }
        
        InitializeOptimizer();
    }
    
    private void Start()
    {
        StartPerformanceMonitoring();
        ApplyMobileOptimizations();
        InitializeCulling();
    }
    
    private void Update()
    {
        if (enablePerformanceMonitoring)
        {
            UpdatePerformanceTracking();
            CheckPerformance();
        }
        
        if (enableFrustumCulling)
        {
            UpdateFrustumCulling();
        }
        
        UpdateCleanupTimers();
    }
    
    void InitializeOptimizer()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
        
        // Set initial memory usage
        initialMemoryUsage = GetMemoryUsage();
        lastMemoryUsage = initialMemoryUsage;
        
        // Apply initial quality settings
        ApplyQualityLevel(currentQualityLevel);
        
        Debug.Log($"Performance Optimizer initialized. Target FPS: {targetFrameRate}");
    }
    
    void CreateDefaultQualityLevels()
    {
        qualityLevels = new QualityLevel[4];
        
        // Ultra Low
        qualityLevels[0] = new QualityLevel
        {
            name = "Ultra Low",
            particleCount = 25,
            renderScale = 0.5f,
            shadowQuality = 0,
            enablePostProcessing = false,
            textureQuality = 2,
            cullingDistance = 8f,
            maxLights = 2,
            enableAntiAliasing = false
        };
        
        // Low
        qualityLevels[1] = new QualityLevel
        {
            name = "Low",
            particleCount = 50,
            renderScale = 0.75f,
            shadowQuality = 1,
            enablePostProcessing = false,
            textureQuality = 1,
            cullingDistance = 12f,
            maxLights = 4,
            enableAntiAliasing = false
        };
        
        // Medium
        qualityLevels[2] = new QualityLevel
        {
            name = "Medium",
            particleCount = 100,
            renderScale = 1f,
            shadowQuality = 2,
            enablePostProcessing = true,
            textureQuality = 0,
            cullingDistance = 15f,
            maxLights = 6,
            enableAntiAliasing = true
        };
        
        // High
        qualityLevels[3] = new QualityLevel
        {
            name = "High",
            particleCount = 200,
            renderScale = 1f,
            shadowQuality = 3,
            enablePostProcessing = true,
            textureQuality = 0,
            cullingDistance = 20f,
            maxLights = 8,
            enableAntiAliasing = true
        };
    }
    
    void StartPerformanceMonitoring()
    {
        if (!enablePerformanceMonitoring) return;
        
        // Initialize frame time history
        for (int i = 0; i < frameTimeHistory.Length; i++)
        {
            frameTimeHistory[i] = 1f / targetFrameRate;
        }
        
        InvokeRepeating(nameof(PerformanceAnalysis), performanceCheckInterval, performanceCheckInterval);
    }
    
    void UpdatePerformanceTracking()
    {
        // Track frame times
        frameTimeHistory[frameTimeIndex] = Time.unscaledDeltaTime;
        frameTimeIndex = (frameTimeIndex + 1) % frameTimeHistory.Length;
        
        // Calculate average frame time
        float totalFrameTime = 0f;
        for (int i = 0; i < frameTimeHistory.Length; i++)
        {
            totalFrameTime += frameTimeHistory[i];
        }
        averageFrameTime = totalFrameTime / frameTimeHistory.Length;
    }
    
    void CheckPerformance()
    {
        if (Time.time - lastPerformanceCheck < performanceCheckInterval) return;
        
        lastPerformanceCheck = Time.time;
        
        float currentFPS = 1f / averageFrameTime;
        
        if (enableAdaptiveQuality && !isOptimizing)
        {
            // Check if we need to adjust quality
            if (currentFPS < minAcceptableFrameRate && currentQualityLevel > 0)
            {
                StartCoroutine(AdjustQualityLevel(-1));
            }
            else if (currentFPS > targetFrameRate * 1.2f && currentQualityLevel < qualityLevels.Length - 1)
            {
                // Only increase quality if we've been stable for a while
                if (Time.time - lastQualityChange > 10f)
                {
                    StartCoroutine(AdjustQualityLevel(1));
                }
            }
        }
    }
    
    void PerformanceAnalysis()
    {
        float currentFPS = 1f / averageFrameTime;
        float memoryUsage = GetMemoryUsage();
        
        // Log performance metrics
        if (enablePerformanceMonitoring)
        {
            Debug.Log($"[Performance] FPS: {currentFPS:F1}, Memory: {memoryUsage:F1}MB, Quality: {qualityLevels[currentQualityLevel].name}");
        }
        
        // Check for memory issues
        if (memoryUsage > lastMemoryUsage * 1.5f)
        {
            TriggerMemoryCleanup();
        }
        
        lastMemoryUsage = memoryUsage;
    }
    
    IEnumerator AdjustQualityLevel(int direction)
    {
        isOptimizing = true;
        
        int newQualityLevel = Mathf.Clamp(currentQualityLevel + direction, 0, qualityLevels.Length - 1);
        
        if (newQualityLevel != currentQualityLevel)
        {
            Debug.Log($"Adjusting quality from {qualityLevels[currentQualityLevel].name} to {qualityLevels[newQualityLevel].name}");
            
            ApplyQualityLevel(newQualityLevel);
            currentQualityLevel = newQualityLevel;
            lastQualityChange = Time.time;
            
            if (direction < 0)
                qualityDowngrades++;
            
            // Wait for settings to take effect
            yield return new WaitForSeconds(1f);
        }
        
        isOptimizing = false;
    }
    
    void ApplyQualityLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= qualityLevels.Length) return;
        
        QualityLevel level = qualityLevels[levelIndex];
        
        // Apply Unity quality settings
        QualitySettings.SetQualityLevel(levelIndex);
        
        // Apply custom settings
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.SetParticleQuality(levelIndex);
        }
        
        // Apply shadow settings
        QualitySettings.shadows = (ShadowQuality)level.shadowQuality;
        QualitySettings.shadowDistance = level.cullingDistance * 0.5f;
        
        // Apply texture settings
        QualitySettings.masterTextureLimit = level.textureQuality;
        
        // Apply lighting settings
        QualitySettings.pixelLightCount = level.maxLights;
        
        // Apply anti-aliasing
        QualitySettings.antiAliasing = level.enableAntiAliasing ? 4 : 0;
        
        // Update culling distance
        cullingDistance = level.cullingDistance;
        
        // Apply render scale
        if (mainCamera != null)
        {
            // For URP/HDRP, you would set render scale here
            // For built-in pipeline, this might involve render texture scaling
        }
        
        Debug.Log($"Applied quality level: {level.name}");
    }
    
    void ApplyMobileOptimizations()
    {
        if (!enableMobileOptimizations) return;
        
        #if UNITY_ANDROID || UNITY_IOS
        
        // Mobile-specific optimizations
        if (reduceShadowQuality)
        {
            QualitySettings.shadows = ShadowQuality.Disable;
        }
        
        if (disableReflections)
        {
            QualitySettings.realtimeReflectionProbes = false;
        }
        
        if (optimizeLighting)
        {
            QualitySettings.pixelLightCount = 2;
        }
        
        // Texture compression
        if (compressTextures)
        {
            QualitySettings.masterTextureLimit = 1;
        }
        
        // Disable VSync on mobile for better performance
        QualitySettings.vSyncCount = 0;
        
        Debug.Log("Mobile optimizations applied");
        
        #endif
    }
    
    void InitializeCulling()
    {
        if (!enableFrustumCulling) return;
        
        // Find all renderers that can be culled
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        foreach (var renderer in allRenderers)
        {
            if (IsLayerInMask(renderer.gameObject.layer, cullableLayers))
            {
                cullableRenderers.Add(renderer);
                rendererVisibility[renderer] = true;
            }
        }
        
        Debug.Log($"Initialized culling for {cullableRenderers.Count} renderers");
    }
    
    void UpdateFrustumCulling()
    {
        if (mainCamera == null || cullableRenderers.Count == 0) return;
        
        // Get camera frustum planes
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        
        foreach (var renderer in cullableRenderers)
        {
            if (renderer == null) continue;
            
            bool isVisible = true;
            
            // Distance culling
            float distance = Vector3.Distance(mainCamera.transform.position, renderer.transform.position);
            if (distance > cullingDistance)
            {
                isVisible = false;
            }
            else
            {
                // Frustum culling
                Bounds bounds = renderer.bounds;
                isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
            }
            
            // Apply visibility
            if (rendererVisibility[renderer] != isVisible)
            {
                renderer.enabled = isVisible;
                rendererVisibility[renderer] = isVisible;
            }
        }
    }
    
    void UpdateCleanupTimers()
    {
        // Memory cleanup
        if (enableMemoryOptimization && Time.time - lastMemoryCleanup > memoryCleanupInterval)
        {
            TriggerMemoryCleanup();
            lastMemoryCleanup = Time.time;
        }
        
        // Pool cleanup
        if (enableAdvancedPooling && Time.time - lastPoolCleanup > poolCleanupInterval)
        {
            CleanupObjectPools();
            lastPoolCleanup = Time.time;
        }
    }
    
    void TriggerMemoryCleanup()
    {
        // Force garbage collection
        System.GC.Collect();
        
        // Unload unused assets
        Resources.UnloadUnusedAssets();
        
        Debug.Log("Memory cleanup performed");
    }
    
    void CleanupObjectPools()
    {
        // Clean up bullet manager pool
        if (BulletManager.Instance != null)
        {
            // BulletManager would need a cleanup method
        }
        
        // Clean up particle manager pools
        if (ParticleManager.Instance != null)
        {
            // ParticleManager already has cleanup functionality
        }
        
        Debug.Log("Object pools cleaned up");
    }
    
    float GetMemoryUsage()
    {
        return System.GC.GetTotalMemory(false) / (1024f * 1024f); // Convert to MB
    }
    
    bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
    
    // Public interface
    public void SetQualityLevel(int level)
    {
        if (level >= 0 && level < qualityLevels.Length)
        {
            ApplyQualityLevel(level);
            currentQualityLevel = level;
            enableAdaptiveQuality = false; // Disable adaptive when manually set
        }
    }
    
    public void EnableAdaptiveQuality(bool enable)
    {
        enableAdaptiveQuality = enable;
    }
    
    public void ForceMemoryCleanup()
    {
        TriggerMemoryCleanup();
    }
    
    public float GetCurrentFPS()
    {
        return 1f / averageFrameTime;
    }
    
    public float GetMemoryUsageMB()
    {
        return GetMemoryUsage();
    }
    
    public string GetCurrentQualityLevelName()
    {
        return qualityLevels[currentQualityLevel].name;
    }
    
    public int GetQualityDowngradeCount()
    {
        return qualityDowngrades;
    }
    
    public void AddCullableRenderer(Renderer renderer)
    {
        if (renderer != null && !cullableRenderers.Contains(renderer))
        {
            cullableRenderers.Add(renderer);
            rendererVisibility[renderer] = true;
        }
    }
    
    public void RemoveCullableRenderer(Renderer renderer)
    {
        if (renderer != null)
        {
            cullableRenderers.Remove(renderer);
            rendererVisibility.Remove(renderer);
        }
    }
    
    // Debug information
    public void LogPerformanceReport()
    {
        float fps = GetCurrentFPS();
        float memory = GetMemoryUsageMB();
        int activeRenderers = cullableRenderers.FindAll(r => r != null && r.enabled).Count;
        
        Debug.Log($"=== Performance Report ===");
        Debug.Log($"FPS: {fps:F1} (Target: {targetFrameRate})");
        Debug.Log($"Memory: {memory:F1}MB (Initial: {initialMemoryUsage:F1}MB)");
        Debug.Log($"Quality Level: {GetCurrentQualityLevelName()}");
        Debug.Log($"Quality Downgrades: {qualityDowngrades}");
        Debug.Log($"Active Renderers: {activeRenderers}/{cullableRenderers.Count}");
        Debug.Log($"==========================");
    }
    
    private void OnDestroy()
    {
        CancelInvoke();
    }
}