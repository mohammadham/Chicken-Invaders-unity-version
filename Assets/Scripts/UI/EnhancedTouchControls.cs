using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Enhanced Touch Controls with advanced features
/// Virtual joystick, pressure-sensitive buttons, haptic feedback, multi-touch support
/// </summary>
public class EnhancedTouchControls : MonoBehaviour
{
    [Header("Touch Control Elements")]
    public RectTransform movementJoystick;
    public RectTransform joystickKnob;
    public Button shootButton;
    public Button pauseButton;
    public Button weaponSwapButton;
    
    [Header("Joystick Settings")]
    public float joystickRange = 100f;
    public float deadZone = 0.1f;
    public bool dynamicJoystick = true;
    public AnimationCurve responseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Button Settings")]
    public bool pressureSensitive = true;
    public float maxPressure = 1f;
    public AnimationCurve pressureCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Header("Haptic Feedback")]
    public bool enableHaptics = true;
    public float lightHapticDuration = 0.1f;
    public float mediumHapticDuration = 0.2f;
    public float heavyHapticDuration = 0.3f;
    
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.yellow;
    public Color activeColor = Color.green;
    public float fadeDuration = 0.2f;
    
    [Header("Safe Area")]
    public bool respectSafeArea = true;
    public float safeAreaMargin = 20f;
    
    // Touch tracking
    private Dictionary<int, TouchInfo> activeTouches = new Dictionary<int, TouchInfo>();
    private int joystickTouchId = -1;
    private int shootTouchId = -1;
    
    // Input values
    private Vector2 movementInput = Vector2.zero;
    private bool shootPressed = false;
    private bool shootHeld = false;
    private float shootPressure = 0f;
    
    // UI positions
    private Vector2 joystickCenter;
    private Vector2 originalJoystickPosition;
    private RectTransform canvasRect;
    
    // Visual effects
    private Coroutine joystickFadeCoroutine;
    private Coroutine shootButtonFadeCoroutine;
    
    public static EnhancedTouchControls Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }
    
    private void Start()
    {
        InitializeTouchControls();
        SetupSafeArea();
        SetupEventListeners();
    }
    
    void InitializeTouchControls()
    {
        // Store original positions
        if (movementJoystick != null)
        {
            originalJoystickPosition = movementJoystick.anchoredPosition;
            joystickCenter = originalJoystickPosition;
        }
        
        // Initialize visual states
        SetJoystickAlpha(0.7f);
        SetButtonColor(shootButton, normalColor);
        
        // Enable haptics if supported
        #if UNITY_ANDROID || UNITY_IOS
        enableHaptics = enableHaptics && SystemInfo.supportsVibration;
        #else
        enableHaptics = false;
        #endif
    }
    
    void SetupSafeArea()
    {
        if (!respectSafeArea) return;
        
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        // Adjust control positions based on safe area
        AdjustControlsForSafeArea(anchorMin, anchorMax);
    }
    
    void AdjustControlsForSafeArea(Vector2 anchorMin, Vector2 anchorMax)
    {
        // Move joystick away from unsafe areas
        if (movementJoystick != null)
        {
            Vector2 pos = movementJoystick.anchoredPosition;
            float leftMargin = anchorMin.x * Screen.width + safeAreaMargin;
            float bottomMargin = anchorMin.y * Screen.height + safeAreaMargin;
            
            if (pos.x < leftMargin)
                pos.x = leftMargin;
            if (pos.y < bottomMargin)
                pos.y = bottomMargin;
                
            movementJoystick.anchoredPosition = pos;
            originalJoystickPosition = pos;
        }
        
        // Move buttons away from unsafe areas
        if (shootButton != null)
        {
            RectTransform buttonRect = shootButton.GetComponent<RectTransform>();
            Vector2 pos = buttonRect.anchoredPosition;
            float rightMargin = (1f - anchorMax.x) * Screen.width + safeAreaMargin;
            float bottomMargin = anchorMin.y * Screen.height + safeAreaMargin;
            
            if (pos.x > Screen.width - rightMargin)
                pos.x = Screen.width - rightMargin;
            if (pos.y < bottomMargin)
                pos.y = bottomMargin;
                
            buttonRect.anchoredPosition = pos;
        }
    }
    
    void SetupEventListeners()
    {
        // Setup shoot button events
        if (shootButton != null)
        {
            EventTrigger trigger = shootButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = shootButton.gameObject.AddComponent<EventTrigger>();
            
            // Pointer down
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnShootButtonDown((PointerEventData)data); });
            trigger.triggers.Add(pointerDown);
            
            // Pointer up
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { OnShootButtonUp((PointerEventData)data); });
            trigger.triggers.Add(pointerUp);
            
            // Pointer exit (for when finger slides off)
            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { OnShootButtonUp((PointerEventData)data); });
            trigger.triggers.Add(pointerExit);
        }
        
        // Setup other buttons
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseButtonPressed);
            
        if (weaponSwapButton != null)
            weaponSwapButton.onClick.AddListener(OnWeaponSwapPressed);
    }
    
    private void Update()
    {
        HandleTouchInput();
        UpdateVisualFeedback();
        
        // Reset one-frame inputs
        shootPressed = false;
    }
    
    void HandleTouchInput()
    {
        // Handle all active touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            ProcessTouch(touch);
        }
        
        // Clean up ended touches
        List<int> touchesToRemove = new List<int>();
        foreach (var kvp in activeTouches)
        {
            bool touchStillActive = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == kvp.Key)
                {
                    touchStillActive = true;
                    break;
                }
            }
            
            if (!touchStillActive)
                touchesToRemove.Add(kvp.Key);
        }
        
        foreach (int touchId in touchesToRemove)
        {
            OnTouchEnded(touchId);
        }
    }
    
    void ProcessTouch(Touch touch)
    {
        Vector2 touchScreenPos = touch.position;
        
        switch (touch.phase)
        {
            case TouchPhase.Began:
                OnTouchBegan(touch.fingerId, touchScreenPos, touch.pressure);
                break;
                
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                OnTouchMoved(touch.fingerId, touchScreenPos, touch.pressure);
                break;
                
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                OnTouchEnded(touch.fingerId);
                break;
        }
    }
    
    void OnTouchBegan(int touchId, Vector2 screenPos, float pressure)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out localPos);
        
        TouchInfo touchInfo = new TouchInfo
        {
            fingerId = touchId,
            startPosition = localPos,
            currentPosition = localPos,
            pressure = pressure,
            startTime = Time.time
        };
        
        activeTouches[touchId] = touchInfo;
        
        // Check if touch is on joystick area
        if (joystickTouchId == -1 && IsPositionInJoystickArea(screenPos))
        {
            StartJoystickTouch(touchId, screenPos);
        }
        // Check if touch is on shoot button
        else if (shootTouchId == -1 && IsPositionOnButton(screenPos, shootButton))
        {
            StartShootTouch(touchId, pressure);
        }
    }
    
    void OnTouchMoved(int touchId, Vector2 screenPos, float pressure)
    {
        if (!activeTouches.ContainsKey(touchId)) return;
        
        TouchInfo touchInfo = activeTouches[touchId];
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out localPos);
        touchInfo.currentPosition = localPos;
        touchInfo.pressure = pressure;
        activeTouches[touchId] = touchInfo;
        
        // Update joystick
        if (touchId == joystickTouchId)
        {
            UpdateJoystick(screenPos);
        }
        // Update shoot pressure
        else if (touchId == shootTouchId && pressureSensitive)
        {
            UpdateShootPressure(pressure);
        }
    }
    
    void OnTouchEnded(int touchId)
    {
        if (!activeTouches.ContainsKey(touchId)) return;
        
        if (touchId == joystickTouchId)
        {
            EndJoystickTouch();
        }
        else if (touchId == shootTouchId)
        {
            EndShootTouch();
        }
        
        activeTouches.Remove(touchId);
    }
    
    void StartJoystickTouch(int touchId, Vector2 screenPos)
    {
        joystickTouchId = touchId;
        
        if (dynamicJoystick)
        {
            // Move joystick to touch position
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out localPos);
            movementJoystick.anchoredPosition = localPos;
            joystickCenter = localPos;
        }
        
        UpdateJoystick(screenPos);
        SetJoystickAlpha(1f);
        
        // Light haptic feedback
        TriggerHapticFeedback(HapticFeedbackType.Light);
    }
    
    void UpdateJoystick(Vector2 screenPos)
    {
        if (movementJoystick == null || joystickKnob == null) return;
        
        Vector2 joystickScreenPos;
        RectTransformUtility.WorldToScreenPoint(null, movementJoystick.position, out joystickScreenPos);
        
        Vector2 direction = (screenPos - joystickScreenPos).normalized;
        float distance = Vector2.Distance(screenPos, joystickScreenPos);
        
        // Clamp distance to joystick range
        distance = Mathf.Min(distance, joystickRange);
        
        // Apply response curve for more natural feel
        float normalizedDistance = distance / joystickRange;
        float curvedDistance = responseCurve.Evaluate(normalizedDistance);
        
        // Update knob position
        Vector2 knobLocalPos = direction * (curvedDistance * joystickRange);
        joystickKnob.anchoredPosition = knobLocalPos;
        
        // Calculate input
        Vector2 input = direction * curvedDistance;
        
        // Apply dead zone
        if (input.magnitude < deadZone)
            input = Vector2.zero;
        else
            input = (input - input.normalized * deadZone) / (1f - deadZone);
        
        movementInput = input;
    }
    
    void EndJoystickTouch()
    {
        joystickTouchId = -1;
        movementInput = Vector2.zero;
        
        // Return knob to center
        if (joystickKnob != null)
        {
            StartCoroutine(AnimateKnobToCenter());
        }
        
        // Return joystick to original position if dynamic
        if (dynamicJoystick)
        {
            StartCoroutine(AnimateJoystickToOriginalPosition());
        }
        
        SetJoystickAlpha(0.7f);
    }
    
    IEnumerator AnimateKnobToCenter()
    {
        Vector2 startPos = joystickKnob.anchoredPosition;
        Vector2 targetPos = Vector2.zero;
        
        float elapsed = 0f;
        float duration = 0.2f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            joystickKnob.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
            yield return null;
        }
        
        joystickKnob.anchoredPosition = targetPos;
    }
    
    IEnumerator AnimateJoystickToOriginalPosition()
    {
        Vector2 startPos = movementJoystick.anchoredPosition;
        Vector2 targetPos = originalJoystickPosition;
        
        float elapsed = 0f;
        float duration = 0.3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / duration;
            movementJoystick.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
            joystickCenter = movementJoystick.anchoredPosition;
            yield return null;
        }
        
        movementJoystick.anchoredPosition = targetPos;
        joystickCenter = targetPos;
    }
    
    void StartShootTouch(int touchId, float pressure)
    {
        shootTouchId = touchId;
        shootPressed = true;
        shootHeld = true;
        
        if (pressureSensitive)
        {
            shootPressure = pressureCurve.Evaluate(pressure / maxPressure);
        }
        else
        {
            shootPressure = 1f;
        }
        
        SetButtonColor(shootButton, pressedColor);
        
        // Medium haptic feedback
        TriggerHapticFeedback(HapticFeedbackType.Medium);
    }
    
    void UpdateShootPressure(float pressure)
    {
        if (pressureSensitive)
        {
            shootPressure = pressureCurve.Evaluate(pressure / maxPressure);
        }
    }
    
    void EndShootTouch()
    {
        shootTouchId = -1;
        shootHeld = false;
        shootPressure = 0f;
        
        SetButtonColor(shootButton, normalColor);
        
        // Light haptic feedback
        TriggerHapticFeedback(HapticFeedbackType.Light);
    }
    
    // Button event handlers
    void OnShootButtonDown(PointerEventData eventData)
    {
        if (shootTouchId == -1)
        {
            StartShootTouch(eventData.pointerId, eventData.pressure);
        }
    }
    
    void OnShootButtonUp(PointerEventData eventData)
    {
        if (shootTouchId == eventData.pointerId)
        {
            EndShootTouch();
        }
    }
    
    void OnPauseButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PauseGame();
            
        TriggerHapticFeedback(HapticFeedbackType.Medium);
    }
    
    void OnWeaponSwapPressed()
    {
        // Implement weapon swapping
        Debug.Log("Weapon swap requested");
        
        TriggerHapticFeedback(HapticFeedbackType.Light);
    }
    
    // Utility methods
    bool IsPositionInJoystickArea(Vector2 screenPos)
    {
        if (movementJoystick == null) return false;
        
        Vector2 joystickScreenPos;
        RectTransformUtility.WorldToScreenPoint(null, movementJoystick.position, out joystickScreenPos);
        
        float distance = Vector2.Distance(screenPos, joystickScreenPos);
        return distance <= joystickRange * 1.5f; // Allow larger touch area
    }
    
    bool IsPositionOnButton(Vector2 screenPos, Button button)
    {
        if (button == null) return false;
        
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        Vector2 localPoint;
        
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            buttonRect, screenPos, null, out localPoint) && 
            buttonRect.rect.Contains(localPoint);
    }
    
    void UpdateVisualFeedback()
    {
        // Update joystick visual feedback based on input strength
        if (joystickKnob != null && movementInput.magnitude > 0)
        {
            float intensity = movementInput.magnitude;
            Color knobColor = Color.Lerp(normalColor, activeColor, intensity);
            
            Image knobImage = joystickKnob.GetComponent<Image>();
            if (knobImage != null)
                knobImage.color = knobColor;
        }
    }
    
    void SetJoystickAlpha(float alpha)
    {
        if (movementJoystick != null)
        {
            CanvasGroup canvasGroup = movementJoystick.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = movementJoystick.gameObject.AddComponent<CanvasGroup>();
                
            if (joystickFadeCoroutine != null)
                StopCoroutine(joystickFadeCoroutine);
                
            joystickFadeCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup, alpha));
        }
    }
    
    void SetButtonColor(Button button, Color color)
    {
        if (button != null)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (shootButtonFadeCoroutine != null)
                    StopCoroutine(shootButtonFadeCoroutine);
                    
                shootButtonFadeCoroutine = StartCoroutine(FadeButtonColor(buttonImage, color));
            }
        }
    }
    
    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
    IEnumerator FadeButtonColor(Image buttonImage, Color targetColor)
    {
        Color startColor = buttonImage.color;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / fadeDuration;
            buttonImage.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        
        buttonImage.color = targetColor;
    }
    
    void TriggerHapticFeedback(HapticFeedbackType type)
    {
        if (!enableHaptics) return;
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Android haptic feedback
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            
            switch (type)
            {
                case HapticFeedbackType.Light:
                    vibrator.Call("vibrate", (long)(lightHapticDuration * 1000));
                    break;
                case HapticFeedbackType.Medium:
                    vibrator.Call("vibrate", (long)(mediumHapticDuration * 1000));
                    break;
                case HapticFeedbackType.Heavy:
                    vibrator.Call("vibrate", (long)(heavyHapticDuration * 1000));
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Haptic feedback failed: " + e.Message);
        }
        #elif UNITY_IOS && !UNITY_EDITOR
        // iOS haptic feedback would require native plugin
        Handheld.Vibrate();
        #else
        // Fallback for other platforms
        Debug.Log($"Haptic feedback: {type}");
        #endif
    }
    
    // Public interface
    public Vector2 GetMovementInput() => movementInput;
    public bool GetShootPressed() => shootPressed;
    public bool GetShootHeld() => shootHeld;
    public float GetShootPressure() => shootPressure;
    
    public void SetControlsVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    public void RecalibrateControls()
    {
        StartCoroutine(RecalibrationRoutine());
    }
    
    IEnumerator RecalibrationRoutine()
    {
        // Show calibration UI
        Debug.Log("Touch controls recalibration started...");
        
        // Reset positions
        if (movementJoystick != null)
        {
            movementJoystick.anchoredPosition = originalJoystickPosition;
            joystickCenter = originalJoystickPosition;
        }
        
        yield return new WaitForSecondsRealtime(1f);
        
        Debug.Log("Touch controls recalibration completed!");
    }
}

/// <summary>
/// Touch information structure
/// </summary>
public struct TouchInfo
{
    public int fingerId;
    public Vector2 startPosition;
    public Vector2 currentPosition;
    public float pressure;
    public float startTime;
}

/// <summary>
/// Haptic feedback types
/// </summary>
public enum HapticFeedbackType
{
    Light,
    Medium,
    Heavy
}