using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enhanced bullet controller for Phase 4 - supports all weapon types with trails and effects
/// Equivalent to Bullet struct from C++ with advanced features
/// </summary>
public class BulletController : MonoBehaviour
{
    [Header("Bullet Properties")]
    public int damage;
    public float speed;
    public bool isPlayerBullet;
    public BulletType bulletType;
    
    private Vector3 direction;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float angle;
    
    [Header("Visual Effects")]
    public GameObject hitEffectPrefab;
    public TrailRenderer bulletTrail;
    
    // Enhanced properties for Phase 4
    private bool hasTrail = false;
    private float lifeTime = 5f;
    private float currentLifeTime = 0f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bulletTrail = GetComponent<TrailRenderer>();
        
        // Add collider for bullet
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
        }
    }
    
    public void Initialize(Vector3 startPosition, Vector3 dir, float bulletSpeed, int bulletDamage, bool playerBullet, BulletType type)
    {
        transform.position = startPosition;
        direction = dir.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        isPlayerBullet = playerBullet;
        bulletType = type;
        currentLifeTime = 0f;
        
        // Set rotation based on direction (equivalent to Convert_to_radian from C++)
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        
        // Set bullet visual based on type and enhanced for Phase 4
        SetBulletVisual();
        
        // Setup trail effect
        SetupTrailEffect();
        
        gameObject.SetActive(true);
        
        // Set velocity
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // Create muzzle flash effect at spawn
        if (CombatEffects.Instance != null && isPlayerBullet)
        {
            CombatEffects.Instance.CreateMuzzleFlash(startPosition, direction, bulletType);
        }
        
        // Auto-destroy after time to prevent memory leaks
        StartCoroutine(AutoDestroy());
    }
    
    void SetBulletVisual()
    {
        // Enhanced visual setup for different weapon types
        if (spriteRenderer == null) return;
        
        Color bulletColor = Color.white;
        Vector3 bulletScale = Vector3.one;
        
        switch (bulletType)
        {
            case BulletType.Ion:
                bulletColor = Color.cyan;
                bulletScale = new Vector3(0.8f, 2f, 1f);
                break;
            case BulletType.Neutron:
                bulletColor = Color.green;
                bulletScale = new Vector3(0.8f, 2f, 1f);
                break;
            case BulletType.Hyper:
                bulletColor = Color.red;
                bulletScale = new Vector3(0.8f, 2f, 1f);
                break;
            case BulletType.Vulcan:
                bulletColor = Color.yellow;
                bulletScale = new Vector3(0.6f, 1.5f, 1f);
                break;
            case BulletType.Plasma:
                bulletColor = Color.magenta;
                bulletScale = new Vector3(1f, 2.5f, 1f);
                break;
            case BulletType.Laser:
                bulletColor = Color.white;
                bulletScale = new Vector3(1f, 2.5f, 1f);
                break;
            case BulletType.EnemyEgg:
                bulletColor = new Color(1f, 0.9f, 0.8f); // Egg color
                bulletScale = new Vector3(1.2f, 1.5f, 1f);
                break;
            default:
                bulletColor = Color.white;
                break;
        }
        
        spriteRenderer.color = bulletColor;
        transform.localScale = bulletScale;
        
        // Add glow effect for energy weapons
        if (bulletType == BulletType.Plasma || bulletType == BulletType.Laser)
        {
            AddGlowEffect(bulletColor);
        }
    }
    
    void AddGlowEffect(Color glowColor)
    {
        // Create a child object with larger scale and lower alpha for glow
        GameObject glow = new GameObject("BulletGlow");
        glow.transform.SetParent(transform);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * 1.5f;
        
        SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = spriteRenderer.sprite;
        glowRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.3f);
        glowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        
        // Make glow additive
        Material glowMaterial = new Material(Shader.Find("Sprites/Default"));
        glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        glowRenderer.material = glowMaterial;
    }
    
    void SetupTrailEffect()
    {
        // Setup trail based on weapon type
        if (bulletTrail == null)
        {
            bulletTrail = gameObject.AddComponent<TrailRenderer>();
        }
        
        switch (bulletType)
        {
            case BulletType.Vulcan:
                SetupSmokeTrail();
                break;
            case BulletType.Ion:
                SetupElectricTrail();
                break;
            case BulletType.Neutron:
                SetupEnergyTrail();
                break;
            case BulletType.Hyper:
                SetupHyperTrail();
                break;
            case BulletType.Plasma:
                SetupPlasmaTrail();
                break;
            case BulletType.Laser:
                SetupLaserTrail();
                break;
            default:
                bulletTrail.enabled = false;
                return;
        }
        
        hasTrail = true;
    }
    
    void SetupSmokeTrail()
    {
        bulletTrail.time = 0.5f;
        bulletTrail.startWidth = 0.1f;
        bulletTrail.endWidth = 0.02f;
        bulletTrail.material = CreateTrailMaterial(Color.gray);
        bulletTrail.sortingOrder = 1;
    }
    
    void SetupElectricTrail()
    {
        bulletTrail.time = 0.3f;
        bulletTrail.startWidth = 0.08f;
        bulletTrail.endWidth = 0.01f;
        bulletTrail.material = CreateTrailMaterial(Color.cyan);
        bulletTrail.sortingOrder = 1;
    }
    
    void SetupEnergyTrail()
    {
        bulletTrail.time = 0.4f;
        bulletTrail.startWidth = 0.06f;
        bulletTrail.endWidth = 0.01f;
        bulletTrail.material = CreateTrailMaterial(Color.green);
        bulletTrail.sortingOrder = 1;
    }
    
    void SetupHyperTrail()
    {
        bulletTrail.time = 0.6f;
        bulletTrail.startWidth = 0.12f;
        bulletTrail.endWidth = 0.02f;
        bulletTrail.material = CreateTrailMaterial(Color.red);
        bulletTrail.sortingOrder = 1;
    }
    
    void SetupPlasmaTrail()
    {
        bulletTrail.time = 0.4f;
        bulletTrail.startWidth = 0.15f;
        bulletTrail.endWidth = 0.03f;
        bulletTrail.material = CreateTrailMaterial(Color.magenta);
        bulletTrail.sortingOrder = 1;
    }
    
    void SetupLaserTrail()
    {
        bulletTrail.time = 0.3f;
        bulletTrail.startWidth = 0.1f;
        bulletTrail.endWidth = 0.02f;
        bulletTrail.material = CreateTrailMaterial(Color.white);
        bulletTrail.sortingOrder = 1;
    }
    
    Material CreateTrailMaterial(Color color)
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
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision detection
        if (isPlayerBullet)
        {
            // Player bullet hits enemy
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null && !enemy.isDead)
            {
                enemy.TakeDamage(damage);
                CreateHitEffect();
                DestroyBullet();
                return;
            }
        }
        else
        {
            // Enemy bullet hits player
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                player.TakeDamage(damage);
                CreateHitEffect();
                DestroyBullet();
                return;
            }
        }
        
        // Bullet hits boundary or other objects
        if (other.CompareTag("Boundary"))
        {
            DestroyBullet();
        }
    }
    
    void CreateHitEffect()
    {
        // Create impact effect using CombatEffects system
        if (CombatEffects.Instance != null)
        {
            CombatEffects.Instance.CreateHitEffect(transform.position, bulletType);
        }
        else if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
    
    void DestroyBullet()
    {
        // Clear trail before returning to pool
        if (hasTrail && bulletTrail != null)
        {
            bulletTrail.Clear();
        }
        
        // Return to pool instead of destroying
        if (BulletManager.Instance != null)
        {
            BulletManager.Instance.ReturnBulletToPool(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    IEnumerator AutoDestroy()
    {
        // Auto-destroy bullet after lifetime
        while (currentLifeTime < lifeTime)
        {
            currentLifeTime += Time.deltaTime;
            yield return null;
        }
        
        if (gameObject.activeInHierarchy)
        {
            DestroyBullet();
        }
    }
    
    // Check if bullet is out of screen bounds (equivalent to C++ boundary check)
    private void Update()
    {
        Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        
        if (transform.position.x < -screenBounds.x - 1f || 
            transform.position.x > screenBounds.x + 1f ||
            transform.position.y < -screenBounds.y - 1f || 
            transform.position.y > screenBounds.y + 1f)
        {
            DestroyBullet();
        }
    }
}