using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enhanced Input Manager with proper two-player support
/// Handles input for both keyboard/mouse and touch controls
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("Touch Controls")]
    public bool enableTouchControls = false;
    public TouchControlsUI touchControlsUI;
    
    [Header("Input Settings")]
    public float touchSensitivity = 2f;
    public float deadZone = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    public static InputManager Instance { get; private set; }
    
    // Input states for both players
    public struct PlayerInput
    {
        public Vector2 movement;
        public bool shootPressed;
        public bool shootHeld;
        public bool specialPressed; // For future features
    }
    
    private PlayerInput player1Input;
    private PlayerInput player2Input;
    
    // Input state tracking
    private bool[] previousShootStates = new bool[2];
    
    private void Awake()
    {
        Instance = this;
        
        // Detect platform and enable touch controls if needed
        DetectPlatform();
    }
    
    void DetectPlatform()
    {
        #if UNITY_ANDROID || UNITY_IOS
            enableTouchControls = true;
        #elif UNITY_STANDALONE || UNITY_WEBGL
            enableTouchControls = false;
        #endif
        
        // Enable touch controls UI if needed
        if (touchControlsUI != null)
            touchControlsUI.gameObject.SetActive(enableTouchControls);
            
        Debug.Log($"InputManager: Touch controls {(enableTouchControls ? "enabled" : "disabled")}");
    }
    
    private void Update()
    {
        UpdatePlayerInputs();
        
        if (showDebugInfo)
        {
            DebugInputs();
        }
    }
    
    void UpdatePlayerInputs()
    {
        if (enableTouchControls)
        {
            UpdateTouchInput();
        }
        else
        {
            UpdateKeyboardInput();
        }
    }
    
    void UpdateKeyboardInput()
    {
        // Player 1 Input (WASD + Space)
        Vector2 p1Move = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) p1Move.y += 1f;
        if (Input.GetKey(KeyCode.S)) p1Move.y -= 1f;
        if (Input.GetKey(KeyCode.A)) p1Move.x -= 1f;
        if (Input.GetKey(KeyCode.D)) p1Move.x += 1f;
        
        bool p1ShootHeld = Input.GetKey(KeyCode.Space);
        bool p1ShootPressed = !previousShootStates[0] && p1ShootHeld;
        previousShootStates[0] = p1ShootHeld;
        
        player1Input = new PlayerInput
        {
            movement = p1Move.normalized,
            shootPressed = p1ShootPressed,
            shootHeld = p1ShootHeld,
            specialPressed = Input.GetKeyDown(KeyCode.LeftShift)
        };
        
        // Player 2 Input (Arrow Keys + RShift)
        Vector2 p2Move = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)) p2Move.y += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) p2Move.y -= 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) p2Move.x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) p2Move.x += 1f;
        
        bool p2ShootHeld = Input.GetKey(KeyCode.RightShift);
        bool p2ShootPressed = !previousShootStates[1] && p2ShootHeld;
        previousShootStates[1] = p2ShootHeld;
        
        player2Input = new PlayerInput
        {
            movement = p2Move.normalized,
            shootPressed = p2ShootPressed,
            shootHeld = p2ShootHeld,
            specialPressed = Input.GetKeyDown(KeyCode.RightControl)
        };
        
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PauseGame();
        }
    }
    
    void UpdateTouchInput()
    {
        // Handle touch input for mobile
        if (touchControlsUI != null)
        {
            Vector2 touchMovement = touchControlsUI.GetMovementInput();
            bool touchShootHeld = touchControlsUI.GetShootHeld();
            bool touchShootPressed = !previousShootStates[0] && touchShootHeld;
            previousShootStates[0] = touchShootHeld;
            
            player1Input = new PlayerInput
            {
                movement = touchMovement,
                shootPressed = touchShootPressed,
                shootHeld = touchShootHeld,
                specialPressed = false
            };
            
            // For touch, only one player is active
            player2Input = new PlayerInput
            {
                movement = Vector2.zero,
                shootPressed = false,
                shootHeld = false,
                specialPressed = false
            };
        }
        
        // Handle touch screen taps for pause (in addition to UI button)
        if (Input.touchCount >= 3) // Three finger tap to pause
        {
            bool allTouchesStarted = true;
            for (int i = 0; i < Input.touchCount && i < 3; i++)
            {
                if (Input.GetTouch(i).phase != TouchPhase.Began)
                {
                    allTouchesStarted = false;
                    break;
                }
            }
            
            if (allTouchesStarted && GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }
    }
    
    public PlayerInput GetPlayerInput(int playerIndex)
    {
        return playerIndex == 0 ? player1Input : player2Input;
    }
    
    public bool IsUsingTouchControls()
    {
        return enableTouchControls;
    }
    
    public void SetTouchControls(bool enabled)
    {
        enableTouchControls = enabled;
        
        if (touchControlsUI != null)
            touchControlsUI.gameObject.SetActive(enableTouchControls);
    }
    
    void DebugInputs()
    {
        if (player1Input.movement != Vector2.zero || player1Input.shootHeld)
        {
            Debug.Log($"P1: Move={player1Input.movement}, Shoot={player1Input.shootHeld}");
        }
        
        if (player2Input.movement != Vector2.zero || player2Input.shootHeld)
        {
            Debug.Log($"P2: Move={player2Input.movement}, Shoot={player2Input.shootHeld}");
        }
    }
    
    // Utility methods
    public bool AnyPlayerInput()
    {
        return player1Input.movement != Vector2.zero || player1Input.shootPressed ||
               player2Input.movement != Vector2.zero || player2Input.shootPressed;
    }
    
    public void EnableDebug(bool enable)
    {
        showDebugInfo = enable;
    }
}