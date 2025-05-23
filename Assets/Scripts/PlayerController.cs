using System.Collections.Generic;
using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions 
{
    const int SEND_UPDATES_PER_SEC = 20;
    const float SEND_UPDATES_FREQUENCY = 1f / SEND_UPDATES_PER_SEC;

    public static PlayerController Local { get; private set; }

    private uint PlayerId;
    private float LastMovementSendTimestamp;

    private Vector2 LockInputPosition;
    private List<PuppetController> OwnedPuppets = new List<PuppetController>();
    public int NumberOfOwnedPuppet => OwnedPuppets.Count;
    public bool IsLocalPlayer => this == Local;

    // Controller settings
    [Header("Controller Settings")]
    public float rotationSpeed = 120f;
    public float rightStickDeadzone = 0.2f;
    
    // Input System
    private PlayerInputActions playerInputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRightMouseButtonPressed = false;
    
    // Track last used device
    private bool isUsingGamepad = false;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.SetCallbacks(this);
        
        // Subscribe to device change events
        InputSystem.onActionChange += OnInputDeviceChanged;
    }
    
    private void OnInputDeviceChanged(object obj, InputActionChange change)
    {
        // Check if the action is being performed (triggered)
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction action = obj as InputAction;
            if (action != null && action.activeControl != null)
            {
                // Check if the active device is a gamepad
                isUsingGamepad = action.activeControl.device is Gamepad;
//                Debug.Log($"Input device changed. Using gamepad: {isUsingGamepad}");
            }
        }
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
        // Unsubscribe from device change events to prevent memory leaks
        InputSystem.onActionChange -= OnInputDeviceChanged;
    }

    public void Initialize(Player player)
    {
        PlayerId = player.PlayerId;
        if (player.Identity == GameManager.LocalIdentity)
        {
            Local = this;
        }
        GameManager.GetObstacles();
    }
    private void OnDestroy()
    {
        // If we have any puppets, destroy them
        foreach (var puppet in OwnedPuppets)
        {
            if (puppet != null)
            {
                Destroy(puppet.gameObject);
            }
        }
        OwnedPuppets.Clear();
    }

    public void OnPuppetSpawned(PuppetController puppet)
    {
        OwnedPuppets.Add(puppet);
         if ( IsLocalPlayer )
        {
            GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>().Target.TrackingTarget = OwnedPuppets[0].gameObject.transform;
        }
    }
    
    public void OnPuppetDeleted(PuppetController deletedPuppet)
    {
        // This means we got eaten
        if (OwnedPuppets.Remove(deletedPuppet) && IsLocalPlayer && OwnedPuppets.Count == 0)
        {
            // DeathScreen.Instance.SetVisible(true);
        }
    }

    private void OnGUI()
    {
        if (!IsLocalPlayer || !GameManager.IsConnected())
        {
            return;
        }

        //GUI.Label(new Rect(0, 0, 100, 50), $"Total Mass: {TotalMass()}");
    }
    
    // InputSystem callback for movement
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            moveInput = context.ReadValue<Vector2>();
            // Store raw input without zeroing it out each frame
            LockInputPosition = moveInput;
            
            // Update the device type when this input is received
            if (context.control != null)
            {
                isUsingGamepad = context.control.device is Gamepad;
            }
        }
    }

    // InputSystem callback for looking/rotation
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        
        // Update the device type when this input is received
        if (context.control != null)
        {
            isUsingGamepad = context.control.device is Gamepad;
        }
    }

    // InputSystem callback for fire button
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed && IsLocalPlayer && NumberOfOwnedPuppet > 0)
        {
            HandleFireAction();
        }
    }
    
    void HandleFireAction()
    {
        if (!OwnedPuppets[0].Puppet.HasSnowball)
        {
            Debug.Log($"PlayerController: Fire button pressed: {PlayerId}, Crafting snowball");
            GameManager.CraftSnowBall(PlayerId);
        }
        else
        {
            var from = OwnedPuppets[0].transform.position;
            Debug.Log($"PlayerController: Fire button pressed: {PlayerId}, Spawning snowball from {from}");
            GameManager.SpawnSnowBall(PlayerId, new Vector2(from.x, from.z));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer || NumberOfOwnedPuppet == 0)
        {
            return;
        }
        
        // Only check for mouse button if we're not using a gamepad
        if (!isUsingGamepad && Mouse.current != null)
        {
            isRightMouseButtonPressed = Mouse.current.rightButton.isPressed;
        }
        
        HandleRotation();

        // Throttled input requests
        if (Time.time - LastMovementSendTimestamp >= SEND_UPDATES_FREQUENCY)
        {
            LastMovementSendTimestamp = Time.time;
            // Use moveInput directly to avoid losing diagonal input between frames
            var direction = GetMoveDirection(moveInput); 
            GameManager.Conn.Reducers.UpdatePlayerInput(direction, transform.rotation.eulerAngles.y);
            // Don't zero out LockInputPosition here to maintain continuous movement
        }
    }

    void HandleRotation()
    {
        if (isUsingGamepad)
        {
            // We're using a gamepad - use right stick for rotation with deadzone
            if (Mathf.Abs(lookInput.x) > rightStickDeadzone)
            {
                Rotate(lookInput.x * rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // We're using mouse input
            if (isRightMouseButtonPressed && lookInput.x != 0)
            {
                Rotate(lookInput.x * rotationSpeed * Time.deltaTime);
            }
        }
    }

    Vector2 GetMoveDirection(Vector2 direction)
    {
        // Only normalize if the magnitude is > 0 to prevent NaN values
        if (direction.sqrMagnitude > 0)
        {
            // Convert to 3D space and keep normalized
            Vector3 move = new Vector3(direction.x, 0, direction.y);
            // Normalize only if magnitude > 1 to preserve small inputs
            if (move.magnitude > 1f)
                move.Normalize();
            move = transform.TransformDirection(move);
            // Return the XZ components as a Vector2 (worldspace movement)
            return new Vector2(move.x, move.z);
        }
        return Vector2.zero;
    }

    void Rotate(float rotationAmount)
    {
        transform.Rotate(0, rotationAmount, 0);
    }
}
