using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weapon pickup item - allows players to collect and change weapons
/// Extended from DropItem for weapon-specific functionality
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Pickup Settings")]
    public BulletType weaponType;
    public bool isUpgrade = false;
    public float rotationSpeed = 90f;
    public float pulseSpeed = 2f;
    
    [Header("Visual Effects")]
    public SpriteRenderer iconRenderer;
    public ParticleSystem glowEffect;
    public AudioClip pickupSound;
    
    private bool collected = false;
    private Vector3 originalScale;
    private Color originalColor;
    private float pulseTimer = 0f;
    
    private void Start()
    {
        SetupPickup();
    }
    
    void SetupPickup()
    {
        // Store original values
        if (iconRenderer != null)
        {
            originalScale = iconRenderer.transform.localScale;
            originalColor = iconRenderer.color;
        }
        
        // Setup collider as trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Setup weapon icon based on type
        SetWeaponIcon();
        
        // Setup glow effect
        SetupGlowEffect();
        
        // Set appropriate tag and layer
        gameObject.tag = "WeaponPickup";
        gameObject.layer = GameConstants.LAYER_PICKUP;
    }
    
    void SetWeaponIcon()
    {
        if (iconRenderer == null) return;
        
        // Set sprite and color based on weapon type
        if (isUpgrade)
        {
            iconRenderer.color = Color.white;
            // Should use upgrade sprite if available
        }
        else
        {
            iconRenderer.color = GetWeaponColor(weaponType);
            // Should use weapon-specific sprites if available
        }
    }
    
    Color GetWeaponColor(BulletType type)
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
    
    void SetupGlowEffect()
    {
        if (glowEffect == null) return;
        
        var main = glowEffect.main;
        main.startColor = isUpgrade ? Color.white : GetWeaponColor(weaponType);
        main.startLifetime = 1f;
        main.startSpeed = 0.5f;
        main.maxParticles = 20;
        
        var emission = glowEffect.emission;
        emission.rateOverTime = 10f;
        
        var shape = glowEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        
        glowEffect.Play();
    }
    
    private void Update()
    {
        if (collected) return;
        
        // Rotate the pickup
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        
        // Pulse effect
        UpdatePulseEffect();
        
        // Move down slowly
        transform.Translate(Vector3.down * Time.deltaTime * 0.5f);
        
        // Check if out of bounds
        if (transform.position.y < -6f)
        {
            DestroyPickup();
        }
    }
    
    void UpdatePulseEffect()
    {
        if (iconRenderer == null) return;
        
        pulseTimer += Time.deltaTime * pulseSpeed;
        
        // Scale pulsing
        float scaleMultiplier = 1f + Mathf.Sin(pulseTimer) * 0.2f;
        iconRenderer.transform.localScale = originalScale * scaleMultiplier;
        
        // Color pulsing
        float alpha = 0.8f + Mathf.Sin(pulseTimer * 2f) * 0.2f;
        Color currentColor = originalColor;
        currentColor.a = alpha;
        iconRenderer.color = currentColor;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            CollectWeapon(player);
        }
    }
    
    void CollectWeapon(PlayerController player)
    {
        if (collected) return;
        
        collected = true;
        
        if (isUpgrade)
        {
            // Upgrade current weapon
            player.ImproveWeapon();
            
            Debug.Log($"Player {player.playerIndex} collected weapon upgrade");
        }
        else
        {
            // Give new weapon
            if (WeaponManager.Instance != null)
            {
                WeaponData newWeapon = WeaponManager.Instance.CreateWeaponFromType(weaponType, 1);
                if (newWeapon != null)
                {
                    player.ChangeWeapon(newWeapon, true); // Give as secondary weapon (limited time)
                }
            }
            
            Debug.Log($"Player {player.playerIndex} collected {weaponType} weapon");
        }
        
        // Play pickup sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponPickup();
        }
        else if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        // Create pickup effect
        CreatePickupEffect();
        
        // Remove from drop manager if applicable
        if (DropManager.Instance != null)
        {
            DropItem dropItem = GetComponent<DropItem>();
            if (dropItem != null)
            {
                DropManager.Instance.RemoveDrop(dropItem);
            }
        }
        
        // Destroy pickup
        DestroyPickup();
    }
    
    void CreatePickupEffect()
    {
        // Create burst effect
        if (glowEffect != null)
        {
            var emission = glowEffect.emission;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.0f, 30)
            });
        }
        
        // Screen flash effect (optional)
        StartCoroutine(FlashEffect());
    }
    
    IEnumerator FlashEffect()
    {
        if (iconRenderer != null)
        {
            Color flashColor = Color.white;
            Color originalColor = iconRenderer.color;
            
            // Flash white
            iconRenderer.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            
            // Fade out
            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
                flashColor.a = alpha;
                iconRenderer.color = flashColor;
                yield return null;
            }
        }
    }
    
    void DestroyPickup()
    {
        // Stop particle effects
        if (glowEffect != null)
        {
            glowEffect.Stop();
        }
        
        // Destroy after a short delay to let effects finish
        Destroy(gameObject, 0.5f);
    }
    
    // Static factory methods for creating pickups
    public static GameObject CreateWeaponPickup(BulletType weaponType, Vector3 position)
    {
        GameObject pickupObj = new GameObject($"WeaponPickup_{weaponType}");
        pickupObj.transform.position = position;
        
        // Add components
        WeaponPickup pickup = pickupObj.AddComponent<WeaponPickup>();
        pickup.weaponType = weaponType;
        pickup.isUpgrade = false;
        
        // Add visual components
        SpriteRenderer renderer = pickupObj.AddComponent<SpriteRenderer>();
        pickup.iconRenderer = renderer;
        
        // Add collider
        CircleCollider2D collider = pickupObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        // Add particle system
        GameObject particleObj = new GameObject("GlowEffect");
        particleObj.transform.SetParent(pickupObj.transform);
        pickup.glowEffect = particleObj.AddComponent<ParticleSystem>();
        
        return pickupObj;
    }
    
    public static GameObject CreateUpgradePickup(Vector3 position)
    {
        GameObject pickupObj = new GameObject("UpgradePickup");
        pickupObj.transform.position = position;
        
        // Add components
        WeaponPickup pickup = pickupObj.AddComponent<WeaponPickup>();
        pickup.isUpgrade = true;
        
        // Add visual components
        SpriteRenderer renderer = pickupObj.AddComponent<SpriteRenderer>();
        pickup.iconRenderer = renderer;
        
        // Add collider
        CircleCollider2D collider = pickupObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        // Add particle system
        GameObject particleObj = new GameObject("GlowEffect");
        particleObj.transform.SetParent(pickupObj.transform);
        pickup.glowEffect = particleObj.AddComponent<ParticleSystem>();
        
        return pickupObj;
    }
}