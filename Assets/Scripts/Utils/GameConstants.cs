using UnityEngine;

/// <summary>
/// Game constants and configuration values - Updated for Phase 4 (Complete Weapon System)
/// </summary>
public static class GameConstants
{
    // Screen bounds
    public static readonly Vector2 SCREEN_BOUNDS = new Vector2(10f, 6f);
    
    // Player settings
    public const float PLAYER_SPEED = 3.5f;
    public const int PLAYER_LIVES = 3;
    public const float SHIELD_DURATION = 3f;
    public const float RESPAWN_SHIELD_DURATION = 3f;
    
    // Enemy settings
    public const int ENEMIES_PER_ROW = 10;
    public const int ENEMY_ROWS = 3;
    public const float ENEMY_SPEED = 0.5f;
    public const float ENEMY_FIRE_RATE = 0.5f;
    public const int ENEMY_HEALTH = 100;
    public const int ENEMY_DAMAGE = 50;
    
    // Enemy movement
    public const float ENEMY_MOVEMENT_BOUNDARY = 0.5f;
    public const float ENEMY_SPEED_INCREASE = 0.1f;
    public const int ENEMY_SPEED_INCREASE_INTERVAL = 5;
    
    // Weapon settings (Phase 4 - Complete)
    public const float WEAPON_FIRE_RATE = 0.015f;
    public const float WEAPON_OVERHEAT_MAX = 70f;
    public const float WEAPON_COOLING_RATE = 0.02f;
    public const float LIMITED_WEAPON_TIME = 30f;
    
    // Weapon specific settings
    public const float ION_BLASTER_FIRE_RATE = 0.4f;
    public const float NEUTRON_GUN_FIRE_RATE = 0.5f;
    public const float HYPER_GUN_FIRE_RATE = 0.5f;
    public const float VULCAN_GUN_FIRE_RATE = 0.5f;
    public const float PLASMA_RIFLE_FIRE_RATE = 0.01f;
    public const float LASER_FIRE_RATE = 0.01f;
    
    // Laser/Plasma settings
    public const float LASER_MAX_DISTANCE = 10f;
    public const float LASER_DAMAGE_PER_SECOND = 50f;
    public const float PLASMA_MAX_DISTANCE = 8f;
    public const float PLASMA_DAMAGE_PER_SECOND = 75f;
    public const float LASER_BEAM_WIDTH = 0.1f;
    public const float PLASMA_BEAM_WIDTH = 0.2f;
    
    // Bullet settings
    public const float BULLET_SPEED = 7f;
    public const float ENEMY_BULLET_SPEED = 1f;
    public const float BULLET_LIFETIME = 5f;
    
    // Scoring
    public const int ENEMY_SCORE_BASE = 100;
    public const int COIN_SCORE_MIN = 500;
    public const int COIN_SCORE_MAX = 1000;
    public const int CHICKEN_SCORE_MIN = 50;
    public const int CHICKEN_SCORE_MAX = 100;
    
    // Drop settings
    public const float COIN_DROP_CHANCE = 0.33f;
    public const float WEAPON_GIFT_DROP_CHANCE = 0.33f;
    public const float DROP_SPEED = 1f;
    public const float DROP_ROTATION_SPEED = 1f;
    public const int MAX_WEAPON_GIFTS_PER_LEVEL = 2;
    
    // Animation settings
    public const float ANIMATION_FRAME_RATE = 0.1f;
    public const float ENEMY_ANIMATION_SPEED = 0.15f;
    public const float EXPLOSION_FRAME_RATE = 0.07f;
    public const float SHIELD_FLASH_RATE = 0.2f;
    public const float DAMAGE_FLASH_DURATION = 0.1f;
    
    // Effect settings (Phase 4)
    public const float MUZZLE_FLASH_DURATION = 0.1f;
    public const float HIT_EFFECT_DURATION = 0.3f;
    public const float EXPLOSION_EFFECT_DURATION = 0.5f;
    public const float SCREEN_SHAKE_INTENSITY = 0.1f;
    public const float SCREEN_SHAKE_DURATION = 0.2f;
    
    // Trail settings
    public const float TRAIL_TIME_VULCAN = 0.5f;
    public const float TRAIL_TIME_ION = 0.3f;
    public const float TRAIL_TIME_NEUTRON = 0.4f;
    public const float TRAIL_TIME_HYPER = 0.6f;
    public const float TRAIL_TIME_PLASMA = 0.4f;
    public const float TRAIL_TIME_LASER = 0.3f;
    
    // Input settings
    public const float INPUT_DEAD_ZONE = 0.1f;
    public const float TOUCH_SENSITIVITY = 2f;
    
    // Tags
    public const string TAG_PLAYER = "Player";
    public const string TAG_ENEMY = "Enemy";
    public const string TAG_BULLET = "Bullet";
    public const string TAG_PICKUP = "Pickup";
    public const string TAG_WEAPON_PICKUP = "WeaponPickup";
    public const string TAG_BOUNDARY = "Boundary";
    
    // Layers
    public const int LAYER_PLAYER = 8;
    public const int LAYER_ENEMY = 9;
    public const int LAYER_BULLET = 10;
    public const int LAYER_PICKUP = 11;
    public const int LAYER_UI = 5;
    public const int LAYER_EFFECTS = 12;
    
    // Physics settings
    public const float COLLISION_OFFSET = 0.1f;
    
    // Performance settings
    public const int MAX_BULLETS_ON_SCREEN = 100;
    public const int BULLET_POOL_SIZE = 200;
    public const int MAX_DROPS_ON_SCREEN = 50;
    public const int EFFECT_POOL_SIZE = 30;
    
    // Screen safe areas (for mobile)
    public const float SAFE_AREA_MARGIN = 0.5f;
    
    // Formation settings
    public const float FORMATION_HORIZONTAL_SPACING = 1f;
    public const float FORMATION_VERTICAL_SPACING = 1f;
    public static readonly Vector3 FORMATION_CENTER = new Vector3(0f, 3f, 0f);
    
    // Weapon upgrade settings
    public const int MAX_WEAPON_LEVEL = 4;
    public const int DAMAGE_BONUS_PER_LEVEL = 10;
    public const float FIRE_RATE_IMPROVEMENT_PER_LEVEL = 0.9f;
    
    // Continuous weapon settings
    public const float LASER_UPDATE_INTERVAL = 0.1f;
    public const float PLASMA_UPDATE_INTERVAL = 0.08f;
    public const float LASER_FLICKER_INTERVAL = 0.1f;
    public const float PLASMA_ARC_SEGMENTS = 5f;
}