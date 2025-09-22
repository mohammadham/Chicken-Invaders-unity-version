using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plasma weapon system - electric beam with particle effects
/// Equivalent to Plasma/Electricity functionality from C++ Weapon struct
/// </summary>
public class PlasmaWeapon : MonoBehaviour
{
    [Header("Plasma Components")]
    public LineRenderer plasmaBeam;
    public ParticleSystem plasmaEffect;
    public ParticleSystem electricArcs;
    
    [Header("Plasma Settings")]
    public LayerMask enemyLayerMask = -1;
    public float maxDistance = 8f;
    public float beamWidth = 0.2f;
    public float damagePerSecond = 15f;
    public int arcSegments = 5; // Number of segments in the electric arc
    
    private PlayerController ownerPlayer;
    private WeaponData weaponData;
    private bool isFiring = false;
    private bool isActive = false;
    private float damageTimer = 0f;
    private float damageInterval = 0.08f; // Apply damage every 80ms (faster than laser)
    
    // Plasma targeting
    private EnemyController targetEnemy = null;
    private bool foundTarget = false;
    
    // Electric arc effect
    private Vector3[] arcPoints;
    private float arcUpdateTimer = 0f;
    private float arcUpdateInterval = 0.05f; // Update arc shape every 50ms
    
    // Visual effects
    private Color plasmaColor;
    private float intensityTimer = 0f;
    private bool soundLoopStarted = false;
    
    public void Initialize(PlayerController player, WeaponData weapon)
    {
        ownerPlayer = player;
        weaponData = weapon;
        
        SetupPlasmaBeam();
        SetupEffects();
        
        plasmaColor = new Color(1f, 0f, 1f, 0.8f); // Magenta with transparency
        damagePerSecond = weaponData.damage * 7f; // Plasma does high DPS
        
        // Initialize arc points array
        arcPoints = new Vector3[arcSegments + 1];
    }
    
    void SetupPlasmaBeam()
    {
        if (plasmaBeam == null)
        {
            GameObject beamObj = new GameObject("PlasmaBeam");
            beamObj.transform.SetParent(transform);
            plasmaBeam = beamObj.AddComponent<LineRenderer>();
        }
        
        // Configure LineRenderer for electric arc effect
        plasmaBeam.material = CreatePlasmaMaterial();
        plasmaBeam.startWidth = beamWidth;
        plasmaBeam.endWidth = beamWidth * 0.3f;
        plasmaBeam.positionCount = arcSegments + 1;
        plasmaBeam.useWorldSpace = true;
        plasmaBeam.enabled = false;
        
        // Electric arc properties
        plasmaBeam.textureMode = LineTextureMode.Tile;
        plasmaBeam.sortingOrder = 4;
    }
    
    Material CreatePlasmaMaterial()
    {
        Material plasmaMat = new Material(Shader.Find("Sprites/Default"));
        plasmaMat.color = plasmaColor;
        
        // Electric glow effect
        plasmaMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        plasmaMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        plasmaMat.SetInt("_ZWrite", 0);
        plasmaMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        plasmaMat.renderQueue = 3000;
        
        return plasmaMat;
    }
    
    void SetupEffects()
    {
        // Setup main plasma effect
        if (plasmaEffect == null)
        {
            GameObject effectObj = new GameObject("PlasmaEffect");
            effectObj.transform.SetParent(transform);
            plasmaEffect = effectObj.AddComponent<ParticleSystem>();
            
            var main = plasmaEffect.main;
            main.startColor = plasmaColor;
            main.startLifetime = 0.3f;
            main.startSpeed = 3f;
            main.maxParticles = 50;
            
            var emission = plasmaEffect.emission;
            emission.rateOverTime = 150f;
            
            var shape = plasmaEffect.shape;
            shape.shapeType = ParticleSystemShapeType.Line;
            shape.scale = new Vector3(maxDistance, 0.1f, 0.1f);
        }
        
        // Setup electric arcs effect
        if (electricArcs == null)
        {
            GameObject arcsObj = new GameObject("ElectricArcs");
            arcsObj.transform.SetParent(transform);
            electricArcs = arcsObj.AddComponent<ParticleSystem>();
            
            var main = electricArcs.main;
            main.startColor = Color.cyan;
            main.startLifetime = 0.1f;
            main.startSpeed = 1f;
            main.maxParticles = 20;
            
            var emission = electricArcs.emission;
            emission.rateOverTime = 200f;
            
            var velocityOverLifetime = electricArcs.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        }
    }
    
    public void FirePlasma(Vector3 startPosition, Vector3 direction)
    {
        if (!isActive)
        {
            StartPlasma(startPosition, direction);
        }
        
        UpdatePlasma(startPosition, direction);
    }
    
