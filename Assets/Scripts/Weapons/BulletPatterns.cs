using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bullet firing patterns for different weapon types and levels
/// Equivalent to the various bullet firing logic from C++ Weapon struct
/// </summary>
public static class BulletPatterns
{
    /// <summary>
    /// Fire bullets based on weapon type and level - exact equivalent to C++ firing patterns
    /// </summary>
    public static void FireBulletPattern(WeaponData weapon, Vector3 firePosition, Vector3 fireDirection, bool isPlayerBullet)
    {
        if (BulletManager.Instance == null) return;
        
        int level = weapon.level;
        int damage = weapon.damage;
        float speed = GameConstants.BULLET_SPEED;
        BulletType bulletType = weapon.weaponType;
        
        switch (level)
        {
            case 1:
                FireSingleBullet(firePosition, fireDirection, speed, damage, isPlayerBullet, bulletType);
                break;
                
            case 2:
                FireDoubleBullet(weapon, firePosition, fireDirection, speed, damage, isPlayerBullet, bulletType);
                break;
                
            case 3:
                FireTripleBullet(weapon, firePosition, fireDirection, speed, damage, isPlayerBullet, bulletType);
                break;
                
            case 4:
            default:
                FireQuadBullet(weapon, firePosition, fireDirection, speed, damage, isPlayerBullet, bulletType);
                break;
        }
    }
    
    static void FireSingleBullet(Vector3 position, Vector3 direction, float speed, int damage, bool isPlayerBullet, BulletType type)
    {
        BulletManager.Instance.SpawnBullet(position, direction, speed, damage, isPlayerBullet, type);
    }
    
    static void FireDoubleBullet(WeaponData weapon, Vector3 position, Vector3 direction, float speed, int damage, bool isPlayerBullet, BulletType type)
    {
        // Equivalent to C++ Level 2 firing logic
        float leftAngle = weapon.leftAngle2;
        float rightAngle = weapon.rightAngle2;
        
        // Convert C++ angles to Unity quaternions
        Vector3 leftDirection = Quaternion.Euler(0, 0, leftAngle - 90f) * direction;
        Vector3 rightDirection = Quaternion.Euler(0, 0, -(rightAngle - 90f)) * direction;
        
        // Fire two bullets with slight position offset
        Vector3 leftPos = position + Vector3.left * GetPositionOffset(type);
        Vector3 rightPos = position + Vector3.right * GetPositionOffset(type);
        
        BulletManager.Instance.SpawnBullet(leftPos, leftDirection, speed, damage, isPlayerBullet, type);
        BulletManager.Instance.SpawnBullet(rightPos, rightDirection, speed, damage, isPlayerBullet, type);
    }
    
    static void FireTripleBullet(WeaponData weapon, Vector3 position, Vector3 direction, float speed, int damage, bool isPlayerBullet, BulletType type)
    {
        // Equivalent to C++ Level 3 firing logic
        float leftAngle = weapon.leftAngle3;
        float rightAngle = weapon.rightAngle3;
        
        Vector3 leftDirection = Quaternion.Euler(0, 0, leftAngle - 90f) * direction;
        Vector3 rightDirection = Quaternion.Euler(0, 0, -(rightAngle - 90f)) * direction;
        
        float offset = GetPositionOffset(type);
        
        // Center bullet (straight)
        BulletManager.Instance.SpawnBullet(position, direction, speed, damage, isPlayerBullet, type);
        
        // Left and right bullets
        BulletManager.Instance.SpawnBullet(position + Vector3.left * offset, leftDirection, speed, damage, isPlayerBullet, type);
        BulletManager.Instance.SpawnBullet(position + Vector3.right * offset, rightDirection, speed, damage, isPlayerBullet, type);
    }
    
    static void FireQuadBullet(WeaponData weapon, Vector3 position, Vector3 direction, float speed, int damage, bool isPlayerBullet, BulletType type)
    {
        // Equivalent to C++ Level 4 firing logic
        float leftAngle = weapon.leftAngle4;
        float rightAngle = weapon.rightAngle4;
        
        Vector3 leftDirection1 = Quaternion.Euler(0, 0, leftAngle - 90f) * direction;
        Vector3 rightDirection1 = Quaternion.Euler(0, 0, -(rightAngle - 90f)) * direction;
        Vector3 leftDirection2 = Quaternion.Euler(0, 0, (leftAngle + 5f) - 90f) * direction;
        Vector3 rightDirection2 = Quaternion.Euler(0, 0, -((rightAngle + 5f) - 90f)) * direction;
        
        float offset = GetPositionOffset(type);
        
        // Four bullets in spread pattern
        BulletManager.Instance.SpawnBullet(position + Vector3.left * (offset * 1.5f), leftDirection1, speed, damage, isPlayerBullet, type);
        BulletManager.Instance.SpawnBullet(position + Vector3.left * (offset * 0.5f), leftDirection2, speed, damage, isPlayerBullet, type);
        BulletManager.Instance.SpawnBullet(position + Vector3.right * (offset * 0.5f), rightDirection2, speed, damage, isPlayerBullet, type);
        BulletManager.Instance.SpawnBullet(position + Vector3.right * (offset * 1.5f), rightDirection1, speed, damage, isPlayerBullet, type);
    }
    
