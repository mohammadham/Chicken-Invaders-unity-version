using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual player HUD display
/// Shows score, lives, weapon overheat, etc.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerController player;
    public int playerIndex = 0;
    
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public Image[] lifeIcons;
    public Image weaponOverheatBar;
    public Image weaponOverheatFill;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponLevelText;
    
    [Header("Colors")]
    public Color normalOverheatColor = Color.green;
    public Color warningOverheatColor = Color.yellow;
    public Color dangerOverheatColor = Color.red;
    
    private void Start()
    {
        // Find player controller if not assigned
        if (player == null)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            foreach (var p in players)
            {
                if (p.playerIndex == playerIndex)
                {
                    player = p;
                    break;
                }
            }
        }
        
        InitializeHUD();
    }
    
    void InitializeHUD()
    {
        // Setup initial values
        if (player != null)
        {
            UpdateScore(player.score);
            UpdateLives(player.lives);
            UpdateWeaponInfo();
        }
    }
    
    private void Update()
    {
        if (player != null)
        {
            UpdateHUD();
        }
    }
    
    void UpdateHUD()
    {
        UpdateScore(player.score);
        UpdateLives(player.lives);
        UpdateWeaponOverheat();
        UpdateWeaponInfo();
    }
    
    public void UpdateScore(long score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString("N0");
        }
    }
    
    public void UpdateLives(int lives)
    {
        if (lifeIcons != null)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].gameObject.SetActive(i < lives);
                }
            }
        }
    }
    
    void UpdateWeaponOverheat()
    {
        if (weaponOverheatFill != null && player != null)
        {
            float overheatPercent = player.GetWeaponOverheat();
            weaponOverheatFill.fillAmount = overheatPercent;
            
            // Change color based on overheat level
            Color targetColor = normalOverheatColor;
            if (overheatPercent > 0.8f)
                targetColor = dangerOverheatColor;
            else if (overheatPercent > 0.5f)
                targetColor = warningOverheatColor;
            
            weaponOverheatFill.color = targetColor;
        }
    }
    
    void UpdateWeaponInfo()
    {
        if (player == null) return;
        
        WeaponData currentWeapon = player.GetCurrentWeaponData();
        
        if (weaponNameText != null)
        {
            weaponNameText.text = currentWeapon.weaponName;
        }
        
        if (weaponLevelText != null)
        {
            weaponLevelText.text = $"Lv.{currentWeapon.level}";
        }
    }
}