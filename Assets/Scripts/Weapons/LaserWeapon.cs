using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Laser weapon system - continuous beam with real-time collision detection
/// Equivalent to Laser functionality from C++ Weapon struct
/// </summary>
public class LaserWeapon : MonoBehaviour
{
    [Header("Laser Components")]
    public LineRenderer laserBeam;
    public ParticleSystem laserImpactEffect;
    public ParticleSystem laserChargeEffect;
    
    [Header("Laser Settings")]
    public LayerMask enemyLayerMask = -1;
    public float maxDistance = 10f;
    public float beamWidth = 0.1f;
    public float damagePerSecond = 10f;
    
    private PlayerController ownerPlayer;
    private WeaponData weaponData;
    private bool isFiring = false;
    private bool isActive = false;
    private float damageTimer = 0f;
    private float damageInterval = 0.1f; // Apply damage every 100ms
    
    // Laser beam targeting (equivalent to C++ laser targeting logic)
    private EnemyController targetEnemy = null;
    private bool foundTarget = false;
    private int targetIndex = 0;
    
    // Visual effects
    private Color laserColor;
    private float flickerTimer = 0f;
    private bool flickerState = false;
    
    public void Initialize(PlayerController player, WeaponData weapon)
    {
        ownerPlayer = player;
        weaponData = weapon;
        
        SetupLaserBeam();
        SetupEffects();
        
        // Set laser properties based on weapon type
        if (weaponData.weaponType == BulletType.Laser)
        {
            laserColor = Color.white;
            maxDistance = 10f;
        }
        else if (weaponData.weaponType == BulletType.Plasma)
        {
            laserColor = Color.magenta;
            maxDistance = 8f;
        }
        
        damagePerSecond = weaponData.damage * 5f; // Laser does more DPS than single bullets
    }
    
    void SetupLaserBeam()
    {
        if (laserBeam == null)
        {
            GameObject beamObj = new GameObject("LaserBeam");
            beamObj.transform.SetParent(transform);
            laserBeam = beamObj.AddComponent<LineRenderer>();
        }
        
        // Configure LineRenderer
        laserBeam.material = CreateLaserMaterial();
        laserBeam.startWidth = beamWidth;
        laserBeam.endWidth = beamWidth * 0.5f;
        laserBeam.positionCount = 2;
        laserBeam.useWorldSpace = true;
        laserBeam.enabled = false;
        
        // Set sorting layer to appear above other objects
        laserBeam.sortingOrder = 5;
    }
    
    Material CreateLaserMaterial()
    {
        // Create a simple laser material
        Material laserMat = new Material(Shader.Find("Sprites/Default"));
        laserMat.color = laserColor;
        
        // Make it additive/glow
        laserMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        laserMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        laserMat.SetInt("_ZWrite", 0);
        laserMat.DisableKeyword("_ALPHATEST_ON");
        laserMat.DisableKeyword("_ALPHABLEND_ON");
        laserMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        laserMat.renderQueue = 3000;
        
        return laserMat;
    }
    
    void SetupEffects()
    {
        // Setup impact effect
        if (laserImpactEffect == null)
        {
            GameObject impactObj = new GameObject("LaserImpactEffect");
            impactObj.transform.SetParent(transform);
            laserImpactEffect = impactObj.AddComponent<ParticleSystem>();
            
            var main = laserImpactEffect.main;
            main.startColor = laserColor;
            main.startLifetime = 0.2f;
            main.startSpeed = 2f;
            main.maxParticles = 20;
            
            var emission = laserImpactEffect.emission;
            emission.rateOverTime = 100f;
            
            var shape = laserImpactEffect.shape;
            shape.shapeType = ParticleSystemShapeType.Point;
        }
        
        // Setup charge effect
        if (laserChargeEffect == null)
        {
            GameObject chargeObj = new GameObject("LaserChargeEffect");
            chargeObj.transform.SetParent(transform);
            laserChargeEffect = chargeObj.AddComponent<ParticleSystem>();
            
            var main = laserChargeEffect.main;
            main.startColor = laserColor;
            main.startLifetime = 0.5f;
            main.startSpeed = 1f;
            main.maxParticles = 10;
        }
    }
    
    public void FireLaser(Vector3 startPosition, Vector3 direction)
    {
        if (!isActive)
        {
            StartLaser(startPosition, direction);
        }
        
        UpdateLaser(startPosition, direction);
    }
    