    void StartPlasma(Vector3 startPosition, Vector3 direction)
    {
        isActive = true;
        isFiring = true;
        
        plasmaBeam.enabled = true;
        
        if (plasmaEffect != null)
            plasmaEffect.Play();
        
        if (electricArcs != null)
            electricArcs.Play();
        
        // Start looped plasma sound (equivalent to C++ looped sound logic)
        if (!soundLoopStarted && AudioManager.Instance != null)
        {
            // AudioManager should handle looped plasma sound
            AudioManager.Instance.PlayWeaponSound(weaponData.weaponType);
            soundLoopStarted = true;
        }
    }
    
    void UpdatePlasma(Vector3 startPosition, Vector3 direction)
    {
        if (!isActive) return;
        
        // Find target with raycast
        Vector3 endPosition = PerformPlasmaRaycast(startPosition, direction);
        
        // Update electric arc shape
        UpdateElectricArc(startPosition, endPosition);
        
        // Update particle effects
        UpdateParticleEffects(startPosition, endPosition);
        
        // Apply damage over time
        ApplyPlasmaDamage();
        
        // Update visual intensity
        UpdatePlasmaIntensity();
    }
    
    Vector3 PerformPlasmaRaycast(Vector3 startPosition, Vector3 direction)
    {
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
        
        return startPosition + direction * maxDistance;
    }
    
    void UpdateElectricArc(Vector3 startPos, Vector3 endPos)
    {
        arcUpdateTimer += Time.deltaTime;
        
        if (arcUpdateTimer >= arcUpdateInterval)
        {
            arcUpdateTimer = 0f;
            
            // Generate electric arc points with random deviation
            Vector3 direction = (endPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, endPos);
            
            arcPoints[0] = startPos;
            arcPoints[arcSegments] = endPos;
            
            for (int i = 1; i < arcSegments; i++)
            {
                float t = (float)i / arcSegments;
                Vector3 basePoint = Vector3.Lerp(startPos, endPos, t);
                
                // Add random perpendicular deviation for electric effect
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
                float deviation = Random.Range(-0.3f, 0.3f) * (1f - Mathf.Abs(t - 0.5f) * 2f); // More deviation in the middle
                
                arcPoints[i] = basePoint + perpendicular * deviation;
            }
            
            // Update LineRenderer
            plasmaBeam.positionCount = arcSegments + 1;
            plasmaBeam.SetPositions(arcPoints);
        }
    }
    
    void UpdateParticleEffects(Vector3 startPos, Vector3 endPos)
    {
        // Update plasma effect to follow the beam
        if (plasmaEffect != null)
        {
            plasmaEffect.transform.position = Vector3.Lerp(startPos, endPos, 0.5f);
            
            var shape = plasmaEffect.shape;
            shape.scale = new Vector3(Vector3.Distance(startPos, endPos), 0.2f, 0.2f);
        }
        
        // Update electric arcs to the impact point
        if (electricArcs != null && foundTarget)
        {
            electricArcs.transform.position = endPos;
        }
    }
    
    void ApplyPlasmaDamage()
    {
        if (!foundTarget || targetEnemy == null) return;
        
        damageTimer += Time.deltaTime;
        
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            
            float damage = GetPlasmaDamage();
            targetEnemy.TakeDamage(Mathf.RoundToInt(damage * damageInterval));
        }
    }
    
    float GetPlasmaDamage()
    {
        // Plasma damage scaling (equivalent to C++ Electericity damage logic)
        switch (weaponData.level)
        {
            case 1: return damagePerSecond * 0.4f; // Lower base damage than laser
            case 2: return damagePerSecond * 1.4f; // But scales better
            case 3: return damagePerSecond * 2f;
            case 4: return damagePerSecond * 3f; // Highest DPS at max level
            default: return damagePerSecond * 0.4f;
        }
    }
    
    void UpdatePlasmaIntensity()
    {
        // Create pulsing intensity effect
        intensityTimer += Time.deltaTime;
        float intensity = 0.8f + Mathf.Sin(intensityTimer * 10f) * 0.2f;
        
        Color currentColor = plasmaColor;
        currentColor.a = intensity;
        
        if (plasmaBeam != null && plasmaBeam.material != null)
        {
            plasmaBeam.material.color = currentColor;
        }
    }
    
    public void StopPlasma()
    {
        isActive = false;
        isFiring = false;
        soundLoopStarted = false;
        
        if (plasmaBeam != null)
            plasmaBeam.enabled = false;
        
        if (plasmaEffect != null)
            plasmaEffect.Stop();
        
        if (electricArcs != null)
            electricArcs.Stop();
        
        foundTarget = false;
        targetEnemy = null;
    }
    
    private void Update()
    {
        // Auto-stop plasma if not being fired
        if (isActive && !isFiring)
        {
            StopPlasma();
        }
        
        // Reset firing flag each frame
        isFiring = false;
    }
    
    // Public properties
    public bool IsActive => isActive;
    public bool HasTarget => foundTarget;
    public EnemyController CurrentTarget => targetEnemy;
    
    private void OnDestroy()
    {
        StopPlasma();
    }
}