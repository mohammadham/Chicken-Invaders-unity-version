using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complete Enemy controller - equivalent to enemyStructure from C++
/// Handles movement patterns, shooting, damage, death, drops
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int enemyIndex = 0;
    public int healthPoints = 100;
    public int maxHealthPoints = 100;
    public float speed = GameConstants.ENEMY_SPEED;
    public int scoreValue = GameConstants.ENEMY_SCORE_BASE;
    public int damage = 50;
    
    [Header("Movement Settings")]
    public EnemyMovementType movementType = EnemyMovementType.Horizontal;
    public float startPoint = 0f;
    public float endPoint = 1f;
    public float upPosition = 4f;
    public float downPosition = 1f;
    
    [Header("Animation")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleSprites;
    public Sprite[] explosionSprites;
    public float animationSpeed = 0.15f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip explosionSound;
    
    [Header("Drops")]
    public GameObject coinPrefab;
    public GameObject chickenPrefab;
    public GameObject[] weaponGiftPrefabs;
    public float coinDropChance = 0.33f;
    public float weaponGiftDropChance = 0.33f;
    
    // Private variables
    public bool isDead = false;
    public bool isExploding = false;
    private bool rightBorder = false;
    private bool downMovement = false;
    private int speedCounter = 0;
    private float speedIncrease = 0f;
    
    // Animation variables
    private int currentFrame = 0;
    private float animationTimer = 0f;
    private int explosionXCounter = 0;
    private int explosionYCounter = 3;
    private bool animationDirection = false; // false = forward, true = backward
    
    // Movement specific variables
    private Vector3 originalPosition;
    private float currentPosition;
    private bool wave2Entering = true;
    private float difference = 0f;
    private bool loop = false;
    private bool topLeft = false, topRight = false, bottomRight = false, bottomLeft = false;
    
    public enum EnemyMovementType
    {
        Horizontal,     // Wave 1 - left/right movement
        Vertical,       // Wave 2 - up/down movement  
        Spiral,         // Wave 3 - spiral/circular movement
        Boss            // Boss movement (future)
    }
    
    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        originalPosition = transform.position;
        maxHealthPoints = healthPoints;
    }
    
    public void Initialize(int index, Vector3 startPosition)
    {
        enemyIndex = index;
        transform.position = startPosition;
        originalPosition = startPosition;
        
        // Set movement boundaries based on position
        startPoint = startPosition.x - 0.5f;
        endPoint = startPosition.x + 0.5f;
        upPosition = startPosition.y + 1f;
        downPosition = startPosition.y - 2f;
        
        // Determine movement type based on row
        int row = enemyIndex / GameConstants.ENEMIES_PER_ROW;
        switch (row)
        {
            case 0:
                movementType = EnemyMovementType.Horizontal;
                break;
            case 1:
                movementType = EnemyMovementType.Vertical;
                break;
            case 2:
                movementType = EnemyMovementType.Spiral;
                break;
        }
        
        Debug.Log($"Enemy {enemyIndex} initialized at {startPosition} with {movementType} movement");
    }
    
    private void Update()
    {
        if (isDead) return;
        
        if (!isExploding)
        {
            HandleMovement();
            HandleAnimation();
        }
        else
        {
            HandleExplosionAnimation();
        }
    }
    
    void HandleMovement()
    {
        switch (movementType)
        {
            case EnemyMovementType.Horizontal:
                MoveHorizontal();
                break;
            case EnemyMovementType.Vertical:
                MoveVertical();
                break;
            case EnemyMovementType.Spiral:
                MoveSpiral();
                break;
        }
    }
    
    void MoveHorizontal()
    {
        // Equivalent to move() function from C++
        float currentX = transform.position.x;
        
        if (currentX >= endPoint)
        {
            rightBorder = true;
            downMovement = true;
        }
        
        if (!rightBorder)
        {
            // Move right
            if (currentX + (speed * Time.deltaTime) <= endPoint)
                transform.Translate(Vector3.right * speed * Time.deltaTime);
            else
                transform.position = new Vector3(endPoint, transform.position.y, transform.position.z);
        }
        else
        {
            // Move left
            if (currentX - (speed * Time.deltaTime) >= startPoint)
                transform.Translate(Vector3.left * speed * Time.deltaTime);
            else
                transform.position = new Vector3(startPoint, transform.position.y, transform.position.z);
        }
        
        if (currentX <= startPoint)
        {
            rightBorder = false;
            downMovement = false;
            speedCounter++;
        }
        
        // Increase speed over time
        if (speedCounter >= 5)
        {
            speedIncrease += 0.1f;
            speed += speedIncrease;
            speedCounter = 0;
        }
    }
    
    void MoveVertical()
    {
        // Equivalent to moveUPandDown() function from C++
        float currentY = transform.position.y;
        
        if (currentY >= downPosition)
        {
            downMovement = true;
        }
        
        if (!downMovement)
        {
            // Move down and slightly right
            transform.Translate(new Vector3(2.5f * Time.deltaTime, speed * Time.deltaTime, 0f));
        }
        else
        {
            // Move up
            transform.Translate(new Vector3(0f, -speed * Time.deltaTime, 0f));
        }
        
        if (currentY <= upPosition)
        {
            downMovement = false;
            speedCounter++;
        }
        
        if (speedCounter >= 5)
        {
            speedIncrease += 0.1f;
            speed += speedIncrease;
            speedCounter = 0;
        }
    }
    
    void MoveSpiral()
    {
        // More complex spiral movement pattern
        float time = Time.time * speed;
        float radius = 1f;
        
        float x = originalPosition.x + Mathf.Cos(time) * radius;
        float y = originalPosition.y + Mathf.Sin(time) * radius * 0.5f;
        
        transform.position = new Vector3(x, y, transform.position.z);
        
        // Gradually move down
        originalPosition += Vector3.down * Time.deltaTime * 0.5f;
    }
    
    void HandleAnimation()
    {
        if (isExploding || idleSprites == null || idleSprites.Length == 0) return;
        
        animationTimer += Time.deltaTime;
        
        if (animationTimer >= animationSpeed)
        {
            animationTimer = 0f;
            
            if (!animationDirection)
            {
                currentFrame++;
                if (currentFrame >= idleSprites.Length - 1)
                {
                    currentFrame = idleSprites.Length - 1;
                    animationDirection = true;
                }
            }
            else
            {
                currentFrame--;
                if (currentFrame <= 0)
                {
                    currentFrame = 0;
                    animationDirection = false;
                }
            }
            
            if (spriteRenderer != null)
                spriteRenderer.sprite = idleSprites[currentFrame];
        }
    }
    
    void HandleExplosionAnimation()
    {
        if (explosionSprites == null || explosionSprites.Length == 0) return;
        
        animationTimer += Time.deltaTime;
        
        if (animationTimer >= GameConstants.EXPLOSION_FRAME_RATE)
        {
            animationTimer = 0f;
            
            // Calculate grid position (8x6 explosion sprite sheet)
            int spriteIndex = explosionYCounter * 8 + explosionXCounter;
            
            if (spriteIndex < explosionSprites.Length)
            {
                if (spriteRenderer != null)
                    spriteRenderer.sprite = explosionSprites[spriteIndex];
                
                explosionXCounter++;
                
                if (explosionXCounter >= 8)
                {
                    explosionXCounter = 0;
                    explosionYCounter++;
                }
                
                if (explosionYCounter >= 6)
                {
                    // Explosion animation complete
                    CompleteDeath();
                }
            }
            else
            {
                CompleteDeath();
            }
        }
    }
    
    public void Shoot()
    {
        if (isDead || isExploding) return;
        
        // Create egg bullet (equivalent to shoot() function from C++)
        if (BulletManager.Instance != null)
        {
            Vector3 firePosition = transform.position + Vector3.down * 0.5f;
            Vector3 fireDirection = Vector3.down;
            
            BulletManager.Instance.SpawnBullet(
                firePosition, 
                fireDirection, 
                GameConstants.ENEMY_BULLET_SPEED, 
                damage, 
                false, // isPlayerBullet = false
                BulletType.EnemyEgg
            );
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        if (isDead || isExploding) return;
        
        healthPoints -= damageAmount;
        
        if (healthPoints <= 0)
        {
            Die();
        }
        else
        {
            // Play hit sound
            if (Random.Range(0, 5) >= 2) // 60% chance to play hit sound
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayEnemyHit();
                else if (audioSource != null && hitSound != null)
                    audioSource.PlayOneShot(hitSound);
            }
            
            // Flash effect for damage
            StartCoroutine(DamageFlashEffect());
        }
    }
    
    IEnumerator DamageFlashEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            
            yield return new WaitForSeconds(0.1f);
            
            spriteRenderer.color = originalColor;
        }
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isExploding = true;
        
        // Notify enemy manager
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.OnEnemyDestroyed(this);
        
        // Notify game manager
        if (GameManager.Instance != null)
            GameManager.Instance.OnEnemyDestroyed(this);
        
        // Play explosion sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayEnemyExplosion();
        else if (audioSource != null && explosionSound != null)
            audioSource.PlayOneShot(explosionSound);
        
        // Drop items
        DropItems();
        
        // Start explosion animation
        explosionXCounter = 0;
        explosionYCounter = 3;
        animationTimer = 0f;
    }
    
    public void DropItems()
    {
        Vector3 dropPosition = transform.position;
        
        // Drop coin (33% chance)
        if (Random.Range(0f, 1f) <= coinDropChance)
        {
            CreateCoinDrop(dropPosition + Vector3.right * Random.Range(-0.5f, 0.5f));
        }
        
        // Always drop chicken
        CreateChickenDrop(dropPosition + Vector3.right * Random.Range(-0.3f, 0.3f));
        
        // Drop weapon gift (33% chance, limited per level)
        if (Random.Range(0f, 1f) <= weaponGiftDropChance && CanDropWeaponGift())
        {
            CreateWeaponGiftDrop(dropPosition + Vector3.right * Random.Range(-0.7f, 0.7f));
        }
    }
    
    void CreateCoinDrop(Vector3 position)
    {
        if (DropManager.Instance != null)
        {
            int coinValue = Random.Range(GameConstants.COIN_SCORE_MIN, GameConstants.COIN_SCORE_MAX + 1);
            DropManager.Instance.CreateCoin(position, coinValue);
        }
    }
    
    void CreateChickenDrop(Vector3 position)
    {
        if (DropManager.Instance != null)
        {
            int chickenValue = Random.Range(GameConstants.CHICKEN_SCORE_MIN, GameConstants.CHICKEN_SCORE_MAX + 1);
            DropManager.Instance.CreateChicken(position, chickenValue);
        }
    }
    
    void CreateWeaponGiftDrop(Vector3 position)
    {
        if (DropManager.Instance != null)
        {
            // Random weapon type or upgrade
            bool isUpgrade = Random.Range(0f, 1f) <= 0.3f; // 30% chance for upgrade
            DropManager.Instance.CreateWeaponGift(position, isUpgrade);
        }
    }
    
    bool CanDropWeaponGift()
    {
        // Limit weapon gift drops based on game state
        if (EnemyManager.Instance != null)
        {
            int totalEnemies = EnemyManager.Instance.GetTotalEnemyCount();
            int giftsDropped = DropManager.Instance != null ? DropManager.Instance.GetGiftsDroppedCount() : 0;
            
            return giftsDropped <= 1 && totalEnemies < 20;
        }
        
        return true;
    }
    
    void CompleteDeath()
    {
        // Completely remove enemy
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.RemoveEnemy(this);
        
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !isDead && !isExploding)
        {
            // Player collided with enemy
            player.TakeDamage(damage);
            
            // Enemy also dies from collision
            TakeDamage(999999);
        }
    }
    
    // Public getters
    public Vector3 GetPosition() => transform.position;
    public bool IsAlive() => !isDead && !isExploding;
    public float GetHealthPercent() => (float)healthPoints / maxHealthPoints;
    public EnemyMovementType GetMovementType() => movementType;
}