    void StartLaser(Vector3 startPosition, Vector3 direction)
    {
        isActive = true;
        isFiring = true;
        
        laserBeam.enabled = true;
        
        if (laserChargeEffect != null)
            laserChargeEffect.Play();
        
        // Play laser sound (looped)
        if (AudioManager.Instance != null)
        {
            // Laser sound should loop while firing
            AudioManager.Instance.PlayWeaponSound(weaponData.weaponType);
        }
    }
    
    void UpdateLaser(Vector3 startPosition, Vector3 direction)
    {
        if (!isActive) return;
        
        // Perform raycast to find hit point and target
        Vector3 endPosition = PerformLaserRaycast(startPosition, direction);
        
        // Update laser beam visual
        laserBeam.SetPosition(0, startPosition);
        laserBeam.SetPosition(1, endPosition);
        
        // Update impact effect position
        if (laserImpactEffect != null)
        {
            laserImpactEffect.transform.position = endPosition;
            if (!laserImpactEffect.isPlaying)
                laserImpactEffect.Play();
        }
        
        // Handle damage over time
        ApplyLaserDamage();
        
        // Handle laser flickering effect (equivalent to C++ laser flickering)
        HandleLaserFlicker();
    }
    
    Vector3 PerformLaserRaycast(Vector3 startPosition, Vector3 direction)
    {
        // Equivalent to Laser_Check() function from C++
        RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, maxDistance, enemyLayerMask);
        
        foundTarget = false;
        targetEnemy = null;
        
        if (hit.collider != null)
        {
            EnemyController enemy = hit.collider.GetComponent<EnemyController>();
            if (enemy != null && !enemy.isDead)
            {
                foundTarget = true;
                targetEnemy = enemy;
                return hit.point;
            }
        }
        
        // No target found, laser goes to max distance
        return startPosition + direction * maxDistance;
    }
    
    void ApplyLaserDamage()
    {
        if (!foundTarget || targetEnemy == null) return;
        
        damageTimer += Time.deltaTime;
        
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            
            // Calculate damage based on level (equivalent to C++ laser damage logic)
            float damage = GetLaserDamage();
            targetEnemy.TakeDamage(Mathf.RoundToInt(damage * damageInterval));
        }
    }
    
    float GetLaserDamage()
    {
        // Damage scaling based on weapon level (from C++)
        switch (weaponData.level)
        {
            case 1: return damagePerSecond; // Base damage
            case 2: return damagePerSecond * 1.5f; // +50% damage
            case 3: return damagePerSecond * 2f; // +100% damage
            case 4: return damagePerSecond * 2.5f; // +150% damage
            default: return damagePerSecond;
        }
    }
    
    void HandleLaserFlicker()
    {
        // Equivalent to laser flickering logic from C++ (for electricity type)
        if (weaponData.weaponType == BulletType.Plasma)
        {
            flickerTimer += Time.deltaTime;
            
            if (flickerTimer >= 0.1f) // Flicker every 100ms
            {
                flickerState = !flickerState;
                flickerTimer = 0f;
                
                // Slightly rotate the laser for electric effect
                if (flickerState)
                {
                    transform.Rotate(0, 0, Random.Range(-3f, 3f));
                }
                else
                {
                    transform.rotation = Quaternion.identity;
                }
                
                // Change beam intensity
                Color currentColor = laserColor;
                currentColor.a = flickerState ? 1f : 0.7f;
                laserBeam.material.color = currentColor;
            }
        }
    }
    
    public void StopLaser()
    {
        isActive = false;
        isFiring = false;
        
        if (laserBeam != null)
            laserBeam.enabled = false;
        
        if (laserImpactEffect != null)
            laserImpactEffect.Stop();
        
        if (laserChargeEffect != null)
            laserChargeEffect.Stop();
        
        // Reset rotation
        transform.rotation = Quaternion.identity;
        
        foundTarget = false;
        targetEnemy = null;
    }
    
    private void Update()
    {
        // Auto-stop laser if not being fired
        if (isActive && !isFiring)
        {
            StopLaser();
        }
        
        // Reset firing flag each frame (will be set by weapon manager if still firing)
        isFiring = false;
    }
    
    // Public properties
    public bool IsActive => isActive;
    public bool HasTarget => foundTarget;
    public EnemyController CurrentTarget => targetEnemy;
    
    private void OnDestroy()
    {
        StopLaser();
    }
}