using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Credits UI with scrolling text and acknowledgments
/// </summary>
public class CreditsUI : MonoBehaviour
{
    [Header("Credits Elements")]
    public GameObject creditsPanel;
    public ScrollRect creditsScrollRect;
    public TextMeshProUGUI creditsText;
    public Button backButton;
    public Button skipButton;
    
    [Header("Scrolling Settings")]
    public float scrollSpeed = 50f;
    public bool autoScroll = true;
    public float autoScrollDelay = 2f;
    
    [Header("Credits Content")]
    [TextArea(20, 30)]
    public string creditsContent = @"CHICKEN INVADERS
Unity Edition

DEVELOPED BY
[Your Name]

ORIGINAL CONCEPT
InterAction studios

PROGRAMMING
Unity C# Implementation
Advanced Weapon Systems
AI Enemy Behaviors
Mobile Touch Controls
Performance Optimization

GRAPHICS & DESIGN
Sprite Animation System
Particle Effects
UI/UX Design
Mobile Responsive Design

AUDIO SYSTEM
Dynamic Music Management
Spatial Audio Implementation
Weapon-Specific Sound Effects
Mobile Audio Optimization

SPECIAL FEATURES
6 Weapon Types with Upgrades
3 Enemy Movement Patterns
Advanced Object Pooling
Dynamic Quality Settings
Cross-Platform Support

PLATFORMS
PC (Windows/Mac/Linux)
Mobile (Android/iOS)
WebGL (Browser)

TECHNOLOGIES USED
Unity 2022.3 LTS
C# Programming
Unity Audio System
Unity UI System
Unity Physics 2D

ACKNOWLEDGMENTS
Unity Technologies
Open Source Community
SFML Framework (Original)
InterAction Studios

TESTING
Performance Testing
Mobile Device Testing
Cross-Platform Validation
User Experience Testing

VERSION HISTORY
v1.0 - Initial Unity Port
v1.1 - Mobile Optimization
v1.2 - Enhanced Audio
v1.3 - Advanced Effects
v1.4 - Performance Updates
v1.5 - Final Polish

SPECIAL THANKS
To all players who enjoy
classic arcade-style games
and the spirit of
retro gaming

This Unity implementation
pays homage to the original
Chicken Invaders series
while bringing modern
optimizations and features

LEGAL
This is an educational
implementation created
for learning purposes

All original assets and
concepts belong to their
respective owners

COPYRIGHT
Implementation © 2024
Educational Use Only

Thank you for playing!

Press ESC or Back to return
to the main menu";
    
    private bool isScrolling = false;
    private Coroutine autoScrollCoroutine;
    
    public static CreditsUI Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        SetupCredits();
        SetupEventListeners();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    void SetupCredits()
    {
        if (creditsText != null)
        {
            creditsText.text = creditsContent;
        }
        
        // Initially hide credits
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }
    
