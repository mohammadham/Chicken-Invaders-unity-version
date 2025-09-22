using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all dropped items - coins, chickens, weapon gifts
/// Equivalent to coinsAndChicken and WeaponGifts vectors from C++
/// </summary>
public class DropManager : MonoBehaviour
{
    [Header("Drop Prefabs")]
    public GameObject coinPrefab;
    public GameObject chickenPrefab;
    public GameObject weaponGiftPrefab;
    public GameObject upgradeGiftPrefab;
    
    [Header("Drop Settings")]
    public Transform dropContainer;
    public float dropSpeed = 1f;
    public float dropRotationSpeed = 1f;
    
    [Header("Weapon Gift Sprites")]
    public Sprite[] weaponGiftSprites; // Ion, Neutron, Hyper, Vulcan, Plasma, Laser
    public Sprite upgradeSprite;
    
    private List<DropItem> activeDrops = new List<DropItem>();
    private int giftsDroppedCount = 0;
    
    public static DropManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Update()
    {
        UpdateDrops();
    }
    
    void UpdateDrops()
    {
        // Update all active drops
        for (int i = activeDrops.Count - 1; i >= 0; i--)
        {
            if (activeDrops[i] == null)
            {
                activeDrops.RemoveAt(i);
                continue;
            }
            
            DropItem drop = activeDrops[i];
            
            // Move drop down
            drop.transform.Translate(Vector3.down * dropSpeed * Time.deltaTime);
            
            // Rotate drop
            drop.transform.Rotate(Vector3.forward * dropRotationSpeed * 60f * Time.deltaTime);
            
            // Check if drop is out of bounds
            if (drop.transform.position.y < -6f)
            {
                RemoveDrop(drop);
            }
        }
    }
    
    public void CreateCoin(Vector3 position, int value)
    {
        if (coinPrefab == null) return;
        
        GameObject coinObj = Instantiate(coinPrefab, position, Quaternion.identity);
        coinObj.transform.SetParent(dropContainer);
        
        DropItem dropItem = coinObj.GetComponent<DropItem>();
        if (dropItem == null)
            dropItem = coinObj.AddComponent<DropItem>();
        
        dropItem.Initialize(DropType.Coin, value);
        
        // Set visual
        SpriteRenderer spriteRenderer = coinObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Coin should have golden color
            spriteRenderer.color = Color.yellow;
        }
        
        activeDrops.Add(dropItem);
        
        Debug.Log($"Created coin drop worth {value} points at {position}");
    }
    
    public void CreateChicken(Vector3 position, int value)
    {
        if (chickenPrefab == null) return;
        
        GameObject chickenObj = Instantiate(chickenPrefab, position, Quaternion.identity);
        chickenObj.transform.SetParent(dropContainer);
        
        DropItem dropItem = chickenObj.GetComponent<DropItem>();
        if (dropItem == null)
            dropItem = chickenObj.AddComponent<DropItem>();
        
        dropItem.Initialize(DropType.Chicken, value);
        
        activeDrops.Add(dropItem);
        
        Debug.Log($"Created chicken drop worth {value} points at {position}");
    }
    
    public void CreateWeaponGift(Vector3 position, bool isUpgrade = false)
    {
        GameObject giftPrefab = isUpgrade ? upgradeGiftPrefab : weaponGiftPrefab;
        if (giftPrefab == null) return;
        
        GameObject giftObj = Instantiate(giftPrefab, position, Quaternion.identity);
        giftObj.transform.SetParent(dropContainer);
        
        DropItem dropItem = giftObj.GetComponent<DropItem>();
        if (dropItem == null)
            dropItem = giftObj.AddComponent<DropItem>();
        
        if (isUpgrade)
        {
            dropItem.Initialize(DropType.WeaponUpgrade, 0);
            
            // Set upgrade sprite
            SpriteRenderer spriteRenderer = giftObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && upgradeSprite != null)
            {
                spriteRenderer.sprite = upgradeSprite;
            }
        }
        else
        {
            // Random weapon type
            BulletType weaponType = GetRandomWeaponType();
            dropItem.Initialize(DropType.Weapon, 0, weaponType);
            
            // Set appropriate weapon sprite
            SpriteRenderer spriteRenderer = giftObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && weaponGiftSprites != null)
            {
                int spriteIndex = GetWeaponSpriteIndex(weaponType);
                if (spriteIndex >= 0 && spriteIndex < weaponGiftSprites.Length)
                {
                    spriteRenderer.sprite = weaponGiftSprites[spriteIndex];
                }
            }
        }
        
        activeDrops.Add(dropItem);
        giftsDroppedCount++;
        
        Debug.Log($"Created weapon gift ({(isUpgrade ? "Upgrade" : dropItem.weaponType.ToString())}) at {position}");
    }
    
    BulletType GetRandomWeaponType()
    {
        // Random weapon type (excluding Normal and EnemyEgg)
        BulletType[] weaponTypes = {
            BulletType.Ion,
            BulletType.Neutron,
            BulletType.Hyper,
            BulletType.Vulcan,
            BulletType.Plasma,
            BulletType.Laser
        };
        
        return weaponTypes[Random.Range(0, weaponTypes.Length)];
    }
    
    int GetWeaponSpriteIndex(BulletType weaponType)
    {
        switch (weaponType)
        {
            case BulletType.Ion: return 0;
            case BulletType.Neutron: return 1;
            case BulletType.Hyper: return 2;
            case BulletType.Vulcan: return 3;
            case BulletType.Plasma: return 4;
            case BulletType.Laser: return 5;
            default: return 0;
        }
    }
    
    public void RemoveDrop(DropItem drop)
    {
        if (activeDrops.Contains(drop))
        {
            activeDrops.Remove(drop);
        }
        
        if (drop != null)
        {
            Destroy(drop.gameObject);
        }
    }
    
    public void ClearAllDrops()
    {
        foreach (var drop in activeDrops.ToArray())
        {
            if (drop != null)
            {
                Destroy(drop.gameObject);
            }
        }
        
        activeDrops.Clear();
        giftsDroppedCount = 0;
    }
    
    // Public getters
    public int GetActiveDropCount() => activeDrops.Count;
    public int GetGiftsDroppedCount() => giftsDroppedCount;
    public List<DropItem> GetActiveDrops() => new List<DropItem>(activeDrops);
}

public enum DropType
{
    Coin,
    Chicken,
    Weapon,
    WeaponUpgrade
}