    static float GetPositionOffset(BulletType type)
    {
        // Different weapons have different spreads
        switch (type)
        {
            case BulletType.Ion:
            case BulletType.Neutron:
                return 0.2f;
            case BulletType.Hyper:
                return 0.15f;
            case BulletType.Vulcan:
                return 0.25f;
            default:
                return 0.2f;
        }
    }
    
    /// <summary>
    /// Create weapon-specific bullet trails and effects
    /// </summary>
    public static void CreateBulletTrail(BulletController bullet, BulletType type)
    {
        if (bullet == null) return;
        
        GameObject trailObj = null;
        
        switch (type)
        {
            case BulletType.Vulcan:
                // Vulcan bullets have smoke trails
                trailObj = CreateSmokeTrail(bullet.transform);
                break;
                
            case BulletType.Ion:
                // Ion bullets have electric sparks
                trailObj = CreateElectricTrail(bullet.transform);
                break;
                
            case BulletType.Neutron:
                // Neutron bullets have energy glow
                trailObj = CreateEnergyTrail(bullet.transform);
                break;
                
            case BulletType.Hyper:
                // Hyper bullets have intense glow
                trailObj = CreateHyperTrail(bullet.transform);
                break;
        }
        
        if (trailObj != null)
        {
            // Make trail follow bullet
            trailObj.transform.SetParent(bullet.transform);
        }
    }
    
    static GameObject CreateSmokeTrail(Transform parent)
    {
        GameObject trail = new GameObject("SmokeTrail");
        trail.transform.SetParent(parent);
        trail.transform.localPosition = Vector3.zero;
        
        // Add TrailRenderer for smoke effect
        TrailRenderer trailRenderer = trail.AddComponent<TrailRenderer>();
        trailRenderer.time = 0.5f;
        trailRenderer.startWidth = 0.1f;
        trailRenderer.endWidth = 0.02f;
        trailRenderer.material = CreateTrailMaterial(Color.gray);
        
        return trail;
    }
    
    static GameObject CreateElectricTrail(Transform parent)
    {
        GameObject trail = new GameObject("ElectricTrail");
        trail.transform.SetParent(parent);
        trail.transform.localPosition = Vector3.zero;
        
        TrailRenderer trailRenderer = trail.AddComponent<TrailRenderer>();
        trailRenderer.time = 0.3f;
        trailRenderer.startWidth = 0.08f;
        trailRenderer.endWidth = 0.01f;
        trailRenderer.material = CreateTrailMaterial(Color.cyan);
        
        return trail;
    }
    
    static GameObject CreateEnergyTrail(Transform parent)
    {
        GameObject trail = new GameObject("EnergyTrail");
        trail.transform.SetParent(parent);
        trail.transform.localPosition = Vector3.zero;
        
        TrailRenderer trailRenderer = trail.AddComponent<TrailRenderer>();
        trailRenderer.time = 0.4f;
        trailRenderer.startWidth = 0.06f;
        trailRenderer.endWidth = 0.01f;
        trailRenderer.material = CreateTrailMaterial(Color.green);
        
        return trail;
    }
    
    static GameObject CreateHyperTrail(Transform parent)
    {
        GameObject trail = new GameObject("HyperTrail");
        trail.transform.SetParent(parent);
        trail.transform.localPosition = Vector3.zero;
        
        TrailRenderer trailRenderer = trail.AddComponent<TrailRenderer>();
        trailRenderer.time = 0.6f;
        trailRenderer.startWidth = 0.12f;
        trailRenderer.endWidth = 0.02f;
        trailRenderer.material = CreateTrailMaterial(Color.red);
        
        return trail;
    }
    
    static Material CreateTrailMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        
        // Make it additive for glow effect
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        
        return mat;
    }
}