    void SetupEventListeners()
    {
        if (backButton != null)
            backButton.onClick.AddListener(CloseCredits);
            
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipToEnd);
    }
    
    void HandleInput()
    {
        if (!gameObject.activeInHierarchy) return;
        
        // Handle keyboard input
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            CloseCredits();
        }
        
        // Handle scroll input
        if (creditsScrollRect != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                StopAutoScroll();
                Vector2 scrollPos = creditsScrollRect.normalizedPosition;
                scrollPos.y += scroll * 0.1f;
                scrollPos.y = Mathf.Clamp01(scrollPos.y);
                creditsScrollRect.normalizedPosition = scrollPos;
            }
            
            // Manual scrolling with arrow keys
            if (Input.GetKey(KeyCode.UpArrow))
            {
                StopAutoScroll();
                Vector2 scrollPos = creditsScrollRect.normalizedPosition;
                scrollPos.y += Time.unscaledDeltaTime * 0.5f;
                scrollPos.y = Mathf.Clamp01(scrollPos.y);
                creditsScrollRect.normalizedPosition = scrollPos;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                StopAutoScroll();
                Vector2 scrollPos = creditsScrollRect.normalizedPosition;
                scrollPos.y -= Time.unscaledDeltaTime * 0.5f;
                scrollPos.y = Mathf.Clamp01(scrollPos.y);
                creditsScrollRect.normalizedPosition = scrollPos;
            }
        }
    }
    
    public void ShowCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
            
        // Reset scroll position to bottom
        if (creditsScrollRect != null)
        {
            creditsScrollRect.normalizedPosition = new Vector2(0, 0);
        }
        
        // Start auto scroll after delay
        if (autoScroll)
        {
            StartAutoScroll();
        }
        
        // Play transition sound
        if (AdvancedAudioManager.Instance != null)
        {
            AdvancedAudioManager.Instance.PlayMenuTransition();
        }
    }
    
    void StartAutoScroll()
    {
        if (autoScrollCoroutine != null)
            StopCoroutine(autoScrollCoroutine);
            
        autoScrollCoroutine = StartCoroutine(AutoScrollCoroutine());
    }
    
    void StopAutoScroll()
    {
        if (autoScrollCoroutine != null)
        {
            StopCoroutine(autoScrollCoroutine);
            autoScrollCoroutine = null;
        }
        isScrolling = false;
    }
    
    IEnumerator AutoScrollCoroutine()
    {
        // Wait for initial delay
        yield return new WaitForSecondsRealtime(autoScrollDelay);
        
        isScrolling = true;
        
        if (creditsScrollRect != null)
        {
            // Calculate scroll duration based on content length
            float contentHeight = creditsText != null ? creditsText.preferredHeight : 1000f;
            float viewportHeight = creditsScrollRect.viewport.rect.height;
            float scrollDistance = contentHeight - viewportHeight;
            float scrollDuration = scrollDistance / scrollSpeed;
            
            float elapsed = 0f;
            Vector2 startPos = creditsScrollRect.normalizedPosition;
            Vector2 endPos = new Vector2(0, 1); // Scroll to top
            
            while (elapsed < scrollDuration && isScrolling)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / scrollDuration;
                
                Vector2 currentPos = Vector2.Lerp(startPos, endPos, progress);
                creditsScrollRect.normalizedPosition = currentPos;
                
                yield return null;
            }
            
            // Hold at the end for a moment
            if (isScrolling)
            {
                yield return new WaitForSecondsRealtime(3f);
                
                // Auto close or loop back
                if (isScrolling)
                {
                    // Loop back to beginning
                    creditsScrollRect.normalizedPosition = new Vector2(0, 0);
                    StartAutoScroll(); // Restart the scroll
                }
            }
        }
    }
    
    void CloseCredits()
    {
        StopAutoScroll();
        
        if (AdvancedAudioManager.Instance != null)
        {
            AdvancedAudioManager.Instance.PlayButtonClick();
        }
        
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
            
        // Notify main menu or return to previous screen
        if (MainMenuUI.Instance != null)
        {
            // MainMenuUI would handle returning to main menu
        }
    }
    
    void SkipToEnd()
    {
        StopAutoScroll();
        
        if (creditsScrollRect != null)
        {
            creditsScrollRect.normalizedPosition = new Vector2(0, 1);
        }
        
        if (AdvancedAudioManager.Instance != null)
        {
            AdvancedAudioManager.Instance.PlayButtonClick();
        }
    }
    
    // Public methods for external control
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
    
    public void SetAutoScroll(bool enable)
    {
        autoScroll = enable;
        
        if (!enable)
            StopAutoScroll();
        else if (gameObject.activeInHierarchy)
            StartAutoScroll();
    }
    
    public void SetCreditsContent(string content)
    {
        creditsContent = content;
        if (creditsText != null)
            creditsText.text = content;
    }
    
    public bool IsCreditsActive()
    {
        return creditsPanel != null && creditsPanel.activeInHierarchy;
    }
    
    private void OnDisable()
    {
        StopAutoScroll();
    }
}