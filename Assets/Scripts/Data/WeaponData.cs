using System;
using UnityEngine;

/// <summary>
/// Weapon configuration data - equivalent to Weapon struct from C++
/// </summary>
[System.Serializable]
public class WeaponData
{
    [Header("Basic Properties")]
    public BulletType weaponType;
    public string weaponName;
    public int damage = 30;
    public int level = 1;
    
    [Header("Fire Rate")]
    public float fireRate = 0.5f; // Time between shots
    
    [Header("Overheat System")]
    public float overHeatMax = 20f;
    public float coolingRate = 0.002f;
    
    [Header("Visual")]
    public Sprite weaponSprite;
    public Color bulletColor = Color.white;
    public Vector2 bulletSize = new Vector2(0.8f, 2f);
    
    [Header("Audio")]
    public AudioClip fireSound;
    
    // Weapon specific angles for different levels (from C++)
    [Header("Advanced Settings")]
    public float rightAngle2 = 80f;
    public float leftAngle2 = 100f;
    public float rightAngle3 = 70f;
    public float leftAngle3 = 110f;
    public float rightAngle4 = 85f;
    public float leftAngle4 = 95f;
    
    public WeaponData()
    {
        // Default constructor
    }
    
    public WeaponData(BulletType type, string name, int dmg, float rate, float heatMax, float cooling)
    {
        weaponType = type;
        weaponName = name;
        damage = dmg;
        fireRate = rate;
        overHeatMax = heatMax;
        coolingRate = cooling;
        level = 1;
    }
    
    // Create weapon presets similar to C++ version
    public static WeaponData CreateIonBlaster()
    {
        return new WeaponData(BulletType.Ion, "Ion Blaster", 30, 0.4f, 20f, 0.002f)
        {
            bulletSize = new Vector2(0.8f, 2f),
            bulletColor = Color.cyan,
            rightAngle2 = 80f,
            leftAngle2 = 100f,
            rightAngle3 = 80f,
            leftAngle3 = 100f,
            rightAngle4 = 85f,
            leftAngle4 = 95f
        };
    }
    
    public static WeaponData CreateNeutronGun()
    {
        return new WeaponData(BulletType.Neutron, "Neutron Gun", 30, 0.5f, 20f, 0.002f)
        {
            bulletSize = new Vector2(0.8f, 2f),
            bulletColor = Color.green,
            rightAngle2 = 80f,
            leftAngle2 = 100f,
            rightAngle3 = 70f,
            leftAngle3 = 110f,
            rightAngle4 = 85f,
            leftAngle4 = 95f
        };
    }
    
    public static WeaponData CreateHyperGun()
    {
        return new WeaponData(BulletType.Hyper, "Hyper Gun", 30, 0.5f, 20f, 0.02f)
        {
            bulletSize = new Vector2(0.8f, 2f),
            bulletColor = Color.red,
            rightAngle2 = 85f,
            leftAngle2 = 95f,
            rightAngle3 = 75f,
            leftAngle3 = 105f,
            rightAngle4 = 85f,
            leftAngle4 = 95f
        };
    }
    
    public static WeaponData CreateVulcanGun()
    {
        return new WeaponData(BulletType.Vulcan, "Vulcan Chaingun", 30, 0.5f, 20f, 0.02f)
        {
            bulletSize = new Vector2(0.6f, 1.5f),
            bulletColor = Color.yellow,
            rightAngle2 = 85f,
            leftAngle2 = 95f,
            rightAngle3 = 85f,
            leftAngle3 = 95f,
            rightAngle4 = 85f,
            leftAngle4 = 95f
        };
    }
    
    public static WeaponData CreatePlasmaRifle()
    {
        return new WeaponData(BulletType.Plasma, "Plasma Rifle", 5, 0.01f, 70f, 0.02f)
        {
            bulletSize = new Vector2(1f, 2.5f),
            bulletColor = Color.magenta
        };
    }
    
    public static WeaponData CreateLaser()
    {
        return new WeaponData(BulletType.Laser, "Lightning Fryer", 5, 0.01f, 70f, 0.02f)
        {
            bulletSize = new Vector2(1f, 2.5f),
            bulletColor = Color.white
        };
    }
}