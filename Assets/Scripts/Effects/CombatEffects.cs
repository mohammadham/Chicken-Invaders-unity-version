using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Combat visual and audio effects manager
/// Handles muzzle flashes, hit effects, explosions, screen shake
/// </summary>
public class CombatEffects : MonoBehaviour
{
    [Header("Effect Prefabs")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public GameObject explosionPrefab;
    public GameObject laserImpactPrefab;
    public GameObject plasmaImpactPrefab;
    
    [Header("Screen Shake")]
    public float screenShakeIntensity = 0.1f;
    public float screenShakeDuration = 0.2f;
    
    [Header("Particle Pools")]
    public int effectPoolSize = 20;
    
    private ObjectPool<ParticleSystem> muzzleFlashPool;
    private ObjectPool<ParticleSystem> hitEffectPool;
    private ObjectPool<ParticleSystem> explosionPool;
    
    private Camera mainCamera;
    private Vector3 cameraOriginalPosition;
    private Coroutine screenShakeCoroutine;
    
    public static CombatEffects Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        Initialize();
    }
    
    void Initialize()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraOriginalPosition = mainCamera.transform.position;
        }
        
        // Initialize effect pools
        InitializeEffectPools();
    }
    
    void InitializeEffectPools()
    {
        // Create simple particle prefabs if not assigned
        if (muzzleFlashPrefab == null)
            muzzleFlashPrefab = CreateDefaultMuzzleFlash();
        
        if (hitEffectPrefab == null)
            hitEffectPrefab = CreateDefaultHitEffect();
        
        if (explosionPrefab == null)
            explosionPrefab = CreateDefaultExplosion();
        
        // Initialize pools
        if (muzzleFlashPrefab != null)
        {
            ParticleSystem muzzlePS = muzzleFlashPrefab.GetComponent<ParticleSystem>();
            if (muzzlePS != null)
                muzzleFlashPool = new ObjectPool<ParticleSystem>(muzzlePS, effectPoolSize, transform);
        }
        
        if (hitEffectPrefab != null)
        {
            ParticleSystem hitPS = hitEffectPrefab.GetComponent<ParticleSystem>();
            if (hitPS != null)
                hitEffectPool = new ObjectPool<ParticleSystem>(hitPS, effectPoolSize, transform);
        }
        
        if (explosionPrefab != null)
        {
            ParticleSystem explosionPS = explosionPrefab.GetComponent<ParticleSystem>();
            if (explosionPS != null)
                explosionPool = new ObjectPool<ParticleSystem>(explosionPS, effectPoolSize, transform);
        }
    }
    
    GameObject CreateDefaultMuzzleFlash()
    {
        GameObject muzzle = new GameObject("MuzzleFlash");
        ParticleSystem ps = muzzle.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startLifetime = 0.1f;
        main.startSpeed = 5f;
        main.startSize = 0.2f;
        main.startColor = Color.yellow;
        main.maxParticles = 10;
        
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 5)
        });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        
        return muzzle;
    }
    
    GameObject CreateDefaultHitEffect()
    {
        GameObject hit = new GameObject("HitEffect");
        ParticleSystem ps = hit.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 3f;
        main.startSize = 0.1f;
        main.startColor = Color.white;
        main.maxParticles = 15;
        
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 10)
        });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
        
        return hit;
    }
    
    GameObject CreateDefaultExplosion()
    {
        GameObject explosion = new GameObject("Explosion");
        ParticleSystem ps = explosion.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 4f;
        main.startSize = 0.3f;
        main.startColor = Color.red;
        main.maxParticles = 30;
        
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 20)
        });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        return explosion;
    }
    
    public void CreateMuzzleFlash(Vector3 position, Vector3 direction, BulletType weaponType)
    {
        if (muzzleFlashPool == null) return;
        
        ParticleSystem muzzleFlash = muzzleFlashPool.Get();
        if (muzzleFlash != null)
        {
            muzzleFlash.transform.position = position;
            muzzleFlash.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            
            // Customize based on weapon type
            var main = muzzleFlash.main;
            main.startColor = GetWeaponEffectColor(weaponType);
            
            muzzleFlash.Play();
            StartCoroutine(ReturnToPoolAfterDelay(muzzleFlash, muzzleFlashPool, 0.5f));
        }
    }
    
    public void CreateHitEffect(Vector3 position, BulletType weaponType = BulletType.Normal)
    {
        if (hitEffectPool == null) return;
        
        ParticleSystem hitEffect = hitEffectPool.Get();
        if (hitEffect != null)
        {
            hitEffect.transform.position = position;
            
            // Customize based on weapon type
            var main = hitEffect.main;
            main.startColor = GetWeaponEffectColor(weaponType);
            
            hitEffect.Play();
            StartCoroutine(ReturnToPoolAfterDelay(hitEffect, hitEffectPool, 0.8f));
        }
    }
    
    public void CreateExplosion(Vector3 position, float intensity = 1f)
    {
        if (explosionPool == null) return;
        
        ParticleSystem explosion = explosionPool.Get();
        if (explosion != null)
        {
            explosion.transform.position = position;
            
            // Scale explosion based on intensity
            var main = explosion.main;
            main.startSize = 0.3f * intensity;
            main.maxParticles = Mathf.RoundToInt(30 * intensity);
            
            explosion.Play();
            StartCoroutine(ReturnToPoolAfterDelay(explosion, explosionPool, 1f));
        }
        
        // Screen shake for big explosions
        if (intensity > 0.7f)
        {
            TriggerScreenShake(intensity);
        }
    }
    
    public void CreateLaserImpact(Vector3 position, Vector3 direction)
    {
        // Create continuous laser impact effect
        GameObject impact = Instantiate(laserImpactPrefab != null ? laserImpactPrefab : hitEffectPrefab);
        impact.transform.position = position;
        impact.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        
        ParticleSystem ps = impact.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = Color.white;
            ps.Play();
        }
        
        Destroy(impact, 0.2f);
    }
    
    public void CreatePlasmaImpact(Vector3 position)
    {
        // Create electric plasma impact effect
        GameObject impact = Instantiate(plasmaImpactPrefab != null ? plasmaImpactPrefab : hitEffectPrefab);
        impact.transform.position = position;
        
        ParticleSystem ps = impact.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = Color.magenta;
            ps.Play();
        }
        
        Destroy(impact, 0.3f);
    }
    
    Color GetWeaponEffectColor(BulletType weaponType)
    {
        switch (weaponType)
        {
            case BulletType.Ion: return Color.cyan;
            case BulletType.Neutron: return Color.green;
            case BulletType.Hyper: return Color.red;
            case BulletType.Vulcan: return Color.yellow;
            case BulletType.Plasma: return Color.magenta;
            case BulletType.Laser: return Color.white;
            default: return Color.white;
        }
    }
    
    public void TriggerScreenShake(float intensity = 1f)
    {
        if (mainCamera == null) return;
        
        if (screenShakeCoroutine != null)
        {
            StopCoroutine(screenShakeCoroutine);
        }
        
        screenShakeCoroutine = StartCoroutine(ScreenShakeCoroutine(intensity));
    }
    
    IEnumerator ScreenShakeCoroutine(float intensity)
    {
        float elapsed = 0f;
        
        while (elapsed < screenShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * screenShakeIntensity * intensity;
            float y = Random.Range(-1f, 1f) * screenShakeIntensity * intensity;
            
            mainCamera.transform.position = cameraOriginalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.position = cameraOriginalPosition;
        screenShakeCoroutine = null;
    }
    
    IEnumerator ReturnToPoolAfterDelay<T>(T obj, ObjectPool<T> pool, float delay) where T : Component
    {
        yield return new WaitForSeconds(delay);
        
        if (obj != null && pool != null)
        {
            pool.Return(obj);
        }
    }
    
    // Public methods for easy effect triggering
    public void WeaponFired(Vector3 position, Vector3 direction, BulletType weaponType)
    {
        CreateMuzzleFlash(position, direction, weaponType);
    }
    
    public void BulletHit(Vector3 position, BulletType weaponType)
    {
        CreateHitEffect(position, weaponType);
    }
    
    public void EnemyDestroyed(Vector3 position)
    {
        CreateExplosion(position, 1f);
        TriggerScreenShake(0.5f);
    }
    
    public void PlayerDestroyed(Vector3 position)
    {
        CreateExplosion(position, 1.5f);
        TriggerScreenShake(1f);
    }
    
    private void OnDestroy()
    {
        if (screenShakeCoroutine != null)
        {
            StopCoroutine(screenShakeCoroutine);
        }
        
        if (mainCamera != null)
        {
            mainCamera.transform.position = cameraOriginalPosition;
        }
    }
}