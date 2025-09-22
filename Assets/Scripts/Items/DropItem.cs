using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Individual drop item - coins, chickens, weapon gifts
/// Equivalent to ScoreDrop and WeaponDrop structs from C++
/// </summary>
public class DropItem : MonoBehaviour
{
    [Header("Drop Settings")]
    public DropType dropType;
    public int scoreValue;
    public BulletType weaponType;
    public bool isWeaponUpgrade = false;
    
    private bool collected = false;
    private Rigidbody2D rb;
    private Collider2D col;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
        // Setup physics
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.isKinematic = true;
        }
        
        // Setup collider as trigger
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    public void Initialize(DropType type, int value, BulletType weapon = BulletType.Normal)
    {
        dropType = type;
        scoreValue = value;
        weaponType = weapon;
        isWeaponUpgrade = (type == DropType.WeaponUpgrade);
        
        // Set appropriate tag
        switch (dropType)
        {
            case DropType.Coin:
            case DropType.Chicken:
                gameObject.tag = "Pickup";
                break;
            case DropType.Weapon:
            case DropType.WeaponUpgrade:
                gameObject.tag = "WeaponPickup";
                break;
        }
        
        // Set layer
        gameObject.layer = GameConstants.LAYER_PICKUP;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            CollectItem(player);
        }
    }
    
    void CollectItem(PlayerController player)
    {
        if (collected) return;
        
        collected = true;
        
        switch (dropType)
        {
            case DropType.Coin:
            case DropType.Chicken:
                CollectScoreItem(player);
                break;
                
            case DropType.Weapon:
                CollectWeapon(player);
                break;
                
            case DropType.WeaponUpgrade:
                CollectWeaponUpgrade(player);
                break;
        }
        
        // Remove from drop manager
        if (DropManager.Instance != null)
        {
            DropManager.Instance.RemoveDrop(this);
        }
        
        // Visual/audio feedback
        CreateCollectionEffect();
        
        // Destroy object
        Destroy(gameObject);
    }
    
    void CollectScoreItem(PlayerController player)
    {
        // Add score to player
        player.AddScore(scoreValue);
        
        // Play pickup sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoinPickup();
        }
        
        Debug.Log($"Player {player.playerIndex} collected {dropType} worth {scoreValue} points");
    }
    
    void CollectWeapon(PlayerController player)
    {
        // Create weapon data
        WeaponData newWeapon = CreateWeaponFromType(weaponType);
        
        // Give weapon to player as secondary weapon (limited time)
        player.ChangeWeapon(newWeapon, true);
        
        // Play weapon pickup sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponPickup();
        }
        
        Debug.Log($"Player {player.playerIndex} collected {weaponType} weapon");
    }
    
    void CollectWeaponUpgrade(PlayerController player)
    {
        // Upgrade current weapon
        player.ImproveWeapon();
        
        // Play weapon pickup sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeaponPickup();
        }
        
        Debug.Log($"Player {player.playerIndex} collected weapon upgrade");
    }
    
    WeaponData CreateWeaponFromType(BulletType weaponType)
    {
        switch (weaponType)
        {
            case BulletType.Ion:
                return WeaponData.CreateIonBlaster();
            case BulletType.Neutron:
                return WeaponData.CreateNeutronGun();
            case BulletType.Hyper:
                return WeaponData.CreateHyperGun();
            case BulletType.Vulcan:
                return WeaponData.CreateVulcanGun();
            case BulletType.Plasma:
                return WeaponData.CreatePlasmaRifle();
            case BulletType.Laser:
                return WeaponData.CreateLaser();
            default:
                return WeaponData.CreateNeutronGun();
        }
    }
    
    void CreateCollectionEffect()
    {
        // Create simple particle effect or flash
        StartCoroutine(FlashEffect());
    }
    
    IEnumerator FlashEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            
            for (int i = 0; i < 3; i++)
            {
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.05f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}