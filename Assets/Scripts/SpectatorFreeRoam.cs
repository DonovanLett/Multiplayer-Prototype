using UnityEngine;
using UnityEngine.InputSystem; // New Input Code

public class SpectatorFreeRoam : MonoBehaviour
{
    // New Input System
    [Header("Input")]
    [SerializeField]
    private InputActionAsset _inputActions;

    private InputActionMap _spectatorMap;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _fastAction;
    private InputAction _slowAction;
    private InputAction _upAction;
    private InputAction _downAction;
    // New Input System

    [Header("Movement Speed")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float slowSpeed = 2f;
    [SerializeField] private float fastSpeed = 12f;

    [Header("Camera Rotation")]
    [SerializeField] private float rotationSpeed = 2f;

    
    // New Input System
    private void Awake()
    {
        InitializeInput();
    }

    private void InitializeInput()
    {
        if (_inputActions == null)
        {
            Debug.LogError(
                $"SpectatorFreeRoam on {gameObject.name}: " +
                "No Input Action Asset has been assigned."
            );

            return;
        }

        _spectatorMap =
            _inputActions.FindActionMap("Spectator");

        if (_spectatorMap == null)
        {
            Debug.LogError(
                "SpectatorFreeRoam: " +
                "Could not find the 'Spectator' Action Map."
            );

            return;
        }

        _moveAction =
            _spectatorMap.FindAction("Move");

        _lookAction =
            _spectatorMap.FindAction("Look");

        _fastAction =
            _spectatorMap.FindAction("Fast");

        _slowAction =
            _spectatorMap.FindAction("Slow");

        _upAction =
            _spectatorMap.FindAction("Up");

        _downAction =
            _spectatorMap.FindAction("Down");

        if (_moveAction == null)
            Debug.LogError(
                "SpectatorFreeRoam: 'Move' action not found."
            );

        if (_lookAction == null)
            Debug.LogError(
                "SpectatorFreeRoam: 'Look' action not found."
            );

        if (_fastAction == null)
            Debug.LogError(
                "SpectatorFreeRoam: 'Fast' action not found."
            );

        if (_slowAction == null)
            Debug.LogError(
                "SpectatorFreeRoam: 'Slow' action not found."
            );

        if (_upAction == null)
            Debug.LogError(
                "SpectatorFreeRoam: 'Up' action not found."
            );

        if (_downAction == null)
            Debug.LogError(
                "SpectatorFreeRoam: 'Down' action not found."
            );

        InputManager.Instance.EnterSpectatorMode(); // Added
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (_moveAction == null)
            return;

        float currentSpeed = normalSpeed;

        if (_fastAction != null &&
            _fastAction.IsPressed())
        {
            currentSpeed = fastSpeed;
        }
        else if (_slowAction != null &&
                 _slowAction.IsPressed())
        {
            currentSpeed = slowSpeed;
        }

        Vector2 input =
            _moveAction.ReadValue<Vector2>();

        Vector3 movement =
            transform.right * input.x +
            transform.forward * input.y;

        if (_upAction != null &&
            _upAction.IsPressed())
        {
            movement += Vector3.up;
        }

        if (_downAction != null &&
            _downAction.IsPressed())
        {
            movement += Vector3.down;
        }

        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        transform.position +=
            movement *
            currentSpeed *
            Time.deltaTime;
    }

    private void HandleRotation()
    {
        if (_lookAction == null)
            return;

        Vector2 look =
            _lookAction.ReadValue<Vector2>();

        transform.Rotate(
            Vector3.up,
            look.x * rotationSpeed,
            Space.World
        );

        transform.Rotate(
            Vector3.left,
            look.y * rotationSpeed,
            Space.Self
        );
    }
    // New Input System
    




    /*
    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        // Determine movement speed
        float currentSpeed = normalSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = fastSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            currentSpeed = slowSpeed;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A))
            horizontal = -1f;

        if (Input.GetKey(KeyCode.D))
            horizontal = 1f;

        if (Input.GetKey(KeyCode.W))
            vertical = 1f;

        if (Input.GetKey(KeyCode.S))
            vertical = -1f;

        // Movement relative to the camera's orientation
        Vector3 movement =
            transform.right * horizontal +
            transform.forward * vertical;

        // Vertical movement
        if (Input.GetKey(KeyCode.Space))
        {
            movement += Vector3.up;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            movement += Vector3.down;
        }

        // Normalize so diagonal movement isn't faster
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        // Apply movement
        transform.position += movement * currentSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Horizontal rotation
        transform.Rotate(
            Vector3.up,
            mouseX * rotationSpeed,
            Space.World
        );

        // Vertical rotation
        transform.Rotate(
            Vector3.left,
            mouseY * rotationSpeed,
            Space.Self
        );
    }
    */
    
}/////
