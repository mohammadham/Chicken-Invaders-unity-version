using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complete Weapon management system - equivalent to Weapon struct and weapon handling from C++
/// Manages all 6 weapon types with full upgrade system, overheat, laser beams, plasma effects
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Prefabs")]
    public GameObject[] bulletPrefabs;
    public GameObject laserBeamPrefab;
    public GameObject plasmaBeamPrefab;
    
    [Header("Weapon Configurations")]
    public WeaponDataSO[] weaponConfigurations;
    
    [Header("Laser System")]
    public LayerMask enemyLayerMask = -1;
    public float laserMaxDistance = 10f;
    public float laserDamageRate = 10f; // damage per second
    
    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public GameObject[] weaponTrailPrefabs;
    
    private Dictionary<BulletType, WeaponDataSO> weaponDatabase;
    private Dictionary<int, List<GameObject>> activeLasers; // Player ID -> Laser beams
    private Dictionary<int, LaserWeapon> playerLasers;
    private Dictionary<int, PlasmaWeapon> playerPlasmas;
    
    public static WeaponManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        InitializeWeaponSystem();
    }
    
    void InitializeWeaponSystem()
    {
        // Initialize weapon database
        weaponDatabase = new Dictionary<BulletType, WeaponDataSO>();
        activeLasers = new Dictionary<int, List<GameObject>>();
        playerLasers = new Dictionary<int, LaserWeapon>();
        playerPlasmas = new Dictionary<int, PlasmaWeapon>();
        
        // Populate weapon database
        foreach (var weaponConfig in weaponConfigurations)
        {
            if (weaponConfig != null)
            {
                weaponDatabase[weaponConfig.weaponType] = weaponConfig;
            }
        }
        
        // Create default weapons if configurations are missing
        CreateDefaultWeaponConfigurations();
        
        Debug.Log($"WeaponManager initialized with {weaponDatabase.Count} weapon types");
    }
    
    void CreateDefaultWeaponConfigurations()
    {
        // Create default weapon configurations if not assigned in inspector
        if (!weaponDatabase.ContainsKey(BulletType.Ion))
        {
            weaponDatabase[BulletType.Ion] = CreateDefaultWeaponData(BulletType.Ion, "Ion Blaster", 30, 0.4f, 20f, 0.002f);
        }
        
        if (!weaponDatabase.ContainsKey(BulletType.Neutron))
        {
            weaponDatabase[BulletType.Neutron] = CreateDefaultWeaponData(BulletType.Neutron, "Neutron Gun", 30, 0.5f, 20f, 0.002f);
        }
        
        if (!weaponDatabase.ContainsKey(BulletType.Hyper))
        {
            weaponDatabase[BulletType.Hyper] = CreateDefaultWeaponData(BulletType.Hyper, "Hyper Gun", 30, 0.5f, 20f, 0.02f);
        }
        
        if (!weaponDatabase.ContainsKey(BulletType.Vulcan))
        {
            weaponDatabase[BulletType.Vulcan] = CreateDefaultWeaponData(BulletType.Vulcan, "Vulcan Chaingun", 30, 0.5f, 20f, 0.02f);
        }
        
        if (!weaponDatabase.ContainsKey(BulletType.Plasma))
        {
            weaponDatabase[BulletType.Plasma] = CreateDefaultWeaponData(BulletType.Plasma, "Plasma Rifle", 5, 0.01f, 70f, 0.02f);
        }
        
        if (!weaponDatabase.ContainsKey(BulletType.Laser))
        {
            weaponDatabase[BulletType.Laser] = CreateDefaultWeaponData(BulletType.Laser, "Lightning Fryer", 5, 0.01f, 70f, 0.02f);
        }
    }
    
    WeaponDataSO CreateDefaultWeaponData(BulletType type, string name, int damage, float fireRate, float overheat, float cooling)
    {
        WeaponDataSO data = ScriptableObject.CreateInstance<WeaponDataSO>();
        data.weaponType = type;
        data.weaponName = name;
        data.baseDamage = damage;
        data.baseFireRate = fireRate;
        data.overHeatMax = overheat;
        data.coolingRate = cooling;
        data.bulletColor = GetDefaultBulletColor(type);
        data.bulletSize = GetDefaultBulletSize(type);
        return data;
    }
    
    Color GetDefaultBulletColor(BulletType type)
    {
        switch (type)
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
    
    Vector2 GetDefaultBulletSize(BulletType type)
    {
        switch (type)
        {
            case BulletType.Ion:
            case BulletType.Neutron:
            case BulletType.Hyper:
                return new Vector2(0.8f, 2f);
            case BulletType.Vulcan:
                return new Vector2(0.6f, 1.5f);
            case BulletType.Plasma:
            case BulletType.Laser:
                return new Vector2(1f, 2.5f);
            default:
                return new Vector2(0.8f, 2f);
        }
    }
    
    public void FireWeapon(PlayerController player, WeaponData weaponData, Vector3 firePosition, Vector3 fireDirection)
    {
        if (player == null || weaponData == null) return;
        
        BulletType weaponType = weaponData.weaponType;
        
        // Handle different weapon types
        switch (weaponType)
        {
            case BulletType.Ion:
            case BulletType.Neutron:
            case BulletType.Hyper:
            case BulletType.Vulcan:
                FireBulletWeapon(player, weaponData, firePosition, fireDirection);
                break;
                
            case BulletType.Laser:
                FireLaserWeapon(player, weaponData, firePosition, fireDirection);
                break;
                
            case BulletType.Plasma:
                FirePlasmaWeapon(player, weaponData, firePosition, fireDirection);
                break;
        }
        
        // Create muzzle flash effect
        CreateMuzzleFlash(firePosition, weaponType);
        
        // Play weapon sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponSound(weaponType);
        }
    }
    
    void FireBulletWeapon(PlayerController player, WeaponData weaponData, Vector3 firePosition, Vector3 fireDirection)
    {
        if (BulletManager.Instance == null) return;
        
        WeaponDataSO config = GetWeaponConfig(weaponData.weaponType);
        if (config == null) return;
        
        int level = weaponData.level;
        int damage = weaponData.damage;
        float speed = config.bulletSpeed;
        
        // Fire pattern based on weapon level (equivalent to C++ weapon firing logic)
        switch (level)
        {
            case 1:
                // Single bullet
                BulletManager.Instance.SpawnBullet(
                    firePosition, 
                    fireDirection, 
                    speed, 
                    damage, 
                    true, 
                    weaponData.weaponType
                );
                break;
                
            case 2:
                // Two bullets at angles (equivalent to C++ Level 2 logic)
                float angle2L = weaponData.leftAngle2;
                float angle2R = weaponData.rightAngle2;
                
                Vector3 leftDir = Quaternion.Euler(0, 0, angle2L - 90f) * fireDirection;
                Vector3 rightDir = Quaternion.Euler(0, 0, -(angle2R - 90f)) * fireDirection;
                
                BulletManager.Instance.SpawnBullet(firePosition + Vector3.left * 0.2f, leftDir, speed, damage, true, weaponData.weaponType);
                BulletManager.Instance.SpawnBullet(firePosition + Vector3.right * 0.2f, rightDir, speed, damage, true, weaponData.weaponType);
                break;
                
            case 3:
                // Three bullets (equivalent to C++ Level 3 logic)
                float angle3L = weaponData.leftAngle3;
                float angle3R = weaponData.rightAngle3;
                
                Vector3 leftDir3 = Quaternion.Euler(0, 0, angle3L - 90f) * fireDirection;
                Vector3 rightDir3 = Quaternion.Euler(0, 0, -(angle3R - 90f)) * fireDirection;
                
                BulletManager.Instance.SpawnBullet(firePosition, fireDirection, speed, damage, true, weaponData.weaponType);
                BulletManager.Instance.SpawnBullet(firePosition + Vector3.left * 0.2f, leftDir3, speed, damage, true, weaponData.weaponType);
                BulletManager.Instance.SpawnBullet(firePosition + Vector3.right * 0.2f, rightDir3, speed, damage, true, weaponData.weaponType);
                break;
                
            case 4:
            default:
                // Four bullets spread (equivalent to C++ Level 4 logic)
                float angle4L = weaponData.leftAngle4;
                float angle4R = weaponData.rightAngle4;
                
                Vector3 leftDir4_1 = Quaternion.Euler(0, 0, angle4L - 90f) * fireDirection;
                Vector3 rightDir4_1 = Quaternion.Euler(0, 0, -(angle4R - 90f)) * fireDirection;
                Vector3 leftDir4_2 = Quaternion.Euler(0, 0, (angle4L + 5f) - 90f) * fireDirection;
                Vector3 rightDir4_2 = Quaternion.Euler(0, 0, -((angle4R + 5f) - 90f)) * fireDirection;
                
                BulletManager.Instance.SpawnBullet(firePosition + Vector3.left * 0.3f, leftDir4_1, speed, damage, true, weaponData.weaponType);
                BulletManager.Instance.SpawnBullet(firePosition + Vector3.left * 0.1f, leftDir4_2, speed, damage, true, weaponData.weaponType);
                BulletManager.Instance.SpawnBullet(firePosition + Vector3.right * 0.1f, rightDir4_2, speed, damage, true, weaponData.weaponType);
                BulletManager.Instance.SpawnBullet(firePosition + Vector3.right * 0.3f, rightDir4_1, speed, damage, true, weaponData.weaponType);
                break;
        }
    }
    
    void FireLaserWeapon(PlayerController player, WeaponData weaponData, Vector3 firePosition, Vector3 fireDirection)
    {
        // Get or create laser weapon component for this player
        if (!playerLasers.ContainsKey(player.playerIndex))
        {
            GameObject laserObj = new GameObject($"LaserWeapon_Player{player.playerIndex}");
            laserObj.transform.SetParent(player.transform);
            LaserWeapon laserWeapon = laserObj.AddComponent<LaserWeapon>();
            laserWeapon.Initialize(player, weaponData);
            playerLasers[player.playerIndex] = laserWeapon;
        }
        
        LaserWeapon laser = playerLasers[player.playerIndex];
        laser.FireLaser(firePosition, fireDirection);
    }
    
    void FirePlasmaWeapon(PlayerController player, WeaponData weaponData, Vector3 firePosition, Vector3 fireDirection)
    {
        // Get or create plasma weapon component for this player
        if (!playerPlasmas.ContainsKey(player.playerIndex))
        {
            GameObject plasmaObj = new GameObject($"PlasmaWeapon_Player{player.playerIndex}");
            plasmaObj.transform.SetParent(player.transform);
            PlasmaWeapon plasmaWeapon = plasmaObj.AddComponent<PlasmaWeapon>();
            plasmaWeapon.Initialize(player, weaponData);
            playerPlasmas[player.playerIndex] = plasmaWeapon;
        }
        
        PlasmaWeapon plasma = playerPlasmas[player.playerIndex];
        plasma.FirePlasma(firePosition, fireDirection);
    }
    
    void CreateMuzzleFlash(Vector3 position, BulletType weaponType)
    {
        if (muzzleFlashPrefab == null) return;
        
        GameObject flash = Instantiate(muzzleFlashPrefab, position, Quaternion.identity);
        
        // Customize muzzle flash based on weapon type
        ParticleSystem particles = flash.GetComponent<ParticleSystem>();
        if (particles != null)
        {
            var main = particles.main;
            main.startColor = GetDefaultBulletColor(weaponType);
        }
        
        // Auto-destroy after a short time
        Destroy(flash, 0.5f);
    }
    
    public void StopContinuousWeapons(int playerIndex)
    {
        // Stop laser
        if (playerLasers.ContainsKey(playerIndex))
        {
            playerLasers[playerIndex].StopLaser();
        }
        
        // Stop plasma
        if (playerPlasmas.ContainsKey(playerIndex))
        {
            playerPlasmas[playerIndex].StopPlasma();
        }
    }
    
    public WeaponDataSO GetWeaponConfig(BulletType weaponType)
    {
        weaponDatabase.TryGetValue(weaponType, out WeaponDataSO config);
        return config;
    }
    
    public WeaponData CreateWeaponFromType(BulletType weaponType, int level = 1)
    {
        WeaponDataSO config = GetWeaponConfig(weaponType);
        if (config == null) return null;
        
        return new WeaponData
        {
            weaponType = weaponType,
            weaponName = config.weaponName,
            damage = config.baseDamage,
            level = level,
            fireRate = config.baseFireRate,
            overHeatMax = config.overHeatMax,
            coolingRate = config.coolingRate,
            bulletColor = config.bulletColor,
            bulletSize = config.bulletSize,
            rightAngle2 = config.rightAngle2,
            leftAngle2 = config.leftAngle2,
            rightAngle3 = config.rightAngle3,
            leftAngle3 = config.leftAngle3,
            rightAngle4 = config.rightAngle4,
            leftAngle4 = config.leftAngle4
        };
    }
    
    public void UpdateWeaponLevel(WeaponData weaponData, int newLevel)
    {
        if (weaponData == null) return;
        
        WeaponDataSO config = GetWeaponConfig(weaponData.weaponType);
        if (config == null) return;
        
        weaponData.level = Mathf.Clamp(newLevel, 1, 4);
        
        // Update damage based on level
        weaponData.damage = config.baseDamage + (weaponData.level - 1) * 10;
        
        // Update other stats based on weapon type and level
        switch (weaponData.weaponType)
        {
            case BulletType.Laser:
            case BulletType.Plasma:
                // Continuous weapons: increase damage and beam size
                weaponData.damage = config.baseDamage * weaponData.level;
                break;
        }
    }
    
    // Cleanup when player is destroyed
    public void CleanupPlayerWeapons(int playerIndex)
    {
        if (playerLasers.ContainsKey(playerIndex))
        {
            if (playerLasers[playerIndex] != null)
                Destroy(playerLasers[playerIndex].gameObject);
            playerLasers.Remove(playerIndex);
        }
        
        if (playerPlasmas.ContainsKey(playerIndex))
        {
            if (playerPlasmas[playerIndex] != null)
                Destroy(playerPlasmas[playerIndex].gameObject);
            playerPlasmas.Remove(playerIndex);
        }
        
        if (activeLasers.ContainsKey(playerIndex))
        {
            foreach (var laser in activeLasers[playerIndex])
            {
                if (laser != null)
                    Destroy(laser);
            }
            activeLasers.Remove(playerIndex);
        }
    }
    
    private void OnDestroy()
    {
        // Cleanup all weapon systems
        foreach (var kvp in playerLasers)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        
        foreach (var kvp in playerPlasmas)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
    }
}