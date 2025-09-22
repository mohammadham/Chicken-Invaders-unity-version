using UnityEngine;

/// <summary>
/// ScriptableObject for weapon configuration data
/// Allows designers to configure weapons in the Unity Inspector
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Chicken Invaders/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Basic Properties")]
    public BulletType weaponType;
    public string weaponName;
    public int baseDamage = 30;
    
    [Header("Fire Rate & Timing")]
    public float baseFireRate = 0.5f; // Time between shots
    public float bulletSpeed = 7f;
    
    [Header("Overheat System")]
    public float overHeatMax = 20f;
    public float coolingRate = 0.002f;
    public float heatPerShot = 1f;
    
    [Header("Visual")]
    public Sprite weaponSprite;
    public Sprite bulletSprite;
    public Color bulletColor = Color.white;
    public Vector2 bulletSize = new Vector2(0.8f, 2f);
    
    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip overheatedSound;
    public bool loopSound = false; // For continuous weapons like laser/plasma
    
    [Header("Level-based Angles (from C++)")]
    [Tooltip("Weapon firing angles for different levels")]
    public float rightAngle2 = 80f;
    public float leftAngle2 = 100f;
    public float rightAngle3 = 70f;
    public float leftAngle3 = 110f;
    public float rightAngle4 = 85f;
    public float leftAngle4 = 95f;
    
    [Header("Special Properties")]
    [Tooltip("For laser/plasma weapons")]
    public bool isContinuous = false;
    public float continuousDamageRate = 10f; // Damage per second
    public float beamWidth = 0.5f;
    public float maxBeamDistance = 10f;
    
    [Header("Particle Effects")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public GameObject trailEffectPrefab;
    
    [Header("Upgrade Progression")]
    [Tooltip("How damage scales with weapon level")]
    public AnimationCurve damageProgression = AnimationCurve.Linear(1, 1, 4, 2);
    
    [Tooltip("How fire rate changes with weapon level (lower = faster)")]
    public AnimationCurve fireRateProgression = AnimationCurve.Linear(1, 1, 4, 0.7f);
    
    // Calculated properties
    public int GetDamageForLevel(int level)
    {
        level = Mathf.Clamp(level, 1, 4);
        return Mathf.RoundToInt(baseDamage * damageProgression.Evaluate(level));
    }
    
    public float GetFireRateForLevel(int level)
    {
        level = Mathf.Clamp(level, 1, 4);
        return baseFireRate * fireRateProgression.Evaluate(level);
    }
    
    public string GetLevelDescription(int level)
    {
        level = Mathf.Clamp(level, 1, 4);
        
        switch (level)
        {
            case 1: return "Single shot";
            case 2: return "Double shot";
            case 3: return "Triple shot";
            case 4: return "Quad shot";
            default: return "Unknown";
        }
    }
    
    // Validation in editor
    private void OnValidate()
    {
        baseDamage = Mathf.Max(1, baseDamage);
        baseFireRate = Mathf.Max(0.01f, baseFireRate);
        overHeatMax = Mathf.Max(1f, overHeatMax);
        coolingRate = Mathf.Max(0.001f, coolingRate);
        bulletSpeed = Mathf.Max(0.1f, bulletSpeed);
        
        if (bulletSize.x <= 0) bulletSize.x = 0.1f;
        if (bulletSize.y <= 0) bulletSize.y = 0.1f;
    }
}