using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all audio in the game - equivalent to sound management from C++
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    
    [Header("Player SFX")]
    public AudioClip playerShootSound;
    public AudioClip playerExplosionSound;
    public AudioClip weaponPickupSound;
    
    [Header("Enemy SFX")]
    public AudioClip enemyHitSound;
    public AudioClip enemyExplosionSound;
    
    [Header("Weapon SFX")]
    public AudioClip ionBlasterSound;
    public AudioClip neutronGunSound;
    public AudioClip hypergunSound;
    public AudioClip vulcangunSound;
    public AudioClip laserSound;
    public AudioClip plasmaSound;
    
    [Header("UI SFX")]
    public AudioClip buttonClickSound;
    public AudioClip coinPickupSound;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    public bool soundVFX = true;
    public bool musicVFX = true;
    
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        
        // Setup audio sources if not assigned
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("Music Source");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        UpdateAudioSettings();
    }
    
    void UpdateAudioSettings()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.mute = !musicVFX;
        }
        
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
            sfxSource.mute = !soundVFX;
        }
    }
    
    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null && musicVFX)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null && soundVFX)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    // Weapon-specific sound methods
    public void PlayWeaponSound(BulletType weaponType)
    {
        AudioClip weaponClip = null;
        
        switch (weaponType)
        {
            case BulletType.Ion:
                weaponClip = ionBlasterSound;
                break;
            case BulletType.Neutron:
                weaponClip = neutronGunSound;
                break;
            case BulletType.Hyper:
                weaponClip = hypergunSound;
                break;
            case BulletType.Vulcan:
                weaponClip = vulcangunSound;
                break;
            case BulletType.Laser:
                weaponClip = laserSound;
                break;
            case BulletType.Plasma:
                weaponClip = plasmaSound;
                break;
            default:
                weaponClip = playerShootSound;
                break;
        }
        
        PlaySFX(weaponClip);
    }
    
    public void PlayPlayerExplosion()
    {
        PlaySFX(playerExplosionSound);
    }
    
    public void PlayEnemyHit()
    {
        PlaySFX(enemyHitSound);
    }
    
    public void PlayEnemyExplosion()
    {
        PlaySFX(enemyExplosionSound);
    }
    
    public void PlayCoinPickup()
    {
        PlaySFX(coinPickupSound);
    }
    
    public void PlayWeaponPickup()
    {
        PlaySFX(weaponPickupSound);
    }
    
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }
    
    // Settings management
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
    
    public void ToggleMusic()
    {
        musicVFX = !musicVFX;
        if (musicSource != null)
            musicSource.mute = !musicVFX;
    }
    
    public void ToggleSFX()
    {
        soundVFX = !soundVFX;
        if (sfxSource != null)
            sfxSource.mute = !soundVFX;
    }
}