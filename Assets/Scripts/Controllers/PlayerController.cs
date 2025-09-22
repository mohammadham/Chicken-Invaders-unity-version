using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complete Player controller - equivalent to Player struct from C++
/// Updated for Phase 4 with complete Weapon integration
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerIndex = 0;
    public string playerName = "Player1";
    public int lives = GameConstants.PLAYER_LIVES;
    public long score = 0;
    public float speed = GameConstants.PLAYER_SPEED;
    
    [Header("Control Scheme")]
    public ControlScheme controlScheme = ControlScheme.WASD;
    
    [Header("Weapon System")]
    public Transform firePoint;
    public WeaponData primaryWeapon;
    public WeaponData secondaryWeapon;
    public bool useSecondaryWeapon = false;
    public float limitedWeaponTime = 30f;
    
    [Header("Shield System")]
    public GameObject shieldVisual;
    public float shieldDuration = 3f;
    public bool hasShield = false;
    
    [Header("Animation")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleSprites;
    public Sprite[] leftSprites;
    public Sprite[] rightSprites;
    public Sprite[] explosionSprites;
    public float animationSpeed = 0.1f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip explosionSound;
    
    // Private variables
    public bool isDead = false;
    public bool isExploding = false;
    private bool isInvulnerable = false;
    private float secondaryWeaponTimer = 0f;
    private float shieldTimer = 0f;
    private Vector2 movement = Vector2.zero;
    private bool shootInput = false;
    private bool shootHeld = false;
    
    // Animation variables
    private int currentFrame = 0;
    private float animationTimer = 0f;
    private AnimationState currentAnimState = AnimationState.Idle;
    private bool facingLeft = false;
    private bool facingRight = false;
    
    // Weapon system (updated for Phase 4)
    private float weaponCooldown = 0f;
    private float weaponOverheat = 0f;
    private bool weaponOverheated = false;
    
    public enum ControlScheme
    {
        WASD,
        Arrows
    }
    
    public enum AnimationState
    {
        Idle,
        Left,
        Right,
        Exploding
    }
    
    private void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        // Setup initial position based on player index
        Vector3 startPos = playerIndex == 0 ? new Vector3(2f, -4f, 0f) : new Vector3(-2f, -4f, 0f);
        transform.position = startPos;
        
        // Setup weapon system with WeaponManager
        SetupWeapons();
        
        // Setup shield visual
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
        
        // Setup audio
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        Debug.Log($"Player {playerIndex} ({playerName}) initialized with {controlScheme} controls");
    }
    
    void SetupWeapons()
    {
        if (WeaponManager.Instance == null) return;
        
        // Setup primary weapon (default neutron gun for player 1, plasma for player 2)
        if (playerIndex == 0)
        {
            primaryWeapon = WeaponManager.Instance.CreateWeaponFromType(BulletType.Neutron, 4);
        }
        else
        {
            primaryWeapon = WeaponManager.Instance.CreateWeaponFromType(BulletType.Plasma, 1);
        }
        
        Debug.Log($"Player {playerIndex} equipped with {primaryWeapon.weaponName}");
    }
    
    public void SetupControls(ControlScheme scheme)
    {
        controlScheme = scheme;
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.gameStarted || GameManager.Instance.gamePaused))
            return;
        
        if (!isDead)
        {
            HandleInput();
            HandleMovement();
            HandleWeaponSystem();
            HandleSecondaryWeapon();
            HandleShield();
        }
        
        HandleAnimation();
    }
    
    void HandleInput()
    {
        if (InputManager.Instance != null)
        {
            var input = InputManager.Instance.GetPlayerInput(playerIndex);
            movement = input.movement;
            shootInput = input.shootPressed;
            shootHeld = input.shootHeld;
        }
    }
    
    void HandleMovement()
    {
        if (isExploding) return;
        
        // Calculate new position
        Vector3 newPosition = transform.position + new Vector3(movement.x, movement.y, 0f) * speed * Time.deltaTime;
        
        // Clamp to screen bounds
        Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        newPosition.x = Mathf.Clamp(newPosition.x, -screenBounds.x + 0.5f, screenBounds.x - 0.5f);
        newPosition.y = Mathf.Clamp(newPosition.y, -screenBounds.y + 0.5f, screenBounds.y - 0.5f);
        
        transform.position = newPosition;
        
        // Update animation state based on movement
        UpdateMovementAnimation();
    }
    
    void UpdateMovementAnimation()
    {
        if (movement.x < -0.1f)
        {
            if (!facingLeft)
            {
                currentAnimState = AnimationState.Left;
                facingLeft = true;
                facingRight = false;
            }
        }
        else if (movement.x > 0.1f)
        {
            if (!facingRight)
            {
                currentAnimState = AnimationState.Right;
                facingRight = true;
                facingLeft = false;
            }
        }
        else
        {
            // Return to idle if not moving
            if (facingLeft || facingRight)
            {
                currentAnimState = AnimationState.Idle;
                facingLeft = false;
                facingRight = false;
            }
        }
    }
    
    void HandleWeaponSystem()
    {
        // Update weapon cooldown
        if (weaponCooldown > 0f)
            weaponCooldown -= Time.deltaTime;
        
        // Handle weapon overheat (updated for Phase 4)
        WeaponData currentWeapon = GetCurrentWeapon();
        if (weaponOverheated)
        {
            weaponOverheat -= currentWeapon.coolingRate * 60f * Time.deltaTime; // Max cooling rate
            if (weaponOverheat <= 0f)
            {
                weaponOverheat = 0f;
                weaponOverheated = false;
            }
        }
        else
        {
            // Natural cooling
            if (weaponOverheat > 0f)
            {
                weaponOverheat -= currentWeapon.coolingRate * Time.deltaTime;
                if (weaponOverheat < 0f)
                    weaponOverheat = 0f;
            }
        }
        
        // Handle shooting
        if (shootHeld && !weaponOverheated && weaponCooldown <= 0f)
        {
            Shoot();
        }
        else if (!shootHeld)
        {
            // Stop continuous weapons when not shooting
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.StopContinuousWeapons(playerIndex);
            }
        }
    }
    
    void HandleSecondaryWeapon()
    {
        if (useSecondaryWeapon)
        {
            secondaryWeaponTimer -= Time.deltaTime;
            if (secondaryWeaponTimer <= 0f)
            {
                useSecondaryWeapon = false;
                weaponOverheat = 0f; // Reset overheat when switching back
            }
        }
    }
    
    void HandleShield()
    {
        if (hasShield)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
        }
    }
    
    void Shoot()
    {
        if (firePoint == null || WeaponManager.Instance == null) return;
        
        WeaponData currentWeapon = GetCurrentWeapon();
        
        // Check if weapon can fire
        if (weaponOverheat >= currentWeapon.overHeatMax)
        {
            weaponOverheated = true;
            return;
        }
        
        // Fire weapon using WeaponManager (Phase 4 integration)
        Vector3 firePosition = firePoint.position;
        Vector3 fireDirection = Vector3.up;
        
        WeaponManager.Instance.FireWeapon(this, currentWeapon, firePosition, fireDirection);
        
        // Update weapon stats
        weaponCooldown = currentWeapon.fireRate;
        weaponOverheat += currentWeapon.fireRate * 10f; // Heat generation
    }
    
    WeaponData GetCurrentWeapon()
    {
        return useSecondaryWeapon ? secondaryWeapon : primaryWeapon;
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable || hasShield) return;
        
        Die();
    }
    
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isExploding = true;
        currentAnimState = AnimationState.Exploding;
        currentFrame = 0;
        
        // Stop any continuous weapons
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.StopContinuousWeapons(playerIndex);
        }
        
        // Reset weapon systems
        weaponOverheat = 0f;
        weaponOverheated = false;
        useSecondaryWeapon = false;
        
        // Reduce weapon level
        if (primaryWeapon != null && primaryWeapon.level > 1)
        {
            primaryWeapon.level--;
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.UpdateWeaponLevel(primaryWeapon, primaryWeapon.level);
            }
        }
        
        // Play explosion sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerExplosion();
        }
        
        // Start respawn process
        StartCoroutine(RespawnProcess());
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath(this);
        }
    }
    
    IEnumerator RespawnProcess()
    {
        // Play explosion animation
        yield return StartCoroutine(PlayExplosionAnimation());
        
        if (lives > 0)
        {
            Respawn();
        }
        else
        {
            // Player is completely dead
            gameObject.SetActive(false);
        }
    }
    
    IEnumerator PlayExplosionAnimation()
    {
        if (explosionSprites == null || explosionSprites.Length == 0)
        {
            yield return new WaitForSeconds(1f);
            yield break;
        }
        
        for (int i = 0; i < explosionSprites.Length; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.sprite = explosionSprites[i];
            
            yield return new WaitForSeconds(GameConstants.EXPLOSION_FRAME_RATE);
        }
    }
    
    void Respawn()
    {
        lives--;
        isDead = false;
        isExploding = false;
        currentAnimState = AnimationState.Idle;
        currentFrame = 0;
        
        // Reset position
        Vector3 respawnPos = playerIndex == 0 ? new Vector3(2f, -4f, 0f) : new Vector3(-2f, -4f, 0f);
        transform.position = respawnPos;
        
        // Activate shield for protection
        ActivateShield(shieldDuration);
        
        // Set back to normal sprite
        if (spriteRenderer != null && idleSprites != null && idleSprites.Length > 0)
        {
            spriteRenderer.sprite = idleSprites[0];
        }
        
        Debug.Log($"Player {playerIndex} respawned. Lives remaining: {lives}");
    }
    
    public void ActivateShield(float duration)
    {
        hasShield = true;
        shieldTimer = duration;
        
        if (shieldVisual != null)
            shieldVisual.SetActive(true);
        
        StartCoroutine(ShieldFlashEffect());
    }
    
    void DeactivateShield()
    {
        hasShield = false;
        shieldTimer = 0f;
        
        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }
    
    IEnumerator ShieldFlashEffect()
    {
        float flashTimer = 0f;
        bool visible = true;
        
        while (hasShield && shieldTimer > 0f)
        {
            flashTimer += Time.deltaTime;
            
            if (flashTimer >= 0.2f)
            {
                visible = !visible;
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = visible ? 1f : 0.5f;
                    spriteRenderer.color = color;
                }
                flashTimer = 0f;
            }
            
            yield return null;
        }
        
        // Restore normal visibility
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }
    
    void HandleAnimation()
    {
        if (spriteRenderer == null) return;
        
        animationTimer += Time.deltaTime;
        
        if (animationTimer >= animationSpeed)
        {
            animationTimer = 0f;
            
            switch (currentAnimState)
            {
                case AnimationState.Idle:
                    AnimateSprites(idleSprites);
                    break;
                case AnimationState.Left:
                    AnimateSprites(leftSprites);
                    break;
                case AnimationState.Right:
                    AnimateSprites(rightSprites);
                    break;
                case AnimationState.Exploding:
                    // Explosion animation is handled separately
                    break;
            }
        }
    }
    
    void AnimateSprites(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;
        
        currentFrame = (currentFrame + 1) % sprites.Length;
        spriteRenderer.sprite = sprites[currentFrame];
    }
    
    public void ChangeWeapon(WeaponData newWeapon, bool isSecondary = false)
    {
        weaponOverheat = 0f;
        weaponOverheated = false;
        
        if (isSecondary)
        {
            if (newWeapon.weaponName == primaryWeapon.weaponName)
            {
                ImproveWeapon();
                return;
            }
            
            secondaryWeapon = newWeapon;
            useSecondaryWeapon = true;
            secondaryWeaponTimer = limitedWeaponTime;
        }
        else
        {
            primaryWeapon = newWeapon;
            useSecondaryWeapon = false;
        }
        
        Debug.Log($"Player {playerIndex} changed weapon to {newWeapon.weaponName}");
    }
    
    public void ImproveWeapon()
    {
        WeaponData currentWeapon = GetCurrentWeapon();
        
        if (currentWeapon.level < 4)
        {
            currentWeapon.level++;
            
            // Update weapon with WeaponManager
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.UpdateWeaponLevel(currentWeapon, currentWeapon.level);
            }
        }
        else
        {
            currentWeapon.damage += 10;
        }
        
        weaponOverheat = 0f;
        weaponOverheated = false;
        
        Debug.Log($"Player {playerIndex} weapon improved to level {currentWeapon.level}");
    }
    
    public void AddScore(long points)
    {
        score += points;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerScore(playerIndex, score);
        }
    }
    
    // Public getters for other systems
    public bool IsAlive => !isDead;
    public float GetWeaponOverheat() => weaponOverheat / GetCurrentWeapon().overHeatMax;
    public WeaponData GetCurrentWeaponData() => GetCurrentWeapon();
    
    private void OnDestroy()
    {
        // Cleanup weapons when player is destroyed
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.CleanupPlayerWeapons(playerIndex);
        }
    }
}