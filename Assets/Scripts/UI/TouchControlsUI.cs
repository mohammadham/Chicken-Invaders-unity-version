using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Touch controls UI for mobile devices
/// </summary>
public class TouchControlsUI : MonoBehaviour
{
    [Header("Touch Controls")]
    public RectTransform movementJoystick;
    public RectTransform joystickHandle;
    public Button shootButton;
    
    [Header("Settings")]
    public float joystickRange = 50f;
    
    private Vector2 movementInput = Vector2.zero;
    private bool shootPressed = false;
    private bool shootHeld = false;
    
    // Touch tracking
    private int movementTouchId = -1;
    private Vector2 joystickCenter;
    
    private void Start()
    {
        SetupTouchControls();
    }
    
    void SetupTouchControls()
    {
        if (movementJoystick != null)
        {
            joystickCenter = movementJoystick.position;
        }
        
        if (shootButton != null)
        {
            // Setup shoot button events
            EventTrigger trigger = shootButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = shootButton.gameObject.AddComponent<EventTrigger>();
            
            // Pointer down
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnShootButtonDown(); });
            trigger.triggers.Add(pointerDown);
            
            // Pointer up
            EventTrigger.Entry pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((data) => { OnShootButtonUp(); });
            trigger.triggers.Add(pointerUp);
        }
    }
    
    private void Update()
    {
        HandleTouchInput();
        
        // Reset shoot pressed after frame
        if (shootPressed)
            shootPressed = false;
    }
    
    void HandleTouchInput()
    {
        // Handle joystick touch
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
            
            if (touch.phase == TouchPhase.Began)
            {
                // Check if touch is in joystick area
                if (movementTouchId == -1 && IsInJoystickArea(touch.position))
                {
                    movementTouchId = touch.fingerId;
                    UpdateJoystick(touch.position);
                }
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (touch.fingerId == movementTouchId)
                {
                    UpdateJoystick(touch.position);
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == movementTouchId)
                {
                    movementTouchId = -1;
                    ResetJoystick();
                }
            }
        }
    }
    
    bool IsInJoystickArea(Vector2 screenPos)
    {
        if (movementJoystick == null) return false;
        
        Vector2 joystickScreenPos = RectTransformUtility.WorldToScreenPoint(null, movementJoystick.position);
        float distance = Vector2.Distance(screenPos, joystickScreenPos);
        return distance <= joystickRange * 2f; // Allow larger touch area
    }
    
    void UpdateJoystick(Vector2 touchScreenPos)
    {
        if (movementJoystick == null || joystickHandle == null) return;
        
        Vector2 joystickScreenPos = RectTransformUtility.WorldToScreenPoint(null, movementJoystick.position);
        Vector2 direction = (touchScreenPos - joystickScreenPos).normalized;
        float distance = Vector2.Distance(touchScreenPos, joystickScreenPos);
        
        // Clamp distance to joystick range
        distance = Mathf.Min(distance, joystickRange);
        
        // Update handle position
        Vector2 handleLocalPos = direction * distance;
        joystickHandle.anchoredPosition = handleLocalPos;
        
        // Update input
        movementInput = direction * (distance / joystickRange);
    }
    
    void ResetJoystick()
    {
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }
        movementInput = Vector2.zero;
    }
    
    void OnShootButtonDown()
    {
        shootPressed = true;
        shootHeld = true;
    }
    
    void OnShootButtonUp()
    {
        shootHeld = false;
    }
    
    // Public methods for InputManager
    public Vector2 GetMovementInput()
    {
        return movementInput;
    }
    
    public bool GetShootPressed()
    {
        return shootPressed;
    }
    
    public bool GetShootHeld()
    {
        return shootHeld;
    